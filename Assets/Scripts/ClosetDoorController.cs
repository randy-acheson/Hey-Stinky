﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosetDoorController : MonoBehaviour, InteractiveObject
{
    public string hoverMessage;

    private Animator animator;
    private List<GameObject> playersToHide;
 
    void Start()
    {
        animator = GetComponent<Animator>();
        playersToHide = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Door_open"))
        {
            foreach (GameObject player in playersToHide) {
                player.transform.position = new Vector3(-12.5f, -7.2f, -17.8f);
                player.transform.rotation.eulerAngles.Set(0f, 67.3f, 0f);
            }
            animator.SetBool("open", false);
            playersToHide.Clear();
        }
    }

    public void OnPlayerInteract(GameObject player, byte actionNum)
    {
        Debug.Log("interacting with door");
        animator.SetBool("open", true);
        playersToHide.Add(player);
    }

    public string getHoverMessage()
    {
        return hoverMessage;
    }
}
