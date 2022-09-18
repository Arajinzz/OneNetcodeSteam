using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    float Speed = 12f;

    [SerializeField]
    float Gravity = -9.81f;

    [SerializeField]
    float JumpHeight = 3f;

    [SerializeField]
    float GroundDistance = 0.4f;

    [SerializeField]
    GameObject playerCameraPosition;

    [SerializeField]
    Transform PlayerBody;

    public Transform GroundCheck;
    public LayerMask GroundMask;

    [SerializeField]
    float mouseSens;

    private CharacterController Controller;

    private Vector3 Velocity;
    private bool bGrounded;
    // Camera rotation
    private float xRotation = 2f;

    void Start()
    {
        Controller = GetComponent<CharacterController>();

        // Set camera position and rotation
        Camera.main.transform.position = playerCameraPosition.transform.position;
        Camera.main.transform.rotation = playerCameraPosition.transform.rotation;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        bGrounded = Physics.CheckSphere(GroundCheck.position, GroundDistance, GroundMask);

        if (bGrounded && Velocity.y < 0)
        {
            Velocity.y = -2f;
        }
    }


    void Update()
    {
        HandleMouvement();
        Jump();

        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSens * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSens * Time.deltaTime;

        // Camera rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -15f, 15f);
        playerCameraPosition.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // Update camera
        Camera.main.transform.position = playerCameraPosition.transform.position;
        Camera.main.transform.rotation = playerCameraPosition.transform.rotation;

        // Falling
        Velocity.y += Gravity * Time.deltaTime;
        Controller.Move(Velocity * Time.deltaTime);
    }

    private void Jump()
    {
        if (Input.GetKey(KeyCode.Space) && bGrounded)
        {
            Velocity.y = Mathf.Sqrt(JumpHeight * -2f * Gravity);
        }
    }

    public void HandleMouvement()
    {
        float x = 0;
        float z = 0;

        if (Input.GetKey(KeyCode.W))
        {
            z += 1;
        }

        if (Input.GetKey(KeyCode.S))
        {
            z += -1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            x += -1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            x += 1;
        }

        Vector3 move = transform.right * x + transform.forward * z;
        Controller.Move(move * Speed * Time.deltaTime);
    }
}
