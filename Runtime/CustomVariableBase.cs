using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CustomVariableBase : MonoBehaviour //base script for handling custom variables
{

    public List<CustomVariable> customVariables = new List<CustomVariable>();
    [HideInInspector] public BCI2000StateSender Sender; 

    public abstract void AddCustomVariables(); //override with method that adds custom variables to customVariables
    

    private void InitializeList()//Clears and repopulates list, this is because the list is erased on assembly reload
    {
        customVariables.Clear();
        AddCustomVariables();
    }



    public void InitializeRuntime() //Custom variables cannot be serialized, so they must be reinitialized whenever assembly is reloaded, before the first frame
    {
        InitializeList();
        foreach (CustomVariable customVar in customVariables)
        {
            Sender.AddCustomVariable(customVar.Name, customVar.Value, customVar.Scale, customVar.Type);
        }
    }

    public void InitializeEditor() //The editor only displays the name of each custom variable
    {
        InitializeList();
        foreach (CustomVariable customVar in customVariables)
        {
            Sender.EditorAddCustomVariable(customVar.Name);
        }
    }

    public class CustomVariable
    {
        public string Name;
        public Func<float> Value;
        public int Scale;
        public UnityBCI2000.StateType Type;

        public CustomVariable(string name, Func<float> value, int scale, UnityBCI2000.StateType type)
        {
            Name = name;
            Value = value;
            Scale = scale;
            Type = type;
        }
    }
}
