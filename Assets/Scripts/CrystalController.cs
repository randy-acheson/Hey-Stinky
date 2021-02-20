using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalController : MonoBehaviour, InteractiveObject
{
    public bool isDeposited = false;
    public string hoverMessage;

    public string getHoverMessage() => hoverMessage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTransformParent(Transform parent)
    {
        transform.parent = parent;
    }

    public void OnPlayerInteract(GameObject player, byte actionNum)
    {
        return;
    }
}
