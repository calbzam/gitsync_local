using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrangeLightMover : MonoBehaviour
{
    private Vector3 direction = Vector3.forward;

    public float lightMoveSpeed = 5f;
    public float forwardLimit = 8f;
    public float backwardLimit = -8f;

    private void Update()
    {
        if (transform.position.z > forwardLimit || transform.position.z < backwardLimit) direction.z *= -1;
        transform.position += direction * lightMoveSpeed * Time.deltaTime;
    }
}
