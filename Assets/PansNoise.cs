using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PansNoise : MonoBehaviour
{
    public AudioClip pans;
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (pans != null)
        {
            audioSource.PlayOneShot(pans, 0.7F);
        }
    }
}
