using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{

    private Animator anim;
    public AudioClip collectSound;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        anim.SetTrigger("Spawn");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GameManager.Instance.GetCoin();
            anim.SetTrigger("Collected");
            AudioSource audio = GetComponent<AudioSource>();
            audio.PlayOneShot(collectSound);
            // Destroy(gameObject, 1.5f);
        }
    }
}
