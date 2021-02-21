using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireNoise : MonoBehaviour
{
    public AudioClip fire;
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (fire != null)
        {
            audioSource.PlayOneShot(fire, 0.7F);
        }
    }
}
