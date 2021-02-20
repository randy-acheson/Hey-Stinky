using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosetDoorBehavior : MonoBehaviour, InteractiveObject
{
    public string hoverMessage;
 
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayerInteract(GameObject player, byte actionNum)
    {
        Debug.Log("interacting with door");
    }

    public string getHoverMessage()
    {
        return hoverMessage;
    }
}
