using System.Collections;
using System.Linq;
using System;
using System.Threading;
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
    /// Send and recieve timeout in ms
    /// If non-Unity modules take a long time to start, the connection will be terminated if this value is not adjusted.
    /// </summary>
    public int Timeout = 1000;
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
    public string[] InitCommands;
    /// <summary>
    /// The file to store log output
    /// </summary>
    public string LogFile = "BCI2000Log.txt";
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
    /// Adds an event to BCI2000 with bit width 32. This must be called within the `Start()` method of a `MonoBehaviour` to work properly
    /// </summary>
    /// <param name="name"></param>
    public void AddEvent(string name)
    {
        eventnames.Add((name, 32));
    }


    /// <summary>
    /// Adds an event to BCI2000 with bit width given by width
    /// </summary>
    /// <param name="name">The parameter name</param>
    /// <param name="width">The parameter bit width. Must be within range 1..32, default 32</param>
    /// <exception cref="Exception">Width must be between 1 and 32.</exception>
    public void AddEvent(string name, uint width = 32)
    {
        if (width < 1 || width > 32)
        {
            throw new Exception($"Event {name} has width {width} which is outside the range 0..32");
        }

        eventnames.Add((name, width));   
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
        return bci.GetSignal(channel, element);
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
        paramAddCmds.Enqueue("add parameter " + section + " string " + name + "= " + defaultValue + " " + defaultValue + " " + minValue + " " + maxValue);
    }

    /// <summary>
    /// Adds a parameter to BCI2000. All parameters are treated as strings.
    /// </summary>
    /// <param name="section">The section label for the parameter within BCI2000</param>
    /// <param name="name">The name of the parameter</param>
    /// <param name="defaultValue">The default value of the parameter</param>
    public void AddParam(string section, string name, string defaultValue)
    {
        paramAddCmds.Enqueue("add parameter " + section + " string " + name + "= " + defaultValue + " " + defaultValue + " % %");
    }

    /// <summary>
    /// Adds a parameter to BCI2000. All parameters are treated as strings. Can only be called within Start(), otherwise, will not work.
    /// </summary>
    /// <param name="section">The section label for the parameter within BCI2000</param>
    /// <param name="name">The name of the parameter</param>
       public void AddParam(string section, string name)
    {
        Debug.Log("Add parameter " + section + " string " + name + "= % % % %");
        paramCmds.Enqueue("Add parameter " + section + " string " + name + "= % % % %");
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
        paramCmds.Enqueue("set parameter " + name + " " + value);
    }

    /// <summary>
    /// Executes a BCI2000 operator script command, information for which can be found <see href="https://www.bci2000.org/mediawiki/index.php/Technical_Reference:Operator_Library">here.</see>
    /// If called before BCI2000's modules connect, the command will be sent after the modules initialize. This is to prevent sending of commands while the operator is not ready, as the operator's usual response to being sent an unwanted command is to shut down communication entirely.
    /// Most things can be done with this command, other than those which must be done while the operator is in idle state, such as adding events or parameters. Use <see cref="AddEvent">AddEvent</see> and <see cref="AddParam">AddParam</see> instead.
    /// </summary>
    /// <param name="command">The command to send</param>
    /// <param name="now">Execute the command now, regardless of if the operator is ready. This will not work for most commands.</param>
    public void ExecuteCommand(string command, bool now = false)
    {
        if (!isStarted && !now)
        {
            cmds.Enqueue(command);
        }
        else
        {
            bci.SimpleCommand(command);
        }
    }

    private BCI2000Remote bci = new BCI2000Remote();
    private List<string> statenames = new List<string>();
    private List<(string, uint)> eventnames = new List<(string, uint)>();
    private Queue<string> paramCmds = new Queue<string>();
    private Queue<string> cmds = new Queue<string>();
    private bool afterFirst = false;
    private Dictionary<string, List<string>> modules;
    private Queue<string> paramAddCmds = new Queue<string>();
    //Is set to true on first Update();
    private bool isStarted = false;
        
    // Start is called before the first frame update
    void Start()
    {

        bci.WindowVisible = 1;
        bci.OperatorPath = OperatorPath;
        if (!String.IsNullOrWhiteSpace(TelnetIp))
            bci.TelnetIp = TelnetIp;
        if (TelnetPort != 0)
            bci.TelnetPort = TelnetPort;
        bci.Timeout = Timeout;
        if (!String.IsNullOrWhiteSpace(LogFile))
            bci.LogFile = LogFile;
        bci.LogStates = LogStates;
        bci.LogPrompts = LogPrompts;

        List<string> initCmdsWithParams = new List<string>(InitCommands);
        initCmdsWithParams.AddRange(paramAddCmds);

        bci.Connect(initCmdsWithParams.ToArray(), eventnames.ToArray());

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

        bci.WaitForSystemState("Connected");

        foreach (string paramfile in parameterFiles) {
            bci.LoadParameters(paramfile);
        }

        foreach (string cmd in cmds)
        {
            bci.SimpleCommand(cmd);
        }

        foreach (string c in paramCmds)
        {
            bci.SimpleCommand(c);
        }
        isStarted = true;
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


    public void StartRun()
    {
        bci.SetConfig();
        bci.Start();
    }

    public void StopRun()
    {
        bci.Stop();
    }
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
