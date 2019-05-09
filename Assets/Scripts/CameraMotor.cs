using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{
    public Transform lookAt; // Our character
    public Vector3 offset = new Vector3(0, 1.0f, -1.5f); // x, y, distance to player
    //public Vector3 offset = new Vector3(0, 3.0f, -10.0f);

    private void Start()
    {
        transform.position = lookAt.position + offset;
    }

    private void LateUpdate()
    {
        Vector3 desiredPosition = lookAt.position + offset;
        //desiredPosition.x = 0;
        transform.position = desiredPosition;
        //transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime);
    }
}
