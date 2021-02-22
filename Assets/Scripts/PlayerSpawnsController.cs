using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSpawnsController : MonoBehaviour
{
    private const int numPlayers = 4; // ¯\_(ツ)_/¯

    private List<GameObject> allSpawns = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in transform)
        {
            allSpawns.Add(child.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector3 GetSpawn(int seed)
    {
        var r = new System.Random(seed);
        var randomValues = Enumerable.Range(0, allSpawns.Count)
            .OrderBy(x => r.Next()).Take(4).ToList();

        var myHash = FindObjectOfType<ClientConnection>()
            .current_creature_script.get_player_hash();

        var playerHolder = FindObjectOfType<ClientConnection>().player_holder;
        var players = new List<string>(playerHolder.Keys);
        players.Add(myHash);
        players.Sort();
        var index = players.IndexOf(myHash);

        return allSpawns[randomValues[index]].transform.position;
    }
}
