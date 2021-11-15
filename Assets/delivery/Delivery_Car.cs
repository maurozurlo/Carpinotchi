using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_Car : MonoBehaviour
{

    public float speed = 35;
    AudioSource AS;
    public AudioClip explosion;
    bool isStopped;

    private void Start() {
        AS = GetComponent<AudioSource>();
    }
    void Update()
    {
        if (isStopped) return;
        transform.Translate(0, 0, speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        if (other.GetComponent<Delivery_Bike>().enabled) {
            other.GetComponent<Delivery_Bike>().Explode();
            isStopped = true;
            AS.PlayOneShot(explosion);
        }
    }
}

