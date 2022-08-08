using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrageableObject : MonoBehaviour {
    private Vector3 originalPosition;
    private Vector3 screenPoint;
    private Vector3 offset;
    public Item item;
    bool isOverTarget;
    public bool isEmpty = true;

    private void Start() {
        originalPosition = this.transform.position;
    }
    void OnMouseDown() {
        if (isEmpty)
            return;
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    void OnMouseDrag() {
        if (isEmpty)
            return;
        Vector3 cursorScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorScreenPoint) + offset;
        transform.position = cursorPosition;
    }

    private void OnMouseUp() {
        if (isEmpty)
            return;
        if (!isOverTarget) {
            transform.position = originalPosition;
            return;
        }

        //Estamos arriba del carpincho
        ItemManager.control.ReduceAmountOfItem(item.id);
        
        //Activar
        if (item is ConsumableItem consumableItem) {
            consumableItem.Consume();
        }
        //Go back to empty?


        if(ItemManager.control.GetItemAmount(item.id) == 0) {
            ItemManager.control.RemoveItemUI();
        }

        //Dull
        VisualManager.control.Dull();
        //Place
        transform.position = originalPosition;
    }



    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("carpincho")) {
            isOverTarget = true;
            VisualManager.control.Glow();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("carpincho")) {
            isOverTarget = false;
            VisualManager.control.Dull();
        }

    }
}
