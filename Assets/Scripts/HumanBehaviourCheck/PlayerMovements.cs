using UnityEngine;
using System.IO;
using System;

public class PlayerMovements : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 100.0f;
    public float verticalRotationSpeed = 100.0f;
    private float verticalRotation = 0.0f;
    private CharacterController controller;
    private string csvFilePath;
    public Camera humanCamera;
    private string screenshotFolderPath;
    public float captureInterval = 1.0f;
    private float nextCaptureTime;
    private Vector3 velocity; // To hold the player's vertical velocity
    public float gravity = -9.81f; // Earth's gravity

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (humanCamera == null)
        {
            Debug.LogError("Human camera is not assigned.");
            enabled = false; // Disable script if no camera is assigned
            return;
        }

        screenshotFolderPath = Path.Combine(Application.streamingAssetsPath, "Hm_Screenshots");
        csvFilePath = Path.Combine(Application.persistentDataPath, "PlayerPosition.csv");
        // Directory.CreateDirectory(screenshotFolderPath);
        // Debug.Log($"Screenshot folder created: {screenshotFolderPath}");
        WriteToCSV("Time,PositionX,PositionY,PositionZ");
        nextCaptureTime = Time.time + captureInterval;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
//        RecordPlayerPosition();

        if (Time.time >= nextCaptureTime)
        {
            nextCaptureTime += captureInterval;
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal") * moveSpeed;
        float vertical = Input.GetAxis("Vertical") * moveSpeed;
        Vector3 movement = new Vector3(horizontal, 0, vertical);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        movement.y = velocity.y;

        controller.Move(transform.TransformDirection(movement) * Time.deltaTime);
    }

    void HandleRotation()
    {
        float rotation = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up *  rotation);

        verticalRotation += Input.GetAxis("Mouse Y") * verticalRotationSpeed * Time.deltaTime;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        humanCamera.transform.localEulerAngles = new Vector3(-verticalRotation, humanCamera.transform.localEulerAngles.y, 0);
    }

    void RecordPlayerPosition()
    {
        string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Vector3 playerPosition = transform.position;
        string positionData = $"{currentTime},{playerPosition.x},{playerPosition.y},{playerPosition.z}";
        WriteToCSV(positionData);
    }

    void WriteToCSV(string data)
    {
        using (StreamWriter writer = new StreamWriter(csvFilePath, true))
        {
            writer.WriteLine(data);
        }
    }
}
