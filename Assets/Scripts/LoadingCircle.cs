using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCircle : MonoBehaviour
{
    private RectTransform rectComponent;
    private float rotateSpeed = -200f;

    private void Start()
    {
        rectComponent = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (transform.parent.gameObject.activeInHierarchy)
        {
            rectComponent.Rotate(0f, 0f, rotateSpeed * Time.unscaledDeltaTime);
        }
    }
}