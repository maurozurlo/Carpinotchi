using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour {
    public static ShopManager control;
    public GameObject shopListModal;

    public List<Item> itemList = new List<Item>();

    public Item selectedItem = null;
    public int selectedItemQty = 1;
    public GameObject[] buySlots;
    public GameObject buyButton;

    private void Awake() {
        if (control == null)
            control = this;
        else
            DestroyImmediate(this);
    }

    private void Start() {
        FillSlots();
    }
    /// Shop List
    void FillSlots() {
        int itemListCount = itemList.Count;
        for (int i = 0; i < itemListCount; i++) {
            ItemShopSelect shopSelect = buySlots[i].GetComponent<ItemShopSelect>();
            shopSelect.SetItem(itemList[i]);
            shopSelect.SetOnStock(true);
        }
    }

    /// Shop Detail    
    public GameObject shopDetailModal, itemTitle, itemDescription, itemImage, itemTotal, itemPrice, inputField;
    public void PopulateShopDetail() {
        if (!selectedItem) return;
        itemTitle.GetComponent<TMP_Text>().text = selectedItem.name;
        itemDescription.GetComponent<TMP_Text>().text = selectedItem.description;
        itemImage.GetComponent<Image>().sprite = selectedItem.sprite;
        itemPrice.GetComponent<TMP_Text>().text = $"<color=#000>Precio: </color>{string.Format("{0:C2}", selectedItem.price)}";
        selectedItemQty = 1;
        UpdateUnits();
    }

    public void CloseModal(string modal) {
        
        if (modal == "detail") {
            shopDetailModal.SetActive(false);
            selectedItem = null;
        }
        if (modal == "list") {
            shopListModal.SetActive(false);
        }

        if(modal == "all") {
            selectedItem = null;
            shopListModal.SetActive(false);
        }
    }

    public void OpenModal(string modal) {
        if (modal == "detail") {
            CloseModal("list");
            PopulateShopDetail();
            shopDetailModal.SetActive(true);
        }
        if (modal == "list") {
            CloseModal("detail");
            shopListModal.SetActive(true);
        }
    }

    public void OpenShopDetailPage(int itemId) {
        selectedItem = itemList.Find((e) => e.id == itemId);
        Debug.Log(selectedItem);
        OpenModal("detail");
    }

    public void BuyItem() {
        Debug.Log($"Buying {selectedItemQty} of {selectedItem.name}");
        MoneyManager.control.SpendMoney(selectedItemQty * selectedItem.price);
        ItemManager.control.AddItemAmount(selectedItem.id, selectedItemQty);
        CloseModal("detail");
    }

    public void MoreUnits() {
        if (selectedItemQty == 10) return;
        selectedItemQty++;
        UpdateUnits();
    }

    public void LessUnits() {
        if (selectedItemQty == 1) return;
        selectedItemQty--;
        UpdateUnits();
    }

    void UpdateUnits() {
        int total = selectedItem.price * selectedItemQty;
        itemTotal.GetComponent<TMP_Text>().text = $"<color=#000>Total: </color>{string.Format("{0:C2}", total)}";
        inputField.GetComponent<InputField>().text = selectedItemQty.ToString();
        buyButton.GetComponent<Button>().interactable = MoneyManager.control.GetMoney() >= total;
    }
}

