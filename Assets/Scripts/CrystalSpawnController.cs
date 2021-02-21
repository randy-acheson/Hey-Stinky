using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrystalSpawnController : MonoBehaviour
{
    public GameObject crystalPrefab;
    public int numCrystals;

    private List<GameObject> allSpawns = new List<GameObject>();
    private List<GameObject> filledSpawns = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in transform)
        {
            allSpawns.Add(child.gameObject);
        }

        PlayerController parent_guy_script = GameObject.FindObjectOfType<PlayerController>();
        SpawnCrystals(parent_guy_script.rSeed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnCrystals(Int32 seed)
    {
        var r = new System.Random(seed);
        var randomValues = Enumerable.Range(0, allSpawns.Count)
            .OrderBy(x => r.Next()).Take(numCrystals).ToList();

        for (int i = 0; i < numCrystals; i++)
        {
            var spawnToUse = allSpawns[randomValues[i]];
            var crystal = Instantiate(crystalPrefab, spawnToUse.transform.position, spawnToUse.transform.rotation);
            crystal.transform.parent = spawnToUse.transform;
            filledSpawns.Add(spawnToUse);
        }
    }
}
