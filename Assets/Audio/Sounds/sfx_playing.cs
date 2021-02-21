using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sfx_playing : MonoBehaviour
{
    public AudioSource lidcreak;
    public AudioSource rice;
    public AudioSource chair_trombone;
    public AudioSource toilet;
    //public AudioSource pans;
  
    public void PlayCreak() { 
        lidcreak.Play();
    }
    public void PlayRice()
    {
        rice.Play();
    }
    public void PlayChair()
    {
        chair_trombone.Play();
    }
    public void PlayToilet()
    {
        toilet.Play();
    }

    void OnTriggerEnter() { 
         rice.Play();
      //void OnTriggerEnter(Collider cube)
 //   {
 //       if (cube.gameObject.tag == "Untagged")
 //       {
 //           Debug.Log(cube.tag);
 //           rice.Play();
 //           Debug.Log(cube.gameObject.tag);
 //       }
    }
    
}
