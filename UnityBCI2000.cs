using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BCI2000RemoteNET;
using UnityEngine;


public class UnityBCI2000 : MonoBehaviour
{
    /// <summary>
    /// The path to the BCI2000 Operator module binary
    /// </summary>
    public string OperatorPath;
    //Use of a remote operator is unsupported due to the fact that UnityBCI2000 will not be able to
    //function correctly with the amount of communication latency that would exist when using a remote operator.
    //public string TelnetIp
    public string TelnetIp;
    //public int TelnetPort;
    public int TelnetPort;
    /// <summary>
    /// Don't start the Source, Processing, or Application modules when starting UnityBCI2000
    /// </summary>
    public bool DontStartModules;
    /// <summary>
    /// Start BCI2000 run alongside the scene. 
    /// </summary>
    public bool StartWithScene = true;
    /// <summary>
    /// Shut down BCI2000 with scene.
    /// </summary>
    public bool ShutdownWithScene = true;
    /// <summary>
    /// The Source Module to start up
    /// </summary>
    public string Module1 = "SignalGenerator";
    /// <summary>
    /// Arguments to pass to the Source module. '--' at the start of each argument is unnecessary.
    /// </summary>
    public string[] Module1Args;
    /// <summary>
    /// The Processing Module to start up
    /// </summary>
    public string Module2 = "DummySignalProcessing";
    /// <summary>
    /// Arguments to pass to the Processing module. '--' at the start of each argument is unnecessary.
    /// </summary>
    public string[] Module2Args;
    /// <summary>
    /// The Application Module to start up. For most use cases, this should be left as `DummyApplication`
    /// because the Unity app should function as its replacement.
    /// </summary>
    public string Module3 = "DummyApplication";
    /// <summary>
    /// Arguments to pass to the Application module. '--' at the start of each argument is unnecessary.
    /// </summary>
    public string[] Module3Args;
    /// <summary>
    /// Commands to run immediately upon startup of BCI2000. These run before any of the modules are started.
    /// </summary>
    public string[] initCommands;
    /// <summary>
    /// The file to store log output
    /// </summary>
    public string LogFile;
    /// <summary>
    /// Log state variable changes
    /// </summary>
    public bool LogStates;
    /// <summary>
    /// Log `>` characters received from BCI2000
    /// </summary>
    public bool LogPrompts;
    /// <summary>
    /// BCI2000 parameter files to be loaded before operaation starts
    /// </summary>
    public List<string> parameterFiles = new List<string>();



    /// <summary>
    /// Adds a state to BCI2000. This must be called within the Start() method of a MonoBehviour to work properly.
    /// The added state has a bit width of 32 and initial state of 0.
    /// </summary>
    /// <param name="name">The name of the state to add</param>
    public void AddState(string name) 
    {
        statenames.Add(name);
    }

    /// <summary>
    /// Adds an event to BCI2000. This must be called within the `Start()` method of a `MonoBehaviour` to work properly
    /// </summary>
    /// <param name="name"></param>
    public void AddEvent(string name)
    {
        eventnames.Add(name);
    }

    /// <summary>
    /// Return the value of the control signal on specified channel and element
    /// </summary>
    /// <param name="channel">The channel of the desired signal</param>
    /// <param name="element">The element of the desired signal</param>
    /// <returns>The value of the signal</returns>
    /// <exception cref="Exception">Channel and element must be greater than or equal to zero.
    /// However, UnityBCI2000 does not check if the channel and element values are valid for the current BCI2000 configuration.</exception>
    public double GetSignal(int channel, int element)
    {
        if (channel < 0)
        {
            throw new Exception("Channel cannot be less than 0");
        }
        if (element < 0)
        {
            throw new Exception("Element cannot be less than 0");
        }
        return bci.GetSignal((uint) channel, (uint) element);
    }
    
    /// <summary>
    /// Sets the value of a selected BCI2000 state variable
    /// </summary>
    /// <param name="name">The name of the desired state variable</param>
    /// <param name="value">The value to set the state to. Values less than zero will be instead sent as zero.</param>
    public void SetState(string name, int value)
    {
        if (afterFirst)
        {
            bci.SetStateVariable(name, (UInt32) Math.Max(value, 0)); // states cannot be negative, values will be set to zero instead
        }

    }

    /// <summary>
    /// Gets the value of a BCI2000 state variable
    /// </summary>
    /// <param name="name">The name of the desired state variable</param>
    /// <returns>The value of the state variable</returns>
    public int GetState(string name)
    {
        if (afterFirst)
        {
            return (int) bci.GetStateVariable(name);
        }
        return 0;
    }

    /// <summary>
    /// Sets the value of a BCI2000 event. This is useful for recording values that change more rapidly than a state variable can handle.
    /// If you are trying to log when something happens, consider using `PulseEvent` instead.
    /// </summary>
    /// <param name="name">The name of the desired event</param>
    /// <param name="value">The value to set the event</param>
    public void SetEvent(string name, int value)
    {
        if (afterFirst)
        {
            bci.SetEvent(name, (UInt32) Math.Max(value, 0));
        }
    }

    /// <summary>
    /// Sets an event to a value for a single sample, then returns it to its previous value.
    /// </summary>
    /// <param name="name">The name of the desired event</param>
    /// <param name="value">The value for the event</param>
    public void PulseEvent(string name, int value)
    {
        if (afterFirst)
        {
            bci.PulseEvent(name, (UInt32) Math.Max(value, 0));
        }
    }

    /// <summary>
    /// Gets the value of an event
    /// </summary>
    /// <param name="eventName">The name of the desired event</param>
    /// <returns>The current value of the event</returns>
    public int GetEvent(string eventName) 
    {
        if (afterFirst)
        {
            return bci.GetEvent(eventName);
        }
        return 0;
    }


    /// <summary>
    /// Adds a parameter to BCI2000. All parameters are treated as strings.
    /// </summary>
    /// <param name="section">The section label for the parameter within BCI2000</param>
    /// <param name="name">The name of the parameter</param>
    /// <param name="defaultValue">The default value of the parameter</param>
    /// <param name="minValue">The parameter's minimum value</param>
    /// <param name="maxValue">The parameter's maximum value</param>
    public void AddParam(string section, string name, string defaultValue, string minValue, string maxValue) {
        bci.AddParameter(section, name, defaultValue, minValue, maxValue);
    }

    /// <summary>
    /// Adds a parameter to BCI2000. All parameters are treated as strings.
    /// </summary>
    /// <param name="section">The section label for the parameter within BCI2000</param>
    /// <param name="name">The name of the parameter</param>
    /// <param name="defaultValue">The default value of the parameter</param>
    public void AddParam(string section, string name, string defaultValue)
    {
        AddParam(section, name, defaultValue, null, null);
    }

    /// <summary>
    /// Adds a parameter to BCI2000. All parameters are treated as strings.
    /// </summary>
    /// <param name="section">The section label for the parameter within BCI2000</param>
    /// <param name="name">The name of the parameter</param>
       public void AddParam(string section, string name)
    {
        AddParam(section, name, null);
    }

    /// <summary>
    /// Gets a parameter value from BCI2000
    /// </summary>
    /// <param name="name">The name of the parameter to get</param>
    /// <returns>The value of the parameter as a string</returns>
    public string GetParam(string name)
    {
        return bci.GetParameter(name);
    }

    /// <summary>
    /// Sets the value of a parameter
    /// </summary>
    /// <param name="name">The name of the parameter to set</param>
    /// <param name="value">The value to set the parameter to</param>
    public void SetParam(string name, string value)
    {
        bci.SetParameter(name, value);  
    }

    private BCI2000Remote bci = new BCI2000Remote();
    private List<string> statenames = new List<string>();
    private List<string> eventnames = new List<string>();
    private bool afterFirst = false;
    private Dictionary<string, List<string>> modules;

    // Start is called before the first frame update
    void Start()
    {

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

        bci.Connect(initCommands, eventnames.ToArray());

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

        foreach (string paramfile in parameterFiles) {
            bci.LoadParameters(paramfile);
        }


    }
    // Update is called once per frame
    void Update()
    {
        if (!afterFirst) //Start and set config, so other scripts can add variables.
        {
            foreach (string state in statenames) //Add all states to BCI2000. these can't be added before or after BCI2000 starts, and must be added here.
            {
                bci.AddStateVariable(state, 32, 0);
            }

            if (StartWithScene)
            {
                bci.SetConfig();
                bci.Start();
            }
            afterFirst = true;
        }


    }
    /*
    public void StartRun()
    {
        bci.Start();
    }
    

    public void StopRun()
    {
        bci.Stop();
    }
    */
    private void OnDestroy()
    {
        if (ShutdownWithScene)
        {
            bci = null;
        }
    }


    private void OnApplicationQuit()
    {
        if (ShutdownWithScene)
        {
            bci.Stop();
        }
    }
    
}
