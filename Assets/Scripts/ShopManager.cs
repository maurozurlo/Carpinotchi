using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ShopManager : MonoBehaviour {
    public static ShopManager control;
    public GameObject shopListModal;

    List<Item> itemList = new List<Item>();

    public Item selectedItem = null;
    public int selectedItemQty = 1;
    public GameObject itemShopList;
    public int itemsPerRow = 5;
    public float itemSlotSize = 125;
    public GameObject itemShopSlotPrefab;
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
        Item[] consumables = Resources.LoadAll<Item>("Consumables");
        itemList = consumables.ToList();

        int itemListCount = itemList.Count;
        for (int i = 0; i < itemListCount; i++) {
            GameObject prefab = Instantiate(itemShopSlotPrefab, itemShopList.transform);
            ItemShopSelect shopSelect = prefab.GetComponent<ItemShopSelect>();
            shopSelect.SetItem(itemList[i]);
            shopSelect.SetOnStock(true);
        }
        float t = (float)itemListCount / itemsPerRow;
        float scrollSpace = (Mathf.CeilToInt(t) - 1) * itemSlotSize;
        RectTransform rect = itemShopList.GetComponent<RectTransform>();
        RectTransformExtensions.SetBottom(rect, -scrollSpace);
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
        OpenModal("detail");
    }

    public void BuyItem() {
        //Debug.Log($"Buying {selectedItemQty} of {selectedItem.name}");
        MoneyManager.control.SpendMoney(selectedItemQty * selectedItem.price);
        ItemManager.control.AddItemAmount(selectedItem, selectedItemQty);
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

