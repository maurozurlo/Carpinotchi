using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class ItemManager : MonoBehaviour
{
    public static ItemManager control;
    public GameObject modal;

    Item previouslySelectedItem;
    public Item selectedItem;

    public List<Item> itemList = new List<Item>();

    List<Vector2Int> AmountList = new List<Vector2Int>();

    public GameObject _slot, _image, _text;
    
    DrageableObject slot;
    MeshRenderer image;
    TextMeshPro text;

    public string itemName;
    public int itemAmount;


    public GameObject itemInventoryList;
    public int itemsPerRow = 5;
    public float itemSlotSize = 125;
    public GameObject itemInventorySlotPrefab;

    private GameObject lastClicked;
    private void Awake()
    {
        if (control == null)
            control = this;
        else
            DestroyImmediate(this);

        // FIXME This is not a good place to handle this...
        slot = _slot.GetComponent<DrageableObject>();
        image = _image.GetComponent<MeshRenderer>();
        text = _text.GetComponent<TextMeshPro>();
    }

    public void OpenModal() {
        previouslySelectedItem = selectedItem;
        modal.SetActive(true);
    }

    public void CloseModal(bool isCancel)
    {
        if (isCancel)
        {
            if(previouslySelectedItem && GetItemAmount(previouslySelectedItem.id) > 0) {
                selectedItem = previouslySelectedItem;
                SetItemDisplay();
            }
        }
        modal.SetActive(false);
    }

    public void SetItemDisplay()
    {
        if (selectedItem)
        {
            text.text = selectedItem.name;
            //Cast
            slot.item = selectedItem;
            slot.isEmpty = false;
            _image.SetActive(true);
            Texture itemTexture = SpriteToTexture.textureFromSprite(selectedItem.sprite);
            image.material.mainTexture = itemTexture;
        }
        else
            RemoveItemUI();
    }

    public void RemoveItemUI()
    {
        slot.isEmpty = true;
        image.material.mainTexture = null;
        _image.SetActive(false);
        text.text = "";
    }



    public void SetSelectedItem(GameObject gameObject)
    {
        if (!lastClicked)
            lastClicked = gameObject;

        if (lastClicked != gameObject)
            lastClicked.GetComponent<Outline>().enabled = false;

        gameObject.GetComponent<Outline>().enabled = true;
        lastClicked = gameObject;

        selectedItem = gameObject.GetComponent<ItemSelect>().item;

        //if (selectedItem)
        //    Debug.Log($"Amount for selected item {selectedItem.name} is: {GetItemAmount(selectedItem.id)}");
        SetItemDisplay();
    }

    Item GetItemBasedOnId(int id)
    {
        foreach (Item item in itemList)
        {
            if (item.id == id)
                return item;
        }
        return null;
    }

    int ChangeItemAmount(int itemId, int amount) {
        int idx = AmountList.FindIndex((el) => el.x == itemId);
        Vector2Int item = AmountList[idx];
        item.y += amount;
        AmountList[idx] = new Vector2Int(item.x, item.y);
        return item.y;
    }


    public void ReduceAmountOfItem(int itemId)
    {
        int rest = ChangeItemAmount(itemId, -1);
        ItemSelect itemSelect = GetItemSlot(itemId);
        itemSelect.UpdateVisuals();
        if(rest == 0) {
            itemSelect.OutOfStock();
            lastClicked.GetComponent<Outline>().enabled = false;
        }
    }

    public void AddItemAmount(Item item, int selectedItemQty) {
        // If item already exists, add more units
        Debug.Log(itemList.Count);
        if (itemList.Count != 0 && itemList.Find((e) => e.id == item.id) != null) {
            ChangeItemAmount(item.id, selectedItemQty);
            ItemSelect _itemSelect = GetItemSlot(item.id);
            _itemSelect.AddedNewUnits();
            _itemSelect.UpdateVisuals();
            return;
        }
        // If item doesn't exist, add it to both container and amount list
        itemList.Add(item);
        AmountList.Add(new Vector2Int(item.id, selectedItemQty));

        int itemListCount = itemList.Count;
        GameObject prefab = Instantiate(itemInventorySlotPrefab, itemInventoryList.transform);
        ItemSelect itemSelect = prefab.GetComponent<ItemSelect>();
        itemSelect.item = item;
        itemSelect.selectable = true;
        itemSelect.AddedNewUnits();
        itemSelect.UpdateVisuals();
        // Update scrollable List UI
        float t = (float)itemListCount / itemsPerRow;
        float scrollSpace = (Mathf.CeilToInt(t) - 1) * itemSlotSize;
        RectTransform rect = itemInventoryList.GetComponent<RectTransform>();
        RectTransformExtensions.SetBottom(rect, -scrollSpace);
    }

    public ItemSelect GetItemSlot(int id) {
        foreach(GameObject go in itemInventoryList.transform) {
            ItemSelect itemSelect = go.GetComponent<ItemSelect>();
            if (itemSelect.item.id == id) {
                return itemSelect;
            }
        }
        return null;
    }

    public int GetItemAmount(int id) {
        Vector2Int item = AmountList.Find((el) => el.x == id);
        if (item != null) {
            return item.y;
        }
        Debug.LogError("Item not found in GetItemAmount");
        return 0;
    }

    public void Start()
    {
        SetItemDisplay();
    }

    public void Activate(Item item)
    {
        slot.SendMessage("Activate");
        Debug.Log(item);
    }
}
