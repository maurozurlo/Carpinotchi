using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSelect : MonoBehaviour, IPointerClickHandler {
    public Item item;
    Image img;
    TMP_Text title, amount;
    public bool selectable;
    bool isInitialized;
    
    // Start is called before the first frame update
    void OnEnable() {
        if (isInitialized) return;
        Transform t = transform;
        // Image
        img = t.Find("Image").GetComponent<Image>();
        // Title
        title = t.Find("Title").GetComponent<TMP_Text>();
        // Amount
        amount = t.Find("Amount").GetComponent<TMP_Text>();
        if (item) selectable = true;
        isInitialized = true;
        UpdateVisuals();
    }

    public void UpdateVisuals() {
        if (!isInitialized) return;
        img.sprite = item.sprite;
        title.text = item.name;
        amount.text = $"x{ItemManager.control.GetItemAmount(item.id)}";
        selectable = true;
    }

    public void OutOfStock() {
        selectable = false;
        img.color = new Color(1, 1, 1, .3f);
    }

    public void AddedNewUnits() {
        if (!isInitialized) return;
        selectable = true;
        img.color = new Color(1, 1, 1, 1);
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (!selectable) return;
        ItemManager.control.SetSelectedItem(gameObject);
    }
}
