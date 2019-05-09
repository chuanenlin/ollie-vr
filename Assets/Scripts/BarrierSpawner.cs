using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierSpawner : MonoBehaviour
{
    private const float DISTANCE_TO_RESPAWN = 10.0f;

    public float scrollSpeed = -2.0f;
    public float totalLength;
    public bool IsScrolling
    {
        set;
        get;
    }

    private float scrollLocation;
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (!IsScrolling)
        {
            return;
        }
        else
        {
            scrollLocation += scrollSpeed * Time.deltaTime;
            Vector3 newLocation = (playerTransform.position.z + scrollLocation) * Vector3.forward;
            transform.position = newLocation;

            if (transform.GetChild(0).transform.position.z < playerTransform.position.z - DISTANCE_TO_RESPAWN)
            {
                transform.GetChild(0).localPosition += Vector3.forward * totalLength;
                transform.GetChild(0).SetSiblingIndex(transform.childCount);

                transform.GetChild(0).localPosition += Vector3.forward * totalLength;
                transform.GetChild(0).SetSiblingIndex(transform.childCount);
            }
        }
    }
}
