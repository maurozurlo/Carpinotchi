using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemShopSelect : MonoBehaviour, IPointerClickHandler {

    private Item item;
    public bool inStock;
    

    public void SetItem(Item item) { 
        this.item = item;
        GetComponent<UnityEngine.UI.Image>().sprite = item.sprite;
    }

    public void SetOnStock(bool inStock) { this.inStock = inStock; }


    public void OnPointerClick(PointerEventData eventData) {
        if(inStock && item) {
            ShopManager.control.OpenShopDetailPage(item.id);
        }
    }

}
