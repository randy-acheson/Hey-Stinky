using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlushNoise : MonoBehaviour
{
    public AudioClip toilet;
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(toilet != null)
        {
            audioSource.PlayOneShot(toilet, 0.7F);
        }
    }
}
    
