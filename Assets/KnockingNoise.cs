using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockingNoise : MonoBehaviour
{
    public AudioClip knocking;
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (knocking != null)
        {
            audioSource.PlayOneShot(knocking, 0.7F);
        }
    }
}
