using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BCI2000StateSender : MonoBehaviour
{
    enum format
    {
        none,
        x1000,
    }

    public GameObject BCIObject;
    private UnityBCI2000 bci;


    bool GlobalX;
    format GXForm = format.x1000;
    bool GlobalY;
    format GYForm = format.x1000;
    bool GlobalZ;
    format GZForm = format.x1000;
    bool ScreenX;
    format SXForm = format.x1000;
    bool ScreenY;
    format SYForm = format.x1000;
    bool ScreenZ;
    format SZForm = format.x1000;
    bool IsOnScreen;
    bool Interaction;
    bool Velocity;
    format VelForm = format.none;


    private List<SendState> variables = new List<SendState>();


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
            AddSendState("GlobalY", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => gameObject.transform.position.x), 1000);
        }                                         
        if (GlobalZ)                              
        {
            AddSendState("GlobalZ", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => gameObject.transform.position.x), 1000);
        }                                         
        if (ScreenX)                              
        {
            AddSendState("ScreenX", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => gameObject.transform.position.x));
        }                                         
        if (ScreenY)                              
        {
            AddSendState("ScreenY", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => gameObject.transform.position.x));
        }                                         
        if (ScreenZ)                              
        {
            AddSendState("ScreenZ", UnityBCI2000.StateType.SignedInt32, new Func<float>(() => gameObject.transform.position.x));
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
        UnityBCI2000.StateVariable state = bci.AddState(gameObject.name + name, type);
        variables.Add(new SendState(state, value, 1000));
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

    private class SendState //class which sends values to a StateVariable. Two of these can point to the same StateVariable, use AddSendExistingState()
    {
        public string Name { get; }
        public int Scale { get; }
        public Func<float> StoredVar { get; }
        private UnityBCI2000.StateVariable state;

        public SendState(UnityBCI2000.StateVariable inState, Func<float> storedVar, int scale ) {
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
