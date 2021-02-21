using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateScript : MonoBehaviour
{
    public GameObject crystals;
    public GameObject crystalSpawns;
    public GameObject goal;
    public int numCrystals = 2;

    // Start is called before the first frame update
    void Start()
    {
        crystalSpawns.GetComponent<CrystalSpawnController>().SpawnCrystals(numCrystals);
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
