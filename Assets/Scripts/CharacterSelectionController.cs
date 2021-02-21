using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectionController : MonoBehaviour
{
    private int numCharactersSelected = 0;

    private const int NumCharacters = 4;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string CharacterSelected(GameObject obj)
    {
        numCharactersSelected++;
        if (numCharactersSelected == NumCharacters)
        {
            GameObject.Find("playerPrefab").GetComponent<PlayerController>().GameStart();
        }
        return GetCharacter(obj);
    }

    public string CharacterDeselected(GameObject obj)
    {
        numCharactersSelected--;
        return null;
    }

    public string GetCharacter(GameObject obj)
    {
        if (obj.CompareTag("CharacterSelect"))
        {
            return obj.name;
        }
        else return null;
    }
}
