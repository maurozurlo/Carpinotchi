using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_Horn : MonoBehaviour
{
    AudioSource AS;
    public AudioClip hornSound;
    float hornPitch = 1;
    void Start()
    {
        AS = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) {
            PlayHorn();
        }
    }

    void PlayHorn() {
        AS.Stop();
        AS.pitch = hornPitch;
        AS.PlayOneShot(hornSound);
        if (Mathf.RoundToInt(hornPitch) < 1.5f) {
            hornPitch += .1f;
        }
        else {
            hornPitch = 1;
        }
    }
}
