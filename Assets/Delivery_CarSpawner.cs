using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_CarSpawner : MonoBehaviour
{
    public GameObject carPrefab;
    public GameObject leftLane, rightLane;
    public float minSpawnTime = .25f;
    bool canSpawn = true;

    void Start()
    {
        StartCoroutine("SpawnCar");
    }

    IEnumerator SpawnCar() {
        if (!canSpawn) yield break;

        float selectLane = Random.Range(0, 10);
        GameObject laneToSpawn = selectLane > 5 ? leftLane : rightLane;
        int multiplier = selectLane > 5 ? -1 : 1;

        GameObject carInstance = Instantiate(carPrefab, laneToSpawn.transform.position, laneToSpawn.transform.rotation);
        Vector3 carSpeed = carInstance.GetComponent<Delivery_Car>().speed;
        carInstance.GetComponent<Delivery_Car>().speed = new Vector3(carSpeed.x * multiplier, carSpeed.y, carSpeed.z);
        carInstance.transform.SetParent(gameObject.transform);
        yield return new WaitForSeconds(Random.Range(minSpawnTime, minSpawnTime * 3));
        StartCoroutine("SpawnCar");
    }

    public void crashHasOccurred() {
        canSpawn = false;
    }



    
}
