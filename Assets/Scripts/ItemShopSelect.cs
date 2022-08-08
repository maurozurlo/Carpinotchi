using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemShopSelect : MonoBehaviour, IPointerClickHandler {

    private Item item;
    public bool onStock;
    

    public void SetItem(Item item) { 
        this.item = item;
        GetComponent<UnityEngine.UI.Image>().sprite = item.sprite;
    }

    public void SetOnStock(bool onStock) { this.onStock = onStock; }


    public void OnPointerClick(PointerEventData eventData) {
        if(onStock && item) {
            ShopManager.control.OpenShopDetailPage(item.id);
        }
    }

}
