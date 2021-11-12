using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_Street : MonoBehaviour
{
    public enum Facing {
        front,
        back
    }
    public Facing facing;
    private bool hasBeenActivated;

    private Vector3 parentPosition;
    // Start is called before the first frame update
    void Start()
    {
        parentPosition = gameObject.transform.parent.transform.position;
    }

    private void OnTriggerEnter(Collider collider) {
        if (!collider.CompareTag("Player")) return;

        if (!hasBeenActivated) {
            Delivery_Map.control.DisplaceStreet(facing.ToString(), parentPosition, this);
        }
    }

    public void ChangeActivationState(bool state) {
        hasBeenActivated = state;
    }
}
