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

    public void CharacterSelected(string character)
    {
        numCharactersSelected++;
        Debug.Log("Select: " + numCharactersSelected);
        if (numCharactersSelected == NumCharacters)
        {
            GameObject.Find("playerPrefab").GetComponent<PlayerController>().GameStart();
        }
    }

    public string CharacterDeselected(string character)
    {
        numCharactersSelected--;
        Debug.Log("Deselect: " + numCharactersSelected);
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
