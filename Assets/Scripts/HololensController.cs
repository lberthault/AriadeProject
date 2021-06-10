using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HololensController : MonoBehaviour
{

    Rigidbody rb;
    public float movementSpeed = 1.5f;
    public float mouseSensitivity = 2f;
    float pitch, yaw;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pitch = transform.eulerAngles.x;
        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        Move();
        Rotate();
    }

    private void Move()
    {
        float hMove = Input.GetAxis("Horizontal");
        float vMove = Input.GetAxis("Vertical");
        Vector3 move = transform.forward * vMove + transform.right * hMove;
        rb.MovePosition(rb.position + move.normalized * movementSpeed * (Input.GetKey(KeyCode.LeftShift) ? 2 : 1) * Time.deltaTime);
    }

    private void Rotate()
    {
        float hRot = Input.GetAxis("Mouse X");
        float vRot = Input.GetAxis("Mouse Y");

        pitch -= mouseSensitivity * vRot;
        yaw += mouseSensitivity * hRot;

        pitch = Mathf.Clamp(pitch, -90f, 90f);

        while (yaw < 0f)
        {
            yaw += 360f;
        }
        while (yaw >= 360f)
        {
            yaw -= 360f;
        }

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }
}
