using System.Collections;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Door_open"))
        {
            foreach (GameObject player in playersToHide) {

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
