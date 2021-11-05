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

    public List<int> AmountList = new List<int>();

    public GameObject _slot, _image, _text;
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
            Texture itemTexture = SpriteToTexture.textureFromSprite(selectedItem.sprite) as Texture;
            image.material.mainTexture = itemTexture;
        }
        else
            RemoveItemUI();
    }

    public void RemoveItemUI()
    {
        slot.isEmpty = true;
        image.material.mainTexture = null;
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

        int id = int.Parse(gameObject.name);
        selectedItem = GetItemBasedOnId(id);

        if (selectedItem)
            Debug.Log($"Amount for selected item {selectedItem.name} is: {AmountList[id]}");
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
        AmountList[id]--;
    }

    public void Start()
    {
        SetItemDisplay();
    }

    public void Activate(Item item)
    {
        slot.SendMessage("Activate");
    }
}
