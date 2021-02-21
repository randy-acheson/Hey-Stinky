using System;
using System.Collections.Generic;
using System.Reflection;

using PlayerController;

namespace CommandEvaluators
{
    public abstract class CommandEvaluator {
    
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
    }

    public class FlashLightCommandEvaluator : CommandEvaluator {
        // Requires:
        //     playerHash;
        //     isLightOn;

        public void toggleFlashlight(Dictionary<string, string> args) {
            GameObject player = getPlayer(args["playerHash"]);
            player.find("/head/hand/flashlight").enabled = args["isLightOn"];
        }
    }
}