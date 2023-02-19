using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_Bike : MonoBehaviour
{
    public static Delivery_Bike control;
    Rigidbody rb;
    public float crashSpeed = 850;

    public GameObject[] wheels;
    public float initialSpeed = 20;
    public float initialRotationSpeed = 10;
    private float speed;
    private float rotationSpeed;
    Vector3 PrevPos;
    Vector3 NewPos;
    Vector3 ObjVelocity;


    public GameObject turningWheel;
    Vector3 turningWheelPrevRot;
    public Vector2 rotationClamp;

    // Audio
    AudioSource AS;
    public AudioClip bikeStart, bikeLoop, bikeStop;
    Queue<AudioClip> clipQueue = new Queue<AudioClip>();

    public int val = 100;



    private void Awake() {
        control = this;
    }

    public void Explode() {
        Camera.main.GetComponent<SmoothFollow>().enabled = false;
        CameraShake cs = Camera.main.GetComponent<CameraShake>();
        StartCoroutine(cs.Shake(3, 0.25f));
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, Random.Range(0, 360), transform.eulerAngles.z);
        rb.isKinematic = false;
        Vector3 force = transform.forward;
        force = new Vector3(force.x, 1, force.z);
        rb.AddForce(force * crashSpeed);
        speed = 0;
        rotationSpeed = 0;
        AS.Stop();
        Delivery_Manager.control.GameOver();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        PrevPos = transform.position;
        NewPos = transform.position;
        AS = GetComponent<AudioSource>();
        turningWheelPrevRot = turningWheel.transform.rotation.eulerAngles;
        InitSpeed();
    }

    public void InitSpeed()
    {
        speed = initialSpeed;
        rotationSpeed = initialRotationSpeed;
    }


    void Update()
    {
        float translation = GetPressure(Mathf.Clamp(Input.GetAxis("Vertical"), 0, 1)) * speed;
        translation *= Time.deltaTime;
        transform.Translate(0, 0, translation);
        MoveWheels(translation);

        PlayAudio(translation);
    }

    float GetPressure(float value) {
        return 1 - (value * 1);
    }

    float calcRotation(float keypress) {
        float val = 90;
        
        if (keypress > .1f) val = Mathf.Lerp(rotationClamp.y, 90, keypress);
        if (keypress < .1f) val = Mathf.Lerp(rotationClamp.x, 90, keypress);
        if (keypress == 0) val = 90;

        return Mathf.Clamp(val, rotationClamp.x, rotationClamp.y);
    }

    void MoveWheels(float rotateSpeed) {
        foreach(GameObject wheel in wheels) {
            float rotate = (rotateSpeed * 25 )* speed * Time.deltaTime;
            wheel.transform.Rotate(new Vector3(0, 0, rotate));
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
