using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BCI2000RemoteNET;
using UnityEngine;


public class UnityBCI2000 : MonoBehaviour
{

    private BCI2000Remote bci;
    public string OperatorPath;
    public string TelnetIp;
    public int TelnetPort;
    public bool DontStartModules;
    public string Module1;
    public string[] Module1Args;
    public string Module2;
    public string[] Module2Args;
    public string Module3;
    public string[] Module3Args;
    private Dictionary<string, List<string>> modules;
    public string LogFile;
    public bool LogStates;
    public bool LogPrompts;

    private List<StateVariable> states;

    public enum StateType //copy this to any object which sends states in Start(), don't want to be copying this every frame
    {
        UnsignedInt32,
        SignedInt32,
        UnsignedInt16,
        SignedInt16,
        Boolean
    }

    public void SetState(string name, int value, StateType defaultType) //Default type is the type to use if the state does not exist
    {
        StateVariable state = states.Find(x => x.Name == name);
        if (state == null)
        {
            state = new StateVariable(name, defaultType, bci);
            states.Add(state);
            state.Set(value);
        }
        else
        {
            state.Set(value);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        bci = new BCI2000Remote();

        bci.WindowVisible = 1;
        bci.OperatorPath = OperatorPath;
        if (!String.IsNullOrWhiteSpace(TelnetIp))
            bci.TelnetIp = TelnetIp;
        if (TelnetPort != 0)
            bci.TelnetPort = TelnetPort;
        if (!String.IsNullOrWhiteSpace(LogFile))
            bci.LogFile = LogFile;
        bci.LogStates = LogStates;
        bci.LogPrompts = LogPrompts;

        bci.Connect();

        List<string> module1ArgsList;
        if (Module1Args.Length == 0)
            module1ArgsList = null;
        else
            module1ArgsList = Module1Args.ToList();
        List<string> module2ArgsList;
        if (Module2Args.Length == 0)
            module2ArgsList = null;
        else
            module2ArgsList = Module2Args.ToList();
        List<string> module3ArgsList;
        if (Module3Args.Length == 0)
            module3ArgsList = null;
        else
            module3ArgsList = Module3Args.ToList();

        if (!DontStartModules)
        {
            bci.StartupModules(new Dictionary<string, List<string>>()
            {
            {Module1, module1ArgsList },
            {Module2, module2ArgsList },
            {Module3, module3ArgsList }
            });
        }
        bci.SetConfig();
        bci.Start();
    }

    // Update is called once per frame
    void Update()
    {


    }

    public void StartRun()
    {
        bci.Start();
    }

    public void StopRun()
    {
        bci.Stop();
    }



    private void OnDestroy()
    {
        bci = null;
    }


    private class StateVariable
    {
        public string Name { get; }
        public StateType Type { get; }
        private readonly BCI2000Remote bci;
        public StateVariable(string name, StateType type, BCI2000Remote inBci)
        {
            Name = name;
            bci = inBci;
            Type = type;
            switch (Type)
            {
                case StateType.Boolean:
                    bci.AddStateVariable(Name, 1, 0);
                    break;
                case StateType.UnsignedInt32:
                    bci.AddStateVariable(Name, 32, 0);
                    break;
                case StateType.SignedInt32:
                    bci.AddStateVariable(Name, 32, 0);
                    bci.AddStateVariable(Name + "Sign", 1, 0);
                    break;
                case StateType.UnsignedInt16:
                    bci.AddStateVariable(Name, 16, 0);
                    break;
                case StateType.SignedInt16:
                    bci.AddStateVariable(Name, 16, 0);
                    bci.AddStateVariable(Name + "Sign", 1, 0);
                    break;
            }
        }

        public void Set(int value)
        {
            switch (Type)
            {
                case StateType.Boolean:
                    if (value == 0)
                        bci.SetStateVariable(Name, 0);
                    else
                        bci.SetStateVariable(Name, 1);
                    break;
                case StateType.SignedInt16:
                case StateType.SignedInt32:
                    bci.SetStateVariable(Name, Mathf.Abs(value));
                    if (value < 0)
                        bci.SetStateVariable(Name, 1);
                    else
                        bci.SetStateVariable(Name + "Sign", 0);
                    break;
                case StateType.UnsignedInt16:
                case StateType.UnsignedInt32:
                    bci.SetStateVariable(Name, value);
                    break;
            }
        }
    }
}
