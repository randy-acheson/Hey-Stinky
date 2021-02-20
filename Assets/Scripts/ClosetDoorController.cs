using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosetDoorController : MonoBehaviour, InteractiveObject
{
    public string openMessage;
    public string closeMessage;
    public bool isOpen = false;

    private Animator animator;
 
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPlayerInteract(GameObject player, byte actionNum)
    {
        Debug.Log("interacting with door");
        isOpen = !isOpen;
        animator.SetBool("open", isOpen);
    }

    public string getHoverMessage()
    {
        if (isOpen)
        {
            return openMessage;
        }
        else
        {
            return closeMessage;
        }
    }
}
