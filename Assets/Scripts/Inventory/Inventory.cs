using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StarterAssets;
using System.Linq;
using JetBrains.Annotations;

// remember to keep in same order of list
public enum ItemCategory { Items, YokaiBalls, TMs } 

public class Inventory : MonoBehaviour, ISavable
{
    [Header("Item Categories and Slots")]
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> yokaiBallSlots;
    [SerializeField] List<ItemSlot> tmSlots;

    List<List<ItemSlot>> allSlots;
    public event Action OnUpdated;

    private void Awake() {
        allSlots = new List<List<ItemSlot>>() { slots, yokaiBallSlots, tmSlots };
    }

    public static List<string> ItemCategories { get; set; } = new List<string>() {
        "ITEMS", "YOKAIBALLS", "TMs & HMs"
    }; 

    public List<ItemSlot> GetSlotByCategory(int categoryIndex) {
        return allSlots[categoryIndex];
    }

    public ItemBase GetItem(int itemIndex, int categoryIndex) {
         var currentSlots = GetSlotByCategory(categoryIndex);
         return currentSlots[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Yokai selectedYokai, int selectedCategory) {
        var item = GetItem(itemIndex, selectedCategory);
        bool itemUsed = item.Use(selectedYokai);
        if (itemUsed) {
            if (!item.IsReusable){
                RemoveItem(item, selectedCategory);
            }
            return item;
        }
        return null;
    }

    public void RemoveItem(ItemBase item, int selectedCategory) {
        var currentSlots = GetSlotByCategory(selectedCategory);

        var itemSlot = currentSlots.First(slot => slot.Item == item);
        itemSlot.Count--;
        if (itemSlot.Count == 0) {
            currentSlots.Remove(itemSlot);
        }

        OnUpdated?.Invoke();
    }

    public void AddItem(ItemBase item, int count=1) {
        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotByCategory(category);

        var itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);
        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currentSlots.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }

        OnUpdated?.Invoke();
    }

    ItemCategory GetCategoryFromItem(ItemBase item) {
        if (item is RecoveryItem || item is EvolutionItem)
        {
            return ItemCategory.Items;
        } else if (item is YokaiBallItem)
        {
            return ItemCategory.YokaiBalls;
        } else
        {
            return ItemCategory.TMs;
        }
    }
    
    public static Inventory GetInventory() {
        return FindObjectOfType<ThirdPersonController>().GetComponent<Inventory>();
    }

    public object CaptureState()
    {
        var saveData = new InventorySaveData()
        {
            items = slots.Select(i => i.GetSaveData()).ToList(),
            yokaiBalls = yokaiBallSlots.Select(i => i.GetSaveData()).ToList(),
            TMs = tmSlots.Select(i => i.GetSaveData()).ToList(),
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as InventorySaveData;

        if (saveData != null)
        {
            slots = saveData.items.Select(i => new ItemSlot(i)).ToList();
            yokaiBallSlots = saveData.yokaiBalls.Select(i => new ItemSlot(i)).ToList();
            tmSlots = saveData.TMs.Select(i => new ItemSlot(i)).ToList();

            allSlots = new List<List<ItemSlot>>() { slots, yokaiBallSlots, tmSlots };

            OnUpdated?.Invoke();
        }
    }
}

[Serializable]
public class ItemSlot {
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot() { }

    public ItemSlot(ItemSaveData saveData)
    {
        // grab item from internal item database by name
        item = ItemDB.GetObjectByName(saveData.name);
        // grab count from save data
        count = saveData.count;
    }

    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData()
        {
            name = item.name,
            count = count
        };

        return saveData;
    }

    public ItemBase Item
    {
        get => item;
        set => item = value;
    }
    public int Count {
        get => count;
        set => count = value;
    }
}

[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

[Serializable]
public class InventorySaveData
{
   public List<ItemSaveData> items;
   public List<ItemSaveData> yokaiBalls;
   public List<ItemSaveData> TMs;
}
