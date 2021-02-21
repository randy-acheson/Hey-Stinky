using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateScript : MonoBehaviour
{
    public GameObject crystalSpawns;
    public GameObject goal;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetNumCrystals()
    {
        return crystalSpawns.GetComponent<CrystalSpawnController>().numCrystals;
    }

    public void OpenPortal()
    {
        goal.SetActive(true);
    }
}
