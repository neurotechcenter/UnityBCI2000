using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



public class BCI2000StateSender : MonoBehaviour, ISerializationCallbackReceiver
{


    [SerializeField] private GameObject BCIObject;

    private UnityBCI2000 bci;

    [SerializeField] private CustomVariableBase customVarsObject;

    [SerializeField] bool GlobalCoords;
    [SerializeField] bool GlobalX;
    [SerializeField] int GXScale = 1000;
    [SerializeField] bool GlobalY;
    [SerializeField] int GYScale = 1000;
    [SerializeField] bool GlobalZ;
    [SerializeField] int GZScale = 1000;


    [SerializeField] bool ScreenPosition;

    [SerializeField] Camera screenCamera;

    [SerializeField] bool ScreenX;
    [SerializeField] int SXScale = 1;
    [SerializeField] bool ScreenY;
    [SerializeField] int SYScale = 1;
    [SerializeField] bool ScreenZ;
    [SerializeField] int SZScale = 1;

    [SerializeField] bool IsOnScreen;
    [SerializeField] bool Interaction;
    [SerializeField] bool Speed = false;
    [SerializeField] int VelScale = 1;


    private List<StateBase> variables = new List<StateBase>();

    [SerializeField] private bool showCustomVars;
    //serialized list of custom variables, so they can be held outside of play mode.
    [SerializeField]
    private List<string> customSendVariables = new List<string>();
    [SerializeField]
    private List<string> customGetVariables = new List<string>();


    // Start is called before the first frame update
    void Start()
    {

        bci = BCIObject.GetComponent<UnityBCI2000>();
        if (customVarsObject != null)
        {
            customVarsObject.Sender = this;
        }

        if (GlobalX)
        {
            AddSendState("GlobalX", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => gameObject.transform.position.x), GXScale);
        }
        if (GlobalY)
        {
            AddSendState("GlobalY", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => gameObject.transform.position.y), GYScale);
        }
        if (GlobalZ)
        {
            AddSendState("GlobalZ", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => gameObject.transform.position.z), GZScale);
        }
        if (ScreenX)
        {
            AddSendState("ScreenX", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => screenCamera.WorldToScreenPoint(gameObject.transform.position).x), SXScale);
        }
        if (ScreenY)
        {
            AddSendState("ScreenY", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => screenCamera.WorldToScreenPoint(gameObject.transform.position).y), SYScale);
        }
        if (ScreenZ)
        {
            AddSendState("ScreenZ", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => screenCamera.WorldToScreenPoint(gameObject.transform.position).z), SZScale);
        }

        if (IsOnScreen)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
                AddSendState("Is on screen", UnityBCI2000.StateType.Boolean, new Func<float>(() => {
                    if (renderer.isVisible)
                        return 1;
                    else
                        return 0;
                }), 1);
        }

        if (Speed)
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            if (rigidbody == null) //there is no rigidbody, so there must be a rigidbody2d
            {
                Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
                AddSendState("Speed", UnityBCI2000.StateType.UnsignedInt32, new Func<float>(() => rigidbody2D.velocity.magnitude), VelScale);
            }
            else
            {
                AddSendState("Speed", UnityBCI2000.StateType.UnsignedInt32, new Func<float>(() => rigidbody.velocity.magnitude), VelScale);
            }
        }

        if (customVarsObject != null)
        {
            customVarsObject.InitializeRuntime();
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (StateBase state in variables)
        {
            state.Update();
        }
    }


    public void AddSendState(string name, UnityBCI2000.StateType type, Func<float> value, int scale)
    {

        UnityBCI2000.StateVariable state = bci.AddState(GetStateNameNoWS(name), type);
        int scale2 = scale;
        if (type == UnityBCI2000.StateType.Boolean) //scale must be 1 if the value is boolean
            scale2 = 1;
        variables.Add(new SendState(state, value, scale2));
    }



    public void AddSendExistingState(string name, Func<float> value, int scale)
    {
        UnityBCI2000.StateVariable state = bci.FindState(name);
        if (state == null)
        {
            Debug.Log("State " + name + " does not exist.");
            return;
        }
        variables.Add(new SendState(state, value, scale));
    }


    public void AddSendExistingState(UnityBCI2000.StateVariable state, Func<float> value, int scale)
    {
        variables.Add(new SendState(state, value, scale));
    }


    public void AddGetState(string name, Action<int> action)
    {
        if (bci.FindState(name) == null)
        {
            Debug.LogError("Unable to find state \"" + name + '\"');
            return;
        }

    }


    public void AddCustomSendVariable(string name, Func<float> value, int scale, UnityBCI2000.StateType type)
    {
        if (bci.FindState(GetStateNameNoWS(name)) == null)
            AddSendState(name, type, value, scale);
        else
            AddSendExistingState(name, value, scale);
    }
    public void AddCustomGetVariable(string name, Action<int> action, int scale, UnityBCI2000.StateType type)
    {

    }
    public void EditorAddCustomVariable(string name, bool isGetVariable)//this is only for displaying the names of added custom vars in the editor, as they are not serializable and must be added at runtime
    {
        if (isGetVariable)
            customGetVariables.Add(name);
        else
            customSendVariables.Add(name);
    }



    public void ClearCustomVariables()
    {
        customSendVariables.Clear();
        customGetVariables.Clear();
    }


    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        ClearCustomVariables();
        if (customVarsObject != null)
        {
            if (customVarsObject.Sender == null)
            {
                customVarsObject.Sender = this;
            }
            customVarsObject.InitializeEditor();
        }
    }


    private string GetStateNameNoWS(string stateName)
    {
        string objNameNoWS = new string(gameObject.name.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());//remove whitespace from names
        string nameNoWS = new string(stateName.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());
        return objNameNoWS + nameNoWS;
    }



    [CustomEditor(typeof(BCI2000StateSender))]
    public class StateSenderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var sender = target as BCI2000StateSender;

            sender.BCIObject = (GameObject)EditorGUILayout.ObjectField("UnityBCI2000 object", sender.BCIObject, typeof(GameObject), true);
            sender.customVarsObject = (CustomVariableBase)EditorGUILayout.ObjectField("Custom Variable Supplier", sender.customVarsObject, typeof(CustomVariableBase), true);

            //Global coordinate toggles and scales
            sender.GlobalCoords = EditorGUILayout.Foldout(sender.GlobalCoords, "Global Coordinates", true);
            if (sender.GlobalCoords)
            {
                sender.GlobalX = EditorGUILayout.Toggle("Global X Position", sender.GlobalX);
                if (sender.GlobalX)
                    sender.GXScale = EditorGUILayout.IntField("Scale", sender.GXScale);
                sender.GlobalY = EditorGUILayout.Toggle("Global Y Position", sender.GlobalY);
                if (sender.GlobalY)
                    sender.GYScale = EditorGUILayout.IntField("Scale", sender.GYScale);
                sender.GlobalZ = EditorGUILayout.Toggle("Global Z Position", sender.GlobalZ);
                if (sender.GlobalZ)
                    sender.GZScale = EditorGUILayout.IntField("Scale", sender.GZScale);
            }
            //Screen coordinate toggles and formats
            sender.ScreenPosition = EditorGUILayout.Foldout(sender.ScreenPosition, "Screen Position", true);
            if (sender.ScreenPosition)
            {
                sender.screenCamera = (Camera)EditorGUILayout.ObjectField("Camera", sender.screenCamera, typeof(Camera), true);
                if (sender.screenCamera != null)
                {
                    sender.ScreenX = EditorGUILayout.Toggle("Screen X Position", sender.ScreenX);
                    if (sender.ScreenX)
                        sender.SXScale = EditorGUILayout.IntField("Scale", sender.SXScale);
                    sender.ScreenY = EditorGUILayout.Toggle("Screen Y Position", sender.ScreenY);
                    if (sender.ScreenY)
                        sender.SYScale = EditorGUILayout.IntField("Scale", sender.SYScale);
                    sender.ScreenZ = EditorGUILayout.Toggle("Screen Z Position", sender.ScreenZ);
                    if (sender.ScreenZ)
                        sender.SZScale = EditorGUILayout.IntField("Scale", sender.SZScale);
                }
            }
            sender.IsOnScreen = EditorGUILayout.Toggle("Is on screen", sender.IsOnScreen);


            //check for rigidbody before showing speed toggle
            if (sender.gameObject.GetComponent<Rigidbody>() != null || sender.gameObject.GetComponent<Rigidbody2D>() != null)
            {
                sender.Speed = EditorGUILayout.Toggle("Speed", sender.Speed);
                if (sender.Speed)
                    sender.VelScale = EditorGUILayout.IntField("Scale", sender.VelScale);
            }
            sender.showCustomVars = EditorGUILayout.Foldout(sender.showCustomVars, "Custom Variables");
            if (sender.showCustomVars)
            {
                if (sender.customSendVariables.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Custom Get Variables");

                    foreach (string name in sender.customSendVariables)
                    {
                        EditorGUILayout.LabelField(name);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (sender.customGetVariables.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Custom Get Variables");

                    foreach (string name in sender.customGetVariables)
                    {
                        EditorGUILayout.LabelField(name);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }


    private abstract class StateBase
    {
        public string Name { get; }
        public int Scale { get; }
        public UnityBCI2000.StateVariable state;

        public StateBase(UnityBCI2000.StateVariable inState, int scale)
        {
            Scale = scale;
            state = inState;
        }
        public abstract void Update();
    }

    private class SendState : StateBase //class which sends values to a StateVariable. Two of these can point to the same StateVariable, use AddSendExistingState()
    {

        private Func<float> StoredVar { get; }

        public SendState(UnityBCI2000.StateVariable inState, Func<float> storedVar, int scale) : base(inState, scale)
        {
            StoredVar = storedVar;
        }
        public override void Update()
        {
            state.Set((int)(StoredVar.Invoke() * Scale));
        }
    }

    private class GetState : StateBase
    {
        private Action<int> GetVar;

        public GetState(UnityBCI2000.StateVariable inState, Action<int> getVar) : base(inState, 1)
        {
            GetVar = getVar;
        }

        public override void Update()
        {
            GetVar.Invoke(state.Get());
        }
        public int GetValue()
        {
            return state.Get();
        }
    }
}
