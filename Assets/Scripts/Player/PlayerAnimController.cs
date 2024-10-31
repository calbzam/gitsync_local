using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerAnimController : MonoBehaviour
{
    private InputControls input;
    private SpriteRenderer spriteRenderer;
    private Vector2 inputDir;

    private void Awake()
    {
        input = new InputControls();

        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
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
        inputDir = input.Player.Movement.ReadValue<Vector2>();

        // flip sprite on x direction
        if (inputDir.x > 0)
            spriteRenderer.flipX = false;
        else if (inputDir.x < 0)
            spriteRenderer.flipX = true;
    }
}
