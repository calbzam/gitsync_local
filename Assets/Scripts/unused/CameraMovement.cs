using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class CameraMovement : MonoBehaviour
{
    private InputControls input;
    private Vector2 inputDir;

    private Camera cam;
    private Vector3 frameCamMovespeed;

    public float maxCamSpeed = 0.1f;
    public float acceleration = 0.04f;
    public float deceleration = 0.04f;

    private void Awake()
    {
        input = new InputControls();
    }

    private void Start()
    {
        cam = gameObject.GetComponent<Camera>();
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Update()
    {
        GatherInput();
    }

    private void FixedUpdate()
    {
        MoveCamera();
    }

    private void GatherInput()
    {
        inputDir = input.Player.Movement.ReadValue<Vector2>();
    }

    private void MoveCamera()
    {
        if (inputDir.x < 0)
            frameCamMovespeed.x = Mathf.MoveTowards(frameCamMovespeed.x, -maxCamSpeed, acceleration * Time.fixedDeltaTime);
        else if (inputDir.x > 0)
            frameCamMovespeed.x = Mathf.MoveTowards(frameCamMovespeed.x, maxCamSpeed, acceleration * Time.fixedDeltaTime);
        else
            frameCamMovespeed.x = Mathf.MoveTowards(frameCamMovespeed.x, 0, deceleration * Time.fixedDeltaTime);
        //frameCamPos.x += acceleration * MathF.Sign(inputDir.x);

        cam.transform.position += frameCamMovespeed;
    }
}
