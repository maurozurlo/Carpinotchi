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
    bool initialized = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable() {
        if (initialized) return;

        Transform t = transform;
        // Image
        img = t.Find("Image").GetComponent<Image>();
        // Title
        title = t.Find("Title").GetComponent<TMP_Text>();
        // Amount
        amount = t.Find("Amount").GetComponent<TMP_Text>();
        if (item) selectable = true;
        initialized = true;
        UpdateVisuals();
    }

    public void UpdateVisuals() {
        if (!initialized) return;
        if (item) {
            img.sprite = item.sprite;
            title.text = item.name;
            amount.text = $"x{ItemManager.control.GetItemAmount(item.id)}";
            selectable = true;
        }
        else {
            img.sprite = null;
            title.text = "None";
            amount.text = "";
            selectable = false;
        }
    }

    public void OutOfStock() {
        selectable = false;
        img.color = new Color(1, 1, 1, .3f);
    }

    public void BuyNewUnits() {
        selectable = true;
        img.color = new Color(1, 1, 1, 1);
    }

    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log(selectable);
        if (!selectable) return;
        ItemManager.control.SetSelectedItem(gameObject);
    }
}
