using System;
using System.Collections;
using System.Collections.Generic;
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
    public GameObject[] inventorySlots;
    DrageableObject slot;
    MeshRenderer image;
    TextMeshPro text;

    public string itemName;
    public int itemAmount;

    private GameObject lastClicked;
    private void Awake()
    {
        if (control == null)
            control = this;
        else
            DestroyImmediate(this);

        //Set
        slot = _slot.GetComponent<DrageableObject>();
        image = _image.GetComponent<MeshRenderer>();
        text = _text.GetComponent<TextMeshPro>();

        //Debug Populate
        int itemLength = itemList.Count;
        for (int i = 0; i < itemLength; i++) {
            AmountList.Add(new Vector2Int(itemList[i].id, 1));
            if (inventorySlots[i]) {
                ItemSelect itemSelect = inventorySlots[i].GetComponent<ItemSelect>();
                itemSelect.item = itemList[i];
                itemSelect.UpdateVisuals();
            }
        }
    }

    private void OnMouseDown()
    {
        GetComponent<BoxCollider>().enabled = false;
        previouslySelectedItem = selectedItem;
        StartCoroutine("openModal");
    }

    IEnumerator openModal()
    {
        yield return new WaitForSeconds(.1f);
        modal.SetActive(true);
    }

    public void CloseModal(bool isCancel)
    {
        if (isCancel)
        {
            selectedItem = previouslySelectedItem;
            SetItemDisplay();
        }

        GetComponent<BoxCollider>().enabled = true;
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

        if (selectedItem)
            Debug.Log($"Amount for selected item {selectedItem.name} is: {GetItemAmount(selectedItem.id)}");
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

    public void ReduceAmountOfItem(int id)
    {
        int idx = AmountList.FindIndex((el) => el.x == id);
        Vector2Int item = AmountList[idx];
        item.y--;
        AmountList[idx] = new Vector2Int(item.x, item.y);

        ItemSelect itemSelect = GetItemSlot(item.x);
        itemSelect.UpdateVisuals();

        if(item.y == 0) {
            itemSelect.OutOfStock();
            lastClicked.GetComponent<Outline>().enabled = false;
        }
    }

    public ItemSelect GetItemSlot(int id) {
        foreach(GameObject go in inventorySlots) {
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
