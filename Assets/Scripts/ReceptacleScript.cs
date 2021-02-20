using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceptacleScript : MonoBehaviour
{
    public GameObject gameState;

    private List<GameObject> crystals = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCrystal(GameObject crystal)
    {
        crystals.Add(crystal);
        crystal.GetComponent<CrystalController>()
            .SetTransformParent(gameObject.transform);
        crystal.GetComponent<CrystalController>().isDeposited = true;

        if (crystals.Count == gameState.GetComponent<GameStateScript>().numCrystals)
        {
            gameState.GetComponent<GameStateScript>().OpenPortal();
        }
    }
}
