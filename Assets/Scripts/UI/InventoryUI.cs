using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StarterAssets;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public enum InventoryUIState { ItemSelection, PartySelection, AbilityToForget, Busy }

public class InventoryUI : MonoBehaviour
{
    [Header("Item list")]
    [SerializeField] GameObject itemList;

    [Header("UI")]
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] TextMeshProUGUI categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [TextArea]
    [SerializeField] TextMeshProUGUI itemDescription;

    [Header("Screens")]
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] AbilitySelectionUI abilityScreen;
    Action<ItemBase> onItemUsed;

    RectTransform itemListRect;

    int selectedItem = 0;
    int selectedCategory = 0;
    AbilitiesBase abilityToLearn;

    InventoryUIState state;
    List<ItemSlotUI> slotUIList;
    Inventory inventory;
    
    bool learned = false;

    private void Awake() {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start() {
        UpdateItemList();

        inventory.OnUpdated += UpdateItemList;
    }

    void UpdateItemList() {
        // Clear all existing items
        foreach (Transform child in itemList.transform) {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.GetSlotByCategory(selectedCategory)) {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed=null) {
        // cache
        this.onItemUsed = onItemUsed;

        if (state == InventoryUIState.ItemSelection) {
            int prevSelection = selectedItem;
            int prevCategory = selectedCategory;

            if (Input.GetKeyDown(KeyCode.DownArrow)) {
                ++selectedItem;
            } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
                --selectedItem;
            } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
                ++selectedCategory;
            } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                --selectedCategory;
            }

            if (selectedCategory > Inventory.ItemCategories.Count - 1) { 
                selectedCategory = 0;
            } else if (selectedCategory < 0) {
                selectedCategory = Inventory.ItemCategories.Count - 1;
            }

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotByCategory(selectedCategory).Count - 1);         

            if (prevCategory != selectedCategory) {
                ResetSelection();
                categoryText.text = Inventory.ItemCategories[selectedCategory];
                UpdateItemList();
            } else if (prevSelection != selectedItem){
                UpdateItemSelection();
            }


            if (Input.GetKeyDown(KeyCode.Z)) {
                StartCoroutine(ItemSelected());
            }else if (Input.GetKeyDown(KeyCode.X)) {
                onBack?.Invoke();
            }
        } else if (state == InventoryUIState.PartySelection){
            // Handle party selection
            Action onSelected = () => {
                // Use the item on the selected Yokai
                StartCoroutine(UseItem());
            };

            Action onBackPartyScreen = () => {
                ClosePartyScreen();
            };
            partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
        } else if (state == InventoryUIState.AbilityToForget) {
            Action<int> onAbilitySelected = (int abilityIndex) => {
                StartCoroutine(OnAbilityToForgetSelected(abilityIndex)); 
            };

            abilityScreen.HandleAbilitySelection(onAbilitySelected);
        }
    }

    IEnumerator ItemSelected() {
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selectedItem, selectedCategory);

        if (GameControler.Instance.State == GameState.Battle) {
            // In battle
            if (!item.CanUseInBattle) {
                yield return DialogManager.Instance.ShowDialogText($"This item cannot be used in battle!");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        } else {
            // Out of battle
            if (!item.CanUseOutsideBattle) {
                yield return DialogManager.Instance.ShowDialogText($"This item cannot be used outside of battle!");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }

        if (selectedCategory == (int)ItemCategory.YokaiBalls) {
            StartCoroutine(UseItem());
        } else {
            OpenPartyScreen();

            if (item is TmItem)
            {
                // Show if TM is useable
                partyScreen.ShowIfTmIsUsable(item as TmItem);
            }
        }
    }

    IEnumerator UseItem() {
        state = InventoryUIState.Busy;

        yield return HandleTmItems();

        

        var item = inventory.GetItem(selectedItem, selectedCategory);
        var yokai = partyScreen.SelectedMemeber;

        

        // Handle yokai Evolution items.
        if (item is EvolutionItem)
        {
            var evolution = yokai.CheckForEvolution(item);
            if (evolution != null)
            {
                yield return EvolutionManager.i.Evolve(yokai, evolution);
            } else
            {
                yield return DialogManager.Instance.ShowDialogText($"It will not have any affect");
                ClosePartyScreen();
                yield break;
            }
        }

        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMemeber, selectedCategory);

        // ensure that the tm is not removed from inventory if yokai cannot learn it.
        if (usedItem != null && learned == false && usedItem is TmItem)
        {
            ClosePartyScreen();
            yield break;
        }

        if (usedItem != null) {
            if (usedItem is RecoveryItem)
                yield return DialogManager.Instance.ShowDialogText($"The player used {usedItem.Name}");

            onItemUsed?.Invoke(usedItem);
        } else {
            if (selectedCategory == (int)ItemCategory.Items)
                yield return DialogManager.Instance.ShowDialogText($"It will not have any affect");
        }

        learned = false;
        ClosePartyScreen();
    }

    IEnumerator HandleTmItems() {
        var tmItem = inventory.GetItem(selectedItem, selectedCategory) as TmItem;
        if (tmItem == null) {
            yield break;
        }

        var yokai = partyScreen.SelectedMemeber;

        if (yokai.HasAbility(tmItem.Ability)) {
            yield return DialogManager.Instance.ShowDialogText($"{yokai.Base.Name} already knows {tmItem.Ability.Name}");
            yield break;    
        }

        if (!tmItem.CanBeTaught(yokai)) {
            yield return DialogManager.Instance.ShowDialogText($"{yokai.Base.Name} cannot learn {tmItem.Ability.Name}");
            yield break;
        }

        if (yokai.Abilities.Count < YokaiBase.MaxNumAbilities) {
            yokai.LearnAbility(tmItem.Ability);
            learned = true;
            yield return DialogManager.Instance.ShowDialogText($"{yokai.Base.Name} learned {tmItem.Ability.Name}");
        } else {
            yield return DialogManager.Instance.ShowDialogText($"{yokai.Base.Name} is trying to learn {tmItem.Ability.Name}");
            yield return DialogManager.Instance.ShowDialogText($"It can not learn more than {YokaiBase.MaxNumAbilities}!");
            yield return ChooseAbilityToForget(yokai, tmItem.Ability);
            learned = true;
            yield return new WaitUntil(() => state != InventoryUIState.AbilityToForget);
        }
    }

    IEnumerator ChooseAbilityToForget(Yokai yokai, AbilitiesBase newAbility) {
        state = InventoryUIState.Busy;
        yield return DialogManager.Instance.ShowDialogText($"Choose an ability you want to forget.", true, false);
        abilityScreen.gameObject.SetActive(true);
        abilityScreen.SetAbiliyData(yokai.Abilities.Select(x => x.Base).ToList(), newAbility);
        abilityToLearn = newAbility;
        state = InventoryUIState.AbilityToForget;
    }

    void UpdateItemSelection() {
        var slots = inventory.GetSlotByCategory(selectedCategory);

        // fixes issue when there is one item left and it is used.
        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);

        for (int i = 0; i < slotUIList.Count; i++) {
            if (i == selectedItem) {
                slotUIList[i].NameText.color = GlobalSetting.i.HighlightedColor;
            } else {
                slotUIList[i].NameText.color = Color.black;
            }
        }

        if (slots.Count > 0) {
            var item = slots[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }
        
        HandleScrolling();
    }

    void HandleScrolling() {
        if (slotUIList.Count <= 15) return;

        float scrollPos = Mathf.Clamp(selectedItem - 7, 0, selectedItem) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > 7;
        upArrow.gameObject.SetActive(showUpArrow);
        bool showDownArrow = selectedItem + 7 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }

    void ResetSelection() {
        selectedItem = 0;
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    void OpenPartyScreen() {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen() {
        state = InventoryUIState.ItemSelection;

        partyScreen.ClearTmUsableMessage();
        partyScreen.gameObject.SetActive(false);
    }

    IEnumerator OnAbilityToForgetSelected(int abilityIndex) {
        var yokai = partyScreen.SelectedMemeber;

        DialogManager.Instance.CloseDialog();
        abilityScreen.gameObject.SetActive(false);
            if (abilityIndex == YokaiBase.MaxNumAbilities) {
                // Don't learn new Ability
                yield return (DialogManager.Instance.ShowDialogText($"{yokai.Base.Name} did not learn {abilityToLearn.Name}."));
            } else {
                // Forget the selected Ability
                var selectedAbiliy = yokai.Abilities[abilityIndex].Base;
                yield return (DialogManager.Instance.ShowDialogText($"{yokai.Base.Name} forgot {selectedAbiliy.Name} and learned {abilityToLearn.Name}."));

                yokai.Abilities[abilityIndex] = new Ability(abilityToLearn);
            }
            
            abilityToLearn = null;
            state = InventoryUIState.ItemSelection;
    }
}
