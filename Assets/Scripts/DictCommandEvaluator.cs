using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DictCommandEvaluator {
    PlayerController parent_guy_script = GameObject.FindObjectOfType<PlayerController>();
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
            if (args["playerHash"] != parent_guy_script.get_player_hash()) {
                GameObject player = client_connection_script.GetPlayer(args["playerHash"]);
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
    public void seedCrystal(Dictionary<string, string> args) {
        parent_guy_script.rSeed = Int32.Parse(args["seed"]);
    }

    public void monsterAction(Dictionary<string, string> args)
    {
        try
        {
            GameObject player = client_connection_script.GetPlayer(args["playerHash"]);
            if (player != parent_guy_script.gameObject)
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
                GameObject playerHit = client_connection_script.GetPlayer(hitName);
                if (playerHit == parent_guy_script)
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
            GameObject player = client_connection_script.GetPlayer(args["playerHash"]);

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