using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_Car : MonoBehaviour
{

    public Vector3 speed = new Vector3(35, 0 ,0);
    AudioSource AS;
    public AudioClip explosion;
    bool isStopped;

    public GameObject wheels1, wheels2;
    public float wheelSpinSpeed = 30;

    public GameObject chassis;

    public Material[] materials;

    public Vector3 mapBounds = new Vector3(60, 0, 0);

    private void Start() {
        AS = GetComponent<AudioSource>();

        // Set random material
        int i = Random.Range(0, materials.Length);
        chassis.GetComponent<MeshRenderer>().material = materials[i];
    }
    void Update()
    {
        if (isStopped) return;
        transform.Translate(speed.x * Time.deltaTime, speed.y * Time.deltaTime, speed.z * Time.deltaTime, Space.World);


        if (transform.position.x < -mapBounds.x) Destroy(gameObject);
        if (transform.position.x > mapBounds.x) Destroy(gameObject);

        float spin = wheelSpinSpeed * Time.deltaTime;
        wheels1.transform.Rotate(new Vector3(spin, 0, 0));
        wheels2.transform.Rotate(new Vector3(spin, 0, 0));
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        if (other.GetComponent<Delivery_Bike>().enabled) {
            other.GetComponent<Delivery_Bike>().Explode();
            isStopped = true;
            AS.PlayOneShot(explosion);
            GetComponentInParent<Delivery_CarSpawner>().crashHasOccurred();
        }
    }
}

