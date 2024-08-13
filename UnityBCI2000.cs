using BCI2000RemoteNET;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityBCI2000 : MonoBehaviour {

    /// <summary>
    ///The connection to BCI2000. Control BCI2000 by accessing this member to run BCI2000Remote commands directly. For example, to set an event:
    /// <code>
    ///UnityBCI2000 bci = GameObject.Find(...).GetComponent<UnityBCI2000>();
    ///bci.Control.SetEvent("event", 7);
    /// </code>
    ///This property cannot be accessed during scene initialization. In order to run BCI2000 commands during scene startup, use either <c>OnIdle</c> to run BCI2000 commands before its modules have started (for adding events, parameters, etc.), or <c>OnConnected</c> to run commands after modules have initialized. (If StartModules is set to false then OnIdle and OnConnected will be equivalent, they will both run during the <c>Start()</c> method.)
    /// </summary>
    public BCI2000Remote Control {
        private set { control = value; } 
	get {
	    if (!initialized) {
		throw new Exception("Cannot directly run BCI2000 commands during scene initialization. If you wish to run a BCI2000 command, use OnIdle and OnConnected to run commands before or after BCI2000's modules have started");
	    } else {
		return control;
	    }
	}
    }
    /// <summary>
    ///Start a local operator module listening on the address specified by OperatorAddress and OperatorPort
    /// </summary>
    public bool StartLocalOperator = true;

    /// <summary>
    ///Path to the Operator binary when starting on the local machine.
    /// </summary>
    public string OperatorPath;

    /// <summary>
    ///IP address of the remote operator. This also sets the address at which the local operator listens for connections if <see cref="LocalOperator"/> is set.
    ///Note: Exposing an operator port to the open internet, such as when connecting to an operator not on the local network, is not recommended (do not do it), because the connection to BCI2000 is unsecured and unencrypted, and can be used to run arbitrary shell commands on the remote system. Connecting to an instance of BCI2000 on another machine on the same local network should be safe, so long as the port at which BCI2000 is listening is not forwarded by the router.
    /// </summary>
    public string OperatorAddress = "127.0.0.1";

    /// <summary>
    ///Port of the remote operator. This also sets the port at which the local operator listens for connections if <see cref="LocalOperator"/> is set.
    /// </summary>
    public int OperatorPort = 3999;

    /// <summary>
    ///Amount of time (in milliseconds) that the underlying tcp connection will block before failing when sending or receiving data. This will generally never need to be changed.
    /// </summary>
    public int ConnectTimeout = 1000; 

    /// <summary>
    ///Start modules automatically when the scene loads. Set to false if you want to start modules manually, and call either <c>Control.StartupModules()</c> with the required arguments, or this object's <c>StartupModules()</c> method, which starts up modules according to the parameters set in the Unity inspector.
    ///Note on manual control of BCI2000 initialization:
    ///If you wish to control BCI2000 initialization manually, such as if your scene starts with a button press rather than automatically, <c>OnIdle</c> and <c>OnConnected</c> will likely not function as they do when starting BCI2000 automatically. In this case it would be more appropriate to control BCI2000 directly via <c>Control</c>.
    /// </summary>
    public bool StartModules = true;

    /// <summary>
    ///Start collecting data when the scene starts. If you want to start BCI2000 manually instead, call <c>Control.Start()</c> 
    /// <see cref="StartModules"/> for a note on controlling BCI2000 manually.
    /// </summary>
    public bool StartWithScene = true;

    /// <summary>
    ///Stop collecting data when scene stops. If you want to stop BCI2000 manually instead, call <c>Control.Stop()</c>
    /// <see cref="StartModules"/> for a note on controlling BCI2000 manually.
    /// </summary>
    public bool StopWithScene = true;

    /// <summary>
    ///Shut down BCI2000 when scene stops. This is useful for using BCI2000 as an extension of your Unity application.
    /// </summary>
    public bool ShutdownWithScene = false;

    /// <summary>
    ///List of paths to parameter files to load
    /// </summary>
    public List<string> ParameterFiles;

    /// <summary>
    ///Add an action to run immediately after connecting to BCI2000, before modules have started. This is useful for adding states, events, and parameters. This must be called within <c>Awake()</c>.
    ///For example, to add an event:
    /// <code>
    ///UnityBCI2000 bci = ...
    ///bci.OnEvent(remote => remote.AddEvent("event"));
    /// </code>
    /// <paramref name="action"/> is an <c>Action<BCI2000Connection></c> which will be called with this object's BCI2000Connection. This can take the form of a lambda which takes one parameter, that being the <c>BCI2000Remote</c> object. 
    ///If you are starting a BCI2000 run manually rather than setting StartWithScene (via <c>Control.Start()</c>), avoid starting the run during scene initialization (i.e. in <c>Awake()</c> or <c>Start()</c> within a script), as this may cause the OnIdle commands to fail, depending on the order in which each <c>GameObject</c> initializes.
    /// </summary>
    /// <param name="action">The action which will be taken immediately after connecting to BCI2000, while it is in the Idle state. It takes one parameter, which will be this object's <c>BCI2000Remote</c> instance.
    /// <seealso cref="OnConnected"/>
    public void OnIdle(Action<BCI2000Remote> action) {
	if (awake_finished) {
	    throw new Exception("Attempted to call OnIdle() outside of Awake(). OnIdle() must be called inside Awake()");
	}
	onIdle.Add(action);
    }

    /// <summary>
    ///Similar to <c>OnIdle</c>, but runs after BCI2000 modules have started. Can be used for setting parameters defined by modules. If <c>StartModules</c> is set to false, this is equivalent to <c>OnIdle</c>
    /// </summary>
    /// <param name="action"><c>Action<BCI2000Remote></c> which will be invoked immediately after modules are started within the <c>Start()</c> method.
    public void OnConnected(Action<BCI2000Remote> action) {
	if (awake_finished) {
	    throw new Exception("Attempted to call OnConnected() outside of Awake(). OnIdle() must be called inside Awake()");
	}
	onConnected.Add(action);
    }


    ///<summary>
    ///Waits for BCI2000 to be in the specified state. Similar to <c>BCI2000Remote.WaitForSystemState</c>, but is a non-blocking Unity coroutine.
    ///</summary>
    ///<param name="state">The <c>BCI2000Remote.SystemState</c> to wait for </param>
    public IEnumerator PollSystemState(BCI2000Remote.SystemState state) {
	while (Control.GetSystemState() != state) {
	    yield return null;
	}
    }

    private uint lastSrcTime = uint.MaxValue;
    private float lastBlockTime = -1;
    private float sampleTime = 0;
    ///<summary>
    ///Gets the offset into the current block such that the sample was taken exactly one block length ago, for use with GetSignal or GetEvent. This is a bit of a workaround because of how BCI2000 processes data in discrete blocks. Therefore this method must be used carefully. It sends multiple commands to the operator and thus should only be used in a low latency environment, that is, when running on the same machine as the operator. If used, it should be called continuously within an Update() method.
    ///</summary>
    public int CurrentSampleOffset() {
	uint srcTime = control.GetState("SourceTime");
	if (srcTime != lastSrcTime) {
	    lastSrcTime = srcTime;
	    lastBlockTime = Time.time;
	    sampleTime = 1 / float.Parse(control.GetParameter("SamplingRate"));
	}
	return (int) Math.Floor((Time.time - lastBlockTime) / sampleTime);
    }


    public string Module1 = "SignalGenerator";
    public string[] Module1Args;

    public string Module2 = "DummySignalProcessing";
    public string[] Module2Args;

    public string Module3 = "DummyApplication";
    public string[] Module3Args;

    private List<Action<BCI2000Remote>> onIdle = new List<Action<BCI2000Remote>>();
    private List<Action<BCI2000Remote>> onConnected = new List<Action<BCI2000Remote>>();
    
    private bool initialized = false;
    private bool awake_finished = false;


    private BCI2000Remote control;


    public void StartupModules() {
	control.StartupModules(new Dictionary<string, IEnumerable<string>>()
	{
	    {Module1, Module1Args },
	    {Module2, Module2Args },
	    {Module3, Module3Args }
	});
    }

    void Awake(){
	Control = new BCI2000Remote(new BCI2000Connection());
	if (StartLocalOperator) {
	    control.connection.StartOperator(OperatorPath, OperatorAddress, OperatorPort);
	}
	control.connection.Connect(OperatorAddress, OperatorPort);
    }

    void Start() {
	awake_finished = true;
	foreach (Action<BCI2000Remote> action in onIdle) {
	    action(control);
	}
	if (StartModules) {
	    StartupModules();
	    control.WaitForSystemState(BCI2000Remote.SystemState.Connected);
	}
	foreach(string file in ParameterFiles) {
	    control.LoadParameters(file);
	}
	foreach(Action<BCI2000Remote> action in onConnected) {
	    action(control);
	}
	initialized = true;
	if (StartWithScene) {
	    control.Start();
	}
    }

    void OnApplicationQuit() {
	if (StopWithScene) {
	    control.Stop();
	}
	if (ShutdownWithScene) {
	    control.connection.Quit();
	}
    }


}
