using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeMovement : MonoBehaviour
{
    Rigidbody rb;
    public GameObject[] wheels;
    public float speed = 20;
    public float rotationSpeed = 20;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
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
    }

    void MoveWheels(float rotateSpeed) {
        foreach(GameObject wheel in wheels) {
            float rotate = (rotateSpeed * 25 )* speed * Time.deltaTime;
            //Debug.Log(rotate);
            wheel.transform.Rotate(new Vector3(0, rotate, 0));
        }
    }
}
