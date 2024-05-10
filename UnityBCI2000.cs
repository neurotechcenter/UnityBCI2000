using BCI2000RemoteNET;

public class UnityBCI2000 : MonoBehaviour {

    ///<summary>
    ///BCI2000Remote instance which handles control of BCI2000.
    ///</summary>
    public BCI2000Remote control;

    ///<summary>
    ///Start a local operator module listening on the address specified by OperatorAddress and OperatorPort
    ///</summary>
    public bool StartLocalOperator = true;

    ///<summary>
    ///Path to the Operator binary when starting on the local machine.
    ///</summary>
    public string OperatorPath;

    ///<summary>
    ///IP address of the remote operator. This also sets the address at which the local operator listens for connections if <see cref="LocalOperator"/> is set.
    ///Note: Exposing an operator port to the open internet, such as when connecting to an operator not on the local network, is not recommended (do not do it), because the connection to BCI2000 is unsecured and unencrypted, and can be used to run arbitrary shell commands on the remote system. Connecting to an instance of BCI2000 on another machine on the same local network should be safe, so long as the port at which BCI2000 is listening is not forwarded by the router.
    ///</summary>
    public string OperatorAddress = "127.0.0.1";

    ///<summary>
    ///Port of the remote operator. This also sets the port at which the local operator listens for connections if <see cref="LocalOperator"/> is set.
    ///</summary>
    public int OperatorPort = 3999;

    ///<summary>
    ///Amount of time (in milliseconds) that the underlying tcp connection will block before failing when sending or receiving data. This will generally never need to be changed.
    ///</summary>
    public int ConnectTimeout = 1000; 

    ///<summary>
    ///Start collecting data when the scene starts. If you want to start BCI2000 manually instead, call <c>control.Start()</c> 
    ///If you are starting a BCI2000 run manually rather than setting StartWithScene (via <c>control.Start()</c>), avoid starting the run during scene initialization (i.e. in <c>Awake()</c> or <c>Start()</c> within a script), as this may cause the <see cref="OnConnect"/> commands to fail, depending on the order in which each <c>GameObject</c> initializes.
    ///</summary>
    public bool StartWithScene = true;

    ///<summary>
    ///Stop collecting data when scene stops. If you want to stop BCI2000 manually instead, call <c>control.Stop()</c>
    ///</summary>
    public bool StopWithScene = true;

    ///<summary>
    ///Shut down BCI2000 when scene stops. This is useful for using BCI2000 as an extension of your Unity application.
    ///</summary>
    public bool ShutdownWithScene = false;

    ///<summary>
    ///List of paths to parameter files to load
    ///</summary>
    public List<string> parameterFiles;

    ///<summary>
    ///Add an action to run immediately after connecting to BCI2000, before modules have started. This is useful for adding states, events, and parameters. This must be called within <c>Awake()</c> or it will have no effect. 
    ///For example, to add an event:
    ///<code>
    ///UnityBCI2000 bci = ...
    ///bci.OnEvent(remote => remote.AddEvent("event"));
    ///</code>
    ///<paramref name="action"/> is an <c>Action<BCI2000Connection></c> which will be called with this object's BCI2000Connection. This can take the form of a lambda which takes one parameter, that being the <c>BCI2000Remote</c> object. 
    ///If you are starting a BCI2000 run manually rather than setting StartWithScene (via <c>control.Start()</c>), avoid starting the run during scene initialization (i.e. in <c>Awake()</c> or <c>Start()</c> within a script), as this may cause the OnConnect commands to fail, depending on the order in which each <c>GameObject</c> initializes.
    ///</summary>
    ///<param name="action">The action which will be taken immediately after connecting to BCI2000, while it is in the Idle state. It takes one parameter, which will be this object's <c>BCI2000Remote</c> instance.
    public void OnConnect(Action<BCI2000Remote> action) {
	onConnect.Add(action);
    }

    public string Module1 = "SignalGenerator";
    public string[] Module1Args;

    public string Module2 = "DummySignalProcessing";
    public string[] Module2Args;

    public string Module3 = "DummyApplication";
    public string[] Module3Args;

    private List<Action<BCI2000Remote>> onConnect = new List<Action<BCI2000Remote>>();

    private BCI2000Connection = new BCI2000Connection();
    private BCI2000Remote = new BCI2000Remote();


    void Awake(){
	control = new BCI2000Remote(new BCI2000Connection));
	if (StartLocalOperator) {
	    control.connection.StartOperator(OperatorPath, OperatorAddress, OperatorPort);
	}
	control.Connect(OperatorAddress, OperatorPort);
    }


}
