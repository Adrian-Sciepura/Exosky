using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotationSpeed = 100f;
    public float zoomSpeed = 10f;

    public float minZoom = 5f;
    public float maxZoom = 50f;
    private float currentZoom = 5f;

    private void Start()
    {
        currentZoom = minZoom;
    }

    void Update()
    {         
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, mouseX, Space.World);
            transform.Rotate(Vector3.left, mouseY);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        Camera.main.fieldOfView = currentZoom;
    }
}
