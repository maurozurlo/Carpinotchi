using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeMovement : MonoBehaviour
{
    public static BikeMovement control;
    Rigidbody rb;
    public GameObject[] wheels;
    public float speed = 20;
    public float rotationSpeed = 20;
    Vector3 PrevPos;
    Vector3 NewPos;
    Vector3 ObjVelocity;

    // Audio
    AudioSource AS;
    public AudioClip bikeStart, bikeLoop, bikeStop;
    Queue<AudioClip> clipQueue = new Queue<AudioClip>();



    private void Awake() {
        control = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        PrevPos = transform.position;
        NewPos = transform.position;
        AS = GetComponent<AudioSource>();
    }

    void Update()
    {
        float translation = Input.GetAxis("Vertical") * speed;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;
        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;
        transform.Translate(0, 0, translation);
        transform.Rotate(0, rotation, 0);
        MoveWheels(translation);

        PlayAudio(translation);
    }

    void MoveWheels(float rotateSpeed) {
        foreach(GameObject wheel in wheels) {
            float rotate = (rotateSpeed * 25 )* speed * Time.deltaTime;
            //Debug.Log(rotate);
            wheel.transform.Rotate(new Vector3(0, rotate, 0));
        }
    }

    void FixedUpdate() {
        NewPos = transform.position;  // each frame track the new position
        ObjVelocity = (NewPos - PrevPos) / Time.fixedDeltaTime;  // velocity = dist/time
        PrevPos = NewPos;  // update position for next frame calculation
    }

    public Vector3 getVelocity() {
        return ObjVelocity;
    }

    public Vector3 playerPosition() {
        return transform.position;
    }

    void PlayAudio(float movingSpeed) {
        if (!AS.isPlaying && clipQueue.Count > 0) {
            AS.clip = clipQueue.Dequeue();           
            AS.Play();
            //Start loop
            if (AS.clip == bikeLoop) AS.loop = true;
        }

        if (!AS.isPlaying) {
            PlayStartSound(movingSpeed);
        }

        if (AS.isPlaying && AS.loop) PlayStopSound(movingSpeed);
    }

    void PlayStartSound(float movingSpeed) {
        if (Mathf.Abs(movingSpeed) > .1f) {
            clipQueue.Enqueue(bikeStart);
            clipQueue.Enqueue(bikeLoop);
        }
    }

    void PlayStopSound(float movingSpeed) {
        if (AS.clip == bikeStop) return;

        if (movingSpeed == 0) {
            clipQueue.Clear();
            AS.Stop();
            AS.loop = false;
            AS.clip = bikeStop;
            AS.PlayOneShot(bikeStop);
        }
    }
}
