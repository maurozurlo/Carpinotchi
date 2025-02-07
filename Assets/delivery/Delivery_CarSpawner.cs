using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_CarSpawner : MonoBehaviour
{
    public GameObject carPrefab;
    public GameObject leftLane, rightLane;
    public float minSpawnTime = .25f;
    bool canSpawn;

    Vector3[] lanesPos = new Vector3[] { Vector3.zero, Vector3.zero };

    void Start(){
        lanesPos[0] = leftLane.transform.position;
        lanesPos[1] = rightLane.transform.position;
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


    public void StartSpawning() {
        canSpawn = true;
        StartCoroutine("SpawnCar");
    }

    public void StopSpawning() {
        canSpawn = false;
    }

    public void MoveLanes(Vector3 distance) {
        leftLane.transform.position += distance;
        rightLane.transform.position += distance;
    }

    public void ResetPositions(){
        leftLane.transform.position = lanesPos[0];
        rightLane.transform.position = lanesPos[1];
    }



    
}
