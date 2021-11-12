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
        GameObject greater = GetFarStreet("greater");
        GameObject smaller = GetFarStreet("smaller");
        if (facing == "front") {
            float farawayStreet = greater.transform.position.z;
            Vector3 smallerPos = smaller.transform.position;
            Vector3 newPos = new Vector3(smallerPos.x, smallerPos.y, farawayStreet + displace);
            smaller.transform.position = newPos;
        }
        else if(facing == "back") {
            float farawayStreet = smaller.transform.position.z;
            Vector3 greaterPos = greater.transform.position;
            Vector3 newPos = new Vector3(greaterPos.x, greaterPos.y, farawayStreet - displace);
            greater.transform.position = newPos;
        }
    }


    GameObject GetFarStreet(string amount) {
        float pos = activeStreets[0].transform.position.z;
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
