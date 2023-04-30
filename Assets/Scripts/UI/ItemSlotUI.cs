using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI countText;

    RectTransform rectTransform;

    private void Awake() {
        
    }

    public TextMeshProUGUI NameText => nameText;
    public TextMeshProUGUI CountText => countText;

    public float Height => rectTransform.rect.height;

    public void SetData(ItemSlot itemSlot) {
        rectTransform = GetComponent<RectTransform>();
        nameText.text = itemSlot.Item.Name;
        countText.text = $"X {itemSlot.Count}";
    }
}
