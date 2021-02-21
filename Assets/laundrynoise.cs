using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class laundrynoise : MonoBehaviour
{
    public AudioClip washer;
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (washer != null)
        {
            audioSource.PlayOneShot(washer, 0.7F);
        }
    }
}
