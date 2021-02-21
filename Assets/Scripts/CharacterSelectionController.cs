using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSelectionController : MonoBehaviour
{
    public Dictionary<string, string> characterDict;

    // Start is called before the first frame update
    void Start()
    {
        characterDict = new Dictionary<string, string>
        {
            ["monster"] = null,
            ["red"] = null,
            ["green"] = null,
            ["blue"] = null,
        };
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CharacterSelectTCP(string playerHash, bool deselect)
    {
        Dictionary<string, string> tcpCharacterSelectCmd = new Dictionary<string, string>
        {
            ["function"] = "selectCharacter",
            ["playerHash"] = playerHash,
            ["deselect"] = deselect.ToString(),
            ["monster"] = characterDict["monster"],
            ["red"] = characterDict["red"],
            ["green"] = characterDict["green"],
            ["blue"] = characterDict["blue"]
        };

        AsyncTCPClient.Send(ClientConnection.dictmuncher(tcpCharacterSelectCmd));
    }

    public void SelectCharacter(GameObject spawnObj, string playerHash)
    {
        var character = GetCharacter(spawnObj);
        characterDict[character] = playerHash;
        CharacterSelectTCP(playerHash, false);
    }

    public void DeselectCharacter(GameObject spawnObj, string playerHash)
    {
        var character = GetCharacter(spawnObj);
        characterDict[character] = null;
        CharacterSelectTCP(playerHash, true);
    }

    public void CharacterSelectedResponse(Dictionary<string, string> charDict)
    {
        characterDict["monster"] = charDict["monster"];
        characterDict["red"] = charDict["red"];
        characterDict["green"] = charDict["green"];
        characterDict["blue"] = charDict["blue"];

        Debug.Log("Select: Monster=" + characterDict["monster"]
            + " Red=" + characterDict["red"]
            + " Green=" + characterDict["green"]
            + " Blue=" + characterDict["blue"]);

        // If every character is taken, start the game
        if (!string.IsNullOrEmpty(characterDict["monster"]) 
            && !string.IsNullOrEmpty(characterDict["red"]) 
            && !string.IsNullOrEmpty(characterDict["green"]) 
            && !string.IsNullOrEmpty(characterDict["blue"]))
        {
            var myHash = GameObject.Find("playerPrefab")
                .GetComponent<PlayerController>().get_player_hash();
            var myCharacter = GetCharacterFromPlayerHash(myHash);
            GameObject.Find("playerPrefab")
                .GetComponent<PlayerController>().GameStart(myCharacter);
        }
    }

    public void CharacterDeselectedResponse(Dictionary<string, string> charDict)
    {
        characterDict["monster"] = charDict["monster"];
        characterDict["red"] = charDict["red"];
        characterDict["green"] = charDict["green"];
        characterDict["blue"] = charDict["blue"];

        Debug.Log("Select: Monster=" + characterDict["monster"]
            + " Red=" + characterDict["red"]
            + " Green=" + characterDict["green"]
            + " Blue=" + characterDict["blue"]);
    }

    public string GetCharacterFromPlayerHash(string playerHash)
    {
        return characterDict.FirstOrDefault(x => x.Value == playerHash).Key;
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
