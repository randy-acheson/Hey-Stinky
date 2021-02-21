using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DictCommandEvaluator {
    ClientConnection client_connection_script = GameObject.FindObjectOfType<ClientConnection>();

    public void eval(string command) {
        Type command_eval_type = this.GetType();
        MethodInfo target_method = command_eval_type.GetMethod(command);
        target_method.Invoke(this, null);
    }

    public void eval(string command, object[] argv) {
        Type command_eval_type = this.GetType();
        MethodInfo target_method = command_eval_type.GetMethod(command);
        target_method.Invoke(this, argv);
    }

    // Requires:
    //     playerHash;
    //     isLightOn;
    public void toggleFlashlight(Dictionary<string, string> args) {
        try {
            if (args["playerHash"] != client_connection_script.current_creature_script.get_player_hash()) {
                GameObject player = client_connection_script.GetRemotePlayer(args["playerHash"]);
                Transform target_hand = player.transform.Find("Head/Hand");
                target_hand.gameObject.SetActive((bool) (args["isLightOn"]=="True"));
            }
        }
        catch (Exception e) {
            Debug.Log(e);
        }
    }

    // Requires:
    //      seed
    public void seedRng(Dictionary<string, string> args) {
        client_connection_script.rSeed = Int32.Parse(args["seed"]);
    }

    public void monsterAction(Dictionary<string, string> args)
    {
        try
        {
            GameObject player = client_connection_script.GetRemotePlayer(args["playerHash"]);
            if (player != null && player != client_connection_script.current_creature_script.getGameObject())
            {
                Animator target_animator = player.GetComponent<Animator>();
                if(args.ContainsKey("action"))
                {
                    target_animator.SetTrigger(args["action"]);
                }
                else if (args.ContainsKey("movementState"))
                {
                    target_animator.SetInteger("movementState", int.Parse(args["movementState"]));
                }
            }

            string hitName = args["playerHit"];
            if (hitName != "")
            {
                GameObject playerHit = client_connection_script.GetRemotePlayer(hitName);
                if (playerHit == client_connection_script.current_creature_script.getGameObject())
                {
                    playerHit.GetComponent<PlayerController>().Die();
                }
                else
                {
                    playerHit.SetActive(false);
                }
            }
            
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void selectCharacter(Dictionary<string,string> args)
    {
        try
        {
            GameObject player = client_connection_script.GetRemotePlayer(args["playerHash"]);

            if (args["deselect"] == "False")
            {
                GameObject.Find("CharacterSelectors")
                    .GetComponent<CharacterSelectionController>()
                    .CharacterSelectedResponse(args);            
            }
            else
            {
                GameObject.Find("CharacterSelectors")
                    .GetComponent<CharacterSelectionController>()
                    .CharacterDeselectedResponse(args);
            }
            
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}