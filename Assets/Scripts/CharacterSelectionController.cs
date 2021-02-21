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
            ["Monster"] = null,
            ["Red"] = null,
            ["Green"] = null,
            ["Blue"] = null,
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
            ["Monster"] = characterDict["Monster"],
            ["Red"] = characterDict["Red"],
            ["Green"] = characterDict["Green"],
            ["Blue"] = characterDict["Blue"]
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
        characterDict["Monster"] = charDict["Monster"];
        characterDict["Red"] = charDict["Red"];
        characterDict["Green"] = charDict["Green"];
        characterDict["Blue"] = charDict["Blue"];

        Debug.Log("Select: Monster=" + characterDict["Monster"]
            + " Red=" + characterDict["Red"]
            + " Green=" + characterDict["Green"]
            + " Blue=" + characterDict["Blue"]);

        // If every character is taken, start the game
        if (!string.IsNullOrEmpty(characterDict["Monster"]) 
            && !string.IsNullOrEmpty(characterDict["Red"]) 
            && !string.IsNullOrEmpty(characterDict["Green"]) 
            && !string.IsNullOrEmpty(characterDict["Blue"]))
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
        characterDict["Monster"] = charDict["Monster"];
        characterDict["Red"] = charDict["Red"];
        characterDict["Green"] = charDict["Green"];
        characterDict["Blue"] = charDict["Blue"];

        Debug.Log("Deselect: Monster=" + characterDict["Monster"]
            + " Red=" + characterDict["Red"]
            + " Green=" + characterDict["Green"]
            + " Blue=" + characterDict["Blue"]);
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
