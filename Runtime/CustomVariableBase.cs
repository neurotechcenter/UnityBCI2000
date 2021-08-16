using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

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
        foreach (CustomVariable customVar in customVariables) //uses reflection so that there can be one central custom variable list, ths is only called before the scene loads, so overhead doesnt matter.
        {
            if (customVar is CustomSendVariable)
                Sender.AddCustomSendVariable(customVar.Name, (Func<float>)Delegate.CreateDelegate(customVar.DelegateType, customVar.Target, customVar.Method), customVar.Scale, customVar.Type);
            else if (customVar is CustomGetVariable)
                Sender.AddCustomGetVariable(customVar.Name, (Action<int>)Delegate.CreateDelegate(customVar.DelegateType, customVar.Target, customVar.Method), customVar.Scale, customVar.Type);
        }
    }

    public void InitializeEditor() //The editor only displays the name of each custom variable
    {
        InitializeList();
        foreach (CustomVariable customVar in customVariables)
        {
            if (customVar is CustomSendVariable)
                Sender.EditorAddCustomVariable(customVar.Name, false);
            else
                Sender.EditorAddCustomVariable(customVar.Name, true);
        }
    }

    public class CustomVariable
    {
        public string Name;
        public UnityBCI2000.StateType Type;
        public int Scale;
        public Type DelegateType;
        public Object Target;
        public MethodInfo Method;


        public CustomVariable(string name, UnityBCI2000.StateType type, int scale)
        {
            Name = name;
            Type = type;
            Scale = scale;
        }
    }

    public class CustomSendVariable : CustomVariable
    {
        public CustomSendVariable(string name, Func<float> value, UnityBCI2000.StateType type, int scale) : base(name, type, scale)
        {
            DelegateType = typeof(Func<float>);
            Target = value.Target;
            Method = value.Method;
        }
    }

    public class CustomGetVariable : CustomVariable
    {
        public CustomGetVariable(string name, Action<int> action, UnityBCI2000.StateType type, int scale) : base(name, type, scale)
        {
            DelegateType = typeof(Action<int>);
            Target = action.Target;
            Method = action.Method;
        }
    }
}
