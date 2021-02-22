using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteMonsterController : MonoBehaviour
{
    private Animator animator;
    private Vector3 prevTransform;
    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        prevTransform = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (animator != null)
        {
            float distMoved = (prevTransform - transform.position).magnitude;
            Debug.Log("run speed: " + distMoved);
            if (distMoved <= 0)
            {
                animator.SetInteger("movementState", 0);
            }
            else if (distMoved <= 5 * Time.deltaTime)
            {
                //Debug.Log("run speed: " + distMoved);
                animator.SetInteger("movementState", 1);
            }
            else
            {
                //Debug.Log("run speed: " + distMoved);
                animator.SetInteger("movementState", 2);
            }
            prevTransform = transform.position;
        }
    }
}
