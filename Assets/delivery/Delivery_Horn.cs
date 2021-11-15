using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_Horn : MonoBehaviour
{
    AudioSource AS;
    public AudioClip hornSound;
    void Start()
    {
        AS = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.K)) {
            if(!AS.isPlaying) AS.Play();
        }
        else {
            AS.Stop();
        }
    }
}
