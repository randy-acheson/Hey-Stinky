using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiceNoise : MonoBehaviour
{
    public AudioClip rice;
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (rice != null)
        {
            audioSource.PlayOneShot(rice, 0.7F);
        }
    }
}
