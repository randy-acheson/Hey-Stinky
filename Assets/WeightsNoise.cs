using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightsNoise : MonoBehaviour
{
    public AudioClip weights;
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (weights != null)
        {
            audioSource.PlayOneShot(weights, 0.7F);
        }
    }
}
