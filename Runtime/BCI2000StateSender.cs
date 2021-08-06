using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[System.Serializable]
public class BCI2000StateSender : MonoBehaviour
{
    enum format
    {
        none,
        x1000,
    }

    public GameObject BCIObject;
    private UnityBCI2000 bci;


    [SerializeField] bool GlobalCoords;
    [SerializeField] bool GlobalX;
    [SerializeField] format GXForm = format.x1000;
    [SerializeField] bool GlobalY;
    [SerializeField] format GYForm = format.x1000;
    [SerializeField] bool GlobalZ;
    [SerializeField] format GZForm = format.x1000;


    [SerializeField] bool ScreenPosition;

    [SerializeField] Camera screenCamera;

    [SerializeField] bool ScreenX;
    [SerializeField] format SXForm = format.x1000;
    [SerializeField] bool ScreenY;
    [SerializeField] format SYForm = format.x1000;
    [SerializeField] bool ScreenZ;
    [SerializeField] format SZForm = format.x1000;

    [SerializeField] bool IsOnScreen;
    [SerializeField] bool Interaction;
    [SerializeField] bool Velocity = false;
    [SerializeField] format VelForm = format.none;


    private List<SendState> variables = new List<SendState>();

    [SerializeField] private bool showCustomVars;
    //serialized list of custom variables, so they can be held outside of play mode.
    [SerializeField] private List<(string, Func<float>, int, UnityBCI2000.StateType)> customVariables = new List<(string, Func<float>, int, UnityBCI2000.StateType)>();


    // Start is called before the first frame update
    void Start()
    {
        bci = BCIObject.GetComponent<UnityBCI2000>();


        if (GlobalX)
        {
            AddSendState("GlobalX", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => gameObject.transform.position.x), 1000);
        }
        if (GlobalY)
        {
            AddSendState("GlobalY", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => gameObject.transform.position.y), 1000);
        }
        if (GlobalZ)
        {
            AddSendState("GlobalZ", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => gameObject.transform.position.z), 1000);
        }
        if (ScreenX)
        {
            AddSendState("ScreenX", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => screenCamera.WorldToScreenPoint(gameObject.transform.position).x));
        }
        if (ScreenY)
        {
            AddSendState("ScreenY", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => screenCamera.WorldToScreenPoint(gameObject.transform.position).y));
        }
        if (ScreenZ)
        {
            AddSendState("ScreenZ", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => screenCamera.WorldToScreenPoint(gameObject.transform.position).z));
        }
        if (Velocity)
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            if (rigidbody == null) //there is no rigidbody, so there must be a rigidbody2d
            {
                Rigidbody2D rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
                AddSendState("Velocity", UnityBCI2000.StateType.UnsignedInt32, new Func<float>(() => rigidbody2D.velocity.magnitude));
            }
            else
            {
                AddSendState("Velocity", UnityBCI2000.StateType.UnsignedInt32, new Func<float>(() => rigidbody.velocity.magnitude));
            }
        }

        foreach ((string, Func<float>, int, UnityBCI2000.StateType) customVar in customVariables) //adds all the custom variables from the list
        {
            if (bci.FindState("name") == null)
                AddSendState(customVar.Item1, customVar.Item4, customVar.Item2, customVar.Item3);
            else
                AddSendExistingState(customVar.Item1, customVar.Item2, customVar.Item3);
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


    private void AddSendState(string name, UnityBCI2000.StateType type, Func<float> value, int scale)
    {

        string nameNoWS = new string(gameObject.name.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());
        UnityBCI2000.StateVariable state = bci.AddState(nameNoWS + name, type);
        variables.Add(new SendState(state, value, 1000));
    }



    private void AddSendExistingState(string name, Func<float> value, int scale)
    {
        UnityBCI2000.StateVariable state = bci.FindState(name);
        if (state == null)
        {
            Debug.Log("State " + name + " does not exist.");
            return;
        }
        variables.Add(new SendState(state, value, scale));
    }


    private void AddSendExistingState(UnityBCI2000.StateVariable state, Func<float> value, int scale)
    {
        variables.Add(new SendState(state, value, scale));
    }



    public void AddCustomVariable(string name, Func<float> value, int scale, UnityBCI2000.StateType type)
    {
        customVariables.Add((name, value, scale, type));
    }
    public void AddCustomVariable(string name, Func<float> value, UnityBCI2000.StateType type)
    {
        AddCustomVariable(name, value, 1, type);
    }



    [CustomEditor(typeof(BCI2000StateSender))]
    public class StateSenderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var sender = target as BCI2000StateSender;


            sender.BCIObject = (GameObject)EditorGUILayout.ObjectField("UnityBCI2000 object", sender.BCIObject, typeof(GameObject), true);
            sender.screenCamera = (Camera)EditorGUILayout.ObjectField("Camera", sender.screenCamera, typeof(Camera), true);


            //Global coordinate toggles and formats
            sender.GlobalCoords = EditorGUILayout.Toggle("Global Coordinates", sender.GlobalCoords);
            if (sender.GlobalCoords)
            {
                sender.GlobalX = EditorGUILayout.Toggle("Global X Position", sender.GlobalX);
                if (sender.GlobalX)
                    sender.GXForm = (format)EditorGUILayout.EnumPopup("Global X Position Format", sender.GXForm);
                sender.GlobalY = EditorGUILayout.Toggle("Global Y Position", sender.GlobalY);
                if (sender.GlobalY)
                    sender.GYForm = (format)EditorGUILayout.EnumPopup("Global Y Position Format", sender.GYForm);
                sender.GlobalZ = EditorGUILayout.Toggle("Global Z Position", sender.GlobalZ);
                if (sender.GlobalZ)
                    sender.GZForm = (format)EditorGUILayout.EnumPopup("Global Z Position Format", sender.GZForm);
            }
            //Screen coordinate toggles and formats
            sender.ScreenPosition = EditorGUILayout.Toggle("Screen Position", sender.ScreenPosition);
            if (sender.ScreenPosition)
            {
                sender.ScreenX = EditorGUILayout.Toggle("Screen X Position", sender.ScreenX);
                if (sender.ScreenX)
                    sender.SXForm = (format)EditorGUILayout.EnumPopup("Screen X Position Format", sender.SXForm);
                sender.ScreenY = EditorGUILayout.Toggle("Screen Y Position", sender.ScreenY);
                if (sender.ScreenY)
                    sender.SYForm = (format)EditorGUILayout.EnumPopup("Screen Y Position Format", sender.SYForm);
                sender.ScreenZ = EditorGUILayout.Toggle("Screen Z Position", sender.ScreenZ);
                if (sender.ScreenZ)
                    sender.SZForm = (format)EditorGUILayout.EnumPopup("Screen Z Position Format", sender.SZForm);

            }
            sender.IsOnScreen = EditorGUILayout.Toggle("Is on screen", sender.IsOnScreen);


            //check for rigidbody before showing velocity toggle
            if (sender.gameObject.GetComponent<Rigidbody>() != null && sender.gameObject.GetComponent<Rigidbody2D>() != null)
            {
                sender.Velocity = EditorGUILayout.Toggle("Velocity", sender.Velocity);
                if (sender.Velocity)
                    sender.VelForm = (format)EditorGUILayout.EnumPopup("Velocity format", sender.VelForm);
            }
            sender.showCustomVars = EditorGUILayout.Foldout(sender.showCustomVars, "Custom Variables");
            if (sender.showCustomVars)
            {
                for (int ii = 0; ii < sender.customVariables.Count; ii++)
                {
                    (string, Func<float>, int, UnityBCI2000.StateType) customVar = sender.customVariables[ii];
                    EditorGUILayout.LabelField(customVar.Item1);
                    int scale = EditorGUILayout.IntField(customVar.Item3);
                    sender.customVariables[ii] = (customVar.Item1, customVar.Item2, scale, customVar.Item4); //changes scale
                }
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
