using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



public class BCI2000StateSender : MonoBehaviour
{


    [SerializeField] private GameObject BCIObject;

    private UnityBCI2000 bci;


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
    [SerializeField] bool Velocity = false;
    [SerializeField] int VelScale = 1;


    private List<SendState> variables = new List<SendState>();


    // Start is called before the first frame update
    void Start()
    {
        bci = BCIObject.GetComponent<UnityBCI2000>();


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
        if (Velocity)
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            if (rigidbody == null) //there is no rigidbody, so there must be a rigidbody2d
            {
                Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
                AddSendState("Velocity", UnityBCI2000.StateType.UnsignedInt32, new Func<float>(() => rigidbody2D.velocity.magnitude), VelScale);
            }
            else
            {
                AddSendState("Velocity", UnityBCI2000.StateType.UnsignedInt32, new Func<float>(() => rigidbody.velocity.magnitude), VelScale);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (SendState state in variables)
        {
            state.Update();
        }
    }


    public void AddSendState(string name, UnityBCI2000.StateType type, Func<float> value, int scale)
    {

        string nameNoWS = new string(gameObject.name.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());
        UnityBCI2000.StateVariable state = bci.AddState(nameNoWS + name, type);
        variables.Add(new SendState(state, value, scale));
    }
    public void AddSendState(string name, UnityBCI2000.StateType type, Func<float> value)
    {
        AddSendState(name, type, value, 1);
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
    public void AddSendExistingState(string name, Func<float> value)
    {
        AddSendExistingState(name, value, 1);
    }

    public void AddSendExistingState(UnityBCI2000.StateVariable state, Func<float> value, int scale)
    {
        variables.Add(new SendState(state, value, scale));
    }
    public void AddSendExistingState(UnityBCI2000.StateVariable state, Func<float> value)
    {
        AddSendExistingState(state, value, 1);
    }


    [CustomEditor(typeof(BCI2000StateSender))]
    public class StateSenderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var sender = target as BCI2000StateSender;


            sender.BCIObject = (GameObject)EditorGUILayout.ObjectField("UnityBCI2000 object", sender.BCIObject, typeof(GameObject), true);
            sender.screenCamera = (Camera)EditorGUILayout.ObjectField("Camera", sender.screenCamera, typeof(Camera), true);


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
            sender.IsOnScreen = EditorGUILayout.Toggle("Is on screen", sender.IsOnScreen);

            //check for rigidbody before showing velocity toggle
            if (sender.gameObject.GetComponent<Rigidbody>() != null || sender.gameObject.GetComponent<Rigidbody2D>() != null)
            {
                sender.Velocity = EditorGUILayout.Toggle("Velocity", sender.Velocity);
                if (sender.Velocity)
                    sender.VelScale = EditorGUILayout.IntField("Scale", sender.VelScale);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }




    private class SendState //class which sends values to a StateVariable. Two of these can point to the same StateVariable, use AddSendExistingState()
    {
        public string Name { get; }
        public int Scale { get; }
        public Func<float> StoredVar { get; }
        private UnityBCI2000.StateVariable state;

        public SendState(UnityBCI2000.StateVariable inState, Func<float> storedVar, int scale)
        {
            Scale = scale;
            StoredVar = storedVar;
            state = inState;
        }
        public void Update()
        {
            state.Set((int)(StoredVar.Invoke() * Scale));
        }
    }
}
