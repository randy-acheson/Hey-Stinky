using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface InteractiveObject
{
    string getHoverMessage();

    void OnPlayerInteract(GameObject player, byte actionNum);
}
