using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OhMyGod : MonoBehaviour
{
    public AudioClip andrew;
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (andrew != null)
        {
            audioSource.PlayOneShot(andrew, 0.7F);
        }
    }
}
