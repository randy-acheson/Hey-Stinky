using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DictCommandEvaluator {
    PlayerController parent_guy_script = GameObject.FindObjectOfType<PlayerController>();

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
            GameObject player = parent_guy_script.GetPlayer(args["playerHash"]);
            if (player != parent_guy_script.gameObject) {
                Transform target_hand = player.transform.Find("Head/Hand");
                target_hand.gameObject.SetActive((bool) (args["isLightOn"]=="True"));
            }
        }
        catch (Exception e) {
            Debug.Log(e);
        }
    }
}