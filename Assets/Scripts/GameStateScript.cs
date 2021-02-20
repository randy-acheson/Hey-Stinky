using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateScript : MonoBehaviour
{
    public GameObject crystals;
    public GameObject goal;
    public int numCrystals;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in crystals.transform)
        {
            numCrystals++;
        }
        Debug.Log(numCrystals);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenPortal()
    {
        goal.SetActive(true);
    }
}
