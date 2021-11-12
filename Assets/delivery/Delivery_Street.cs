using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_Street : MonoBehaviour
{
    private Vector3 parentPosition;
    void Start()
    {
        parentPosition = gameObject.transform.parent.transform.position;
    }
    private void OnTriggerEnter(Collider collider) {
        if (!collider.CompareTag("Player")) return;
        Vector3 velocity = collider.GetComponent<BikeMovement>().getVelocity();
        Vector3 localVel = collider.transform.TransformDirection(velocity);
        string facing = localVel.z < 0 ? "back" : "front";
        Delivery_Map.control.DisplaceStreet(facing, parentPosition, this);
    }
}
