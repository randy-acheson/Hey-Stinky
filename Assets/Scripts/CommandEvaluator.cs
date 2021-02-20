// using UnityEngine;
using System;
using System.Reflection;

public class CommandEvaluator {
    
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

    public void helloWorld() {
        System.Console.WriteLine("Hellow World");
    }

    public void printList(int[] list) {
        foreach (var item in list)
        {
            System.Console.WriteLine(item);
        }
    }
}