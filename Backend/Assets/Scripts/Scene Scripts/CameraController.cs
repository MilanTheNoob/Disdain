using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector3 zoomAmount;

    [Space]

    public float normalSpeed;
    public float fastSpeed;

    [Space]

    float moveSpeed;
    public float moveTime;
    public float rotAmount;

    Vector3 newPos;
    Vector3 newZoom;
    Quaternion newRot;

    Vector3 dragStartPos;
    Vector3 dragCurrentPos;

    Vector3 rotateStartPos;
    Vector3 rotateCurrentPos;

    void Start()
    {
        newPos = transform.position;
        newRot = transform.rotation;

        newZoom = cameraTransform.localPosition;
    }

    void Update()
    {
        //HandleMouseInput();
        HandleMoveInput();
    }

    void HandleMouseInput()
    {
        if (Input.mouseScrollDelta.y != 0)
            newZoom += Input.mouseScrollDelta.y * zoomAmount;

        if (Input.GetMouseButtonDown(1))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;
            if (plane.Raycast(ray, out entry))
            {
                dragStartPos = ray.GetPoint(entry);
            }
        }
        if (Input.GetMouseButton(1))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;
            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPos = ray.GetPoint(entry);
                newPos = transform.position + dragStartPos - dragCurrentPos;
            }
        }

        if (Input.GetMouseButtonDown(2))
        {
            rotateStartPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            rotateCurrentPos = Input.mousePosition;
            Vector3 difference = rotateStartPos - rotateCurrentPos;

            rotateStartPos = rotateCurrentPos;
            newRot *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }
    }

    void HandleMoveInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = fastSpeed;
        }
        else
        {
            moveSpeed = normalSpeed;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            newPos += (transform.forward * moveSpeed);
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            newPos += (transform.forward * -moveSpeed);
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            newPos += (transform.right * -moveSpeed);
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            newPos += (transform.right * moveSpeed);

        if (Input.GetKey(KeyCode.Q))
            newRot *= Quaternion.Euler(Vector3.up * rotAmount);
        if (Input.GetKey(KeyCode.E))
            newRot *= Quaternion.Euler(Vector3.up * -rotAmount);

        if (Input.GetKey(KeyCode.R))
            newZoom += zoomAmount;
        if (Input.GetKey(KeyCode.F))
            newZoom -= zoomAmount;

        transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * moveTime);
        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * moveTime);

        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * moveTime);
    }
}
