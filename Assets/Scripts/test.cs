using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEngine;

public class test : MonoBehaviour
{
    private InputControls input;
    private Vector2 inputDir;

    private Rigidbody2D rb;

    private void Awake()
    {
        input = new InputControls();
        rb = gameObject.GetComponent<Rigidbody2D>();
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
        MovePlayer();
    }

    private void GatherInput()
    {
        inputDir = input.Player.Movement.ReadValue<Vector2>();

        //Debug.Log(inputDir);
    }

    private void MovePlayer()
    {
        Vector2 _frameVelocity = rb.velocity;
        if (inputDir.x == 0)
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, 10f * Time.fixedDeltaTime);
        }
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, inputDir.x * 10f, 10f * Time.fixedDeltaTime);
        }

        rb.velocity = _frameVelocity;
    }
}
