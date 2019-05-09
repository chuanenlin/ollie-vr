using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private Transform playerTransform;
    private Vector3 offset;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        offset = this.transform.position - playerTransform.position;
    }

    void Update()
    {
        this.transform.position = Vector3.forward * playerTransform.position.z + offset; // so it does not follow jumping
    }
}
