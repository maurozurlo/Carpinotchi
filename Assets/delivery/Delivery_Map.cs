using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_Map : MonoBehaviour
{
    public static Delivery_Map control;

    public GameObject[] activeStreets;

    public float displace = 100;

    private void Awake() {
        control = this;
    }

    
    public void DisplaceStreet(string facing, Vector3 parentPos, Delivery_Street street) {
        Debug.Log(facing);
        Debug.Log(parentPos);

        foreach(GameObject _street in activeStreets) {
            foreach(Delivery_Street delivery_Street in _street.GetComponentsInChildren<Delivery_Street>()) {
                if (delivery_Street != street) delivery_Street.ChangeActivationState(false);
                else delivery_Street.ChangeActivationState(true);
            }
        }

        if(facing == "front") {
            float farawayStreet = GetFarStreet("greater").transform.position.z;
            Vector3 orgPos = GetFarStreet("smaller").transform.position;
            GetFarStreet("smaller").transform.position = new Vector3(orgPos.x, orgPos.y, farawayStreet + displace);
        }
        else if(facing == "back") {
            float farawayStreet = GetFarStreet("smaller").transform.position.z;
            Vector3 orgPos = GetFarStreet("greater").transform.position;
            GetFarStreet("greater").transform.position = new Vector3(orgPos.x, orgPos.y, farawayStreet - displace);
        }
    }


    GameObject GetFarStreet(string amount) {
        float pos = 0;
        int farStreet = 0;
        int streets = activeStreets.Length;

        for (int i = 0; i < streets; i++) {
            if (amount == "greater") {
                if (activeStreets[i].transform.position.z > pos) {
                    pos = activeStreets[i].transform.position.z;
                    farStreet = i;
                }
            } else if (amount == "smaller") {
                if (activeStreets[i].transform.position.z < pos) {
                    pos = activeStreets[i].transform.position.z;
                    farStreet = i;
                }
            }
        }

        return activeStreets[farStreet];
    }
}
