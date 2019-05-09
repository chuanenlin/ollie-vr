using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightRailAnimation : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    private MeshRenderer rend;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float offset = Time.time * scrollSpeed;
        rend.material.SetTextureOffset("_MainTex", new Vector2(0, -offset));
    }
}
