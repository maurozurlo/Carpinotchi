using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_Streetlight : MonoBehaviour {
    public GameObject UI_base;
    public GameObject UI_Green, UI_Yellow, UI_Red;
    enum color {
        red,
        yellow,
        green,
        _
    }

    public Vector3 distanceToNextCrossing;
    public int currentStreet = 0;

    public Vector2 waitingTime = new Vector2(3, 5);
    float currWaitingTime;

    Delivery_CarSpawner carSpawner;

    color nextColor;


    void Start() {
        carSpawner = GetComponent<Delivery_CarSpawner>();
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;

        currWaitingTime = Random.Range(waitingTime.x, waitingTime.y);
        StartCoroutine(SetupStreetlight(color.red));
    }

    IEnumerator SetupStreetlight(color color) {
        // Start carSpawner
        switch (color) {
            case color.red:
            nextColor = color.yellow;
            carSpawner.StartSpawning();
            UI_base.SetActive(true);
            UI_Red.SetActive(true);
            break;
            case color.yellow:
            nextColor = color.green;
            UI_Red.SetActive(false);
            UI_Yellow.SetActive(true);
            break;
            case color.green:
            nextColor = color._;
            carSpawner.StopSpawning();
            UI_Yellow.SetActive(false);
            UI_Green.SetActive(true);
            break;
        }

        yield return new WaitForSeconds(currWaitingTime);

        if (nextColor != color._) {
            StartCoroutine(SetupStreetlight(nextColor));
        }
        else {
            Cleanup();
        }
    }

    void Cleanup() {
        transform.position += distanceToNextCrossing;
        carSpawner.MoveLanes(distanceToNextCrossing);
        UI_base.SetActive(false);
        UI_Green.SetActive(false);
        UI_Red.SetActive(false);
        UI_Yellow.SetActive(false);
    }


}
