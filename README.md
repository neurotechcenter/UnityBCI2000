UnityBCI2000
===
Unity package which integrates BCI2000

Dependencies
---
UnityBCI2000 depends on the BCI2000RemoteNET library, located at
[GitHub](https://github.com/neurotechcenter/BCI2000RemoteNET) or [NuGet](https://www.nuget.org/packages/BCI2000RemoteNET).
Keep in mind that at the moment both BCI2000RemoteNET and UnityBCI2000 are not fully released,
and are still in development, so features and API structure are subject to change at any time,
and bugs are to be expected.


Description
---

BCI2000 is a set of programs for brain and brain-computer interface research.  
Data is sent to BCI2000 using variables called 'states'. A state is simply an unsigned
integer of 1 to 32 bits which is sent over TCP to the BCI2000 operator module.

Usage
---

Add the `UnityBCI2000.cs` script to one `GameObject`. This is the central script which communicates with BCI2000.  
Add the `BCI2000StateSender` to any object which you want to monitor with BCI2000.  
Before starting, you must set the operator and modules to start up, by specifying the fields
`OperatorPath` and `Module[1-3]`. All other properties have default settings and will function without change.  
Alternatively, if you leave `OperatorPath` blank, `BCI2000Remote` will attempt to connect to an operator at
`TelnetIp:TelnetPort`. This is useful for if you already have an operator open that you want to use.  
If you also have modules started within a running BCI2000 instance, set `DontStartModules` to `true`, this will preserve
your running modules.


### Adding and calling events

Adding events must be done in the `Start()` method of a `MonoBehaviour`.
This is done by calling `UnityBCI2000.addEvent("{{ Your event name }}");`.

Events are called via the `UnityBCI2000.callEvent(string, int)` method, with the name of the event and a value to log to BCI2000.
In both of these cases `UnityBCI2000 will be replace with the name of a local reference to a UnityBCI2000 object, which should be initialized by calling `GameObject.find("{{ The name of the GameObject containing UnityBCI2000 }}").getComponent<UnityBCI2000>();` in the `Start()` method of `MonoBehaviour`.

#### Example:
```
For a scene which is structured like this:
Scene1
|-BCI   <- GameObject
| |- UnityBCI2000.cs
|
|-SomeGameObject <- GameObject
  |-SomeLogic.cs <- MonoBehaviour
 ```
`SomeLogic.cs` could look like this:

 ```
 public class SomeLogic : MonoBehaviour {
     private UnityBCI2000 bci;
     private int someValue;
     
     void Start() {
         bci = GameObject.find("BCI").getComponent<UnityBCI2000>();
         bci.addEvent("SomeEvent");
     }

     void Update() {
         {{ Object Logic }}
         if (someCondition) {
             doSomething();
         }
     }

     void doSomething() {
         {{ Object Logic }}
         bci.callEvent("SomeEvent", someValue);
     }
 }
 ```

### Using built-in variables

`BCI2000StateSender` has some variables built in. To use these, just select their check boxes in the inspector.

### Adding custom variables

To add custom variables,  add a script to the same object as a `BCI2000StateSender`, which inherits from `CustomVariableBase`.  
Then, add the `CustomVariableBase` to the "Custom Variable Supplier" field of the `BCI2000StateSender`.

Then, add `CustomVariable`s to the list `customVariables` within the `CustomVariableBase`. There are some included
templates and snippets which show the syntax for adding custom variables.

#### Custom Variables

A custom variable has four fields. These are its name, value, scale, and type.  
Its name is a string which holds its name.  
Its value is a `Func<float>` delegate which returns a float, which will be sent to BCI2000, after being multiplied by its scale.  
Its type is a member of the enum `UnityBCI2000.StateType`, which represents the format of the number being sent to BCI2000.  


#### Custom Variable Class Example

Template

    public class <ClassName> : CustomVariableBase
    {
        public override void AddCustomVariables()
        {
            customVariables.Add(new CustomSetVariable(  //Copy this for more set variables
                "[Name]",
                new Func<float>(() => [Code which returns variable to send]),
                [Scale],
                UnityBCI2000.StateType.[Type]
                ));
                
            customVariables.Add(new CustomGetVariable(  //Copy this for more get variables
                "[Name]",  
                new Action<int> ((int i) => [Code which uses i])
            ));

        }
    }

Example

    public class CustomVariableSupplier1 : CustomVariableBase
    {
        public override void AddCustomVariables() 
        {
            customVariables.Add(new CustomSetVariable( 
                "Custom variable 1",
                new Func<float>(() => {return 65 / 5;}),
                100,
                UnityBCI2000.StateType.SignedInt16
                ));
    
            customVariables.Add(new CustomSetVariable(
                "Custom variable 2: Frame count",
                new Func<float>(() => Time.frameCount),
                1,
                UnityBCI2000.StateType.UnsignedInt32
                ));

            customVariables.Add(new CustomGetVariable(  
                "StateName",  
                new Action<int> ((int i) => {score = i})
            ));
        }
    }


### Number Formats

The available formats are found in the enum `UnityBCI2000.StateType`. As they are being accessed outside of `UnityBCI2000`, 
they will always be preceded by "UnityBCI2000.".

The formats are `Boolean`, `UnsignedInt16`, `UnsignedInt32`, `SignedInt16`, and `SignedInt32`.  
These are available formats because BCI2000 takes state values in the form of unsigned integers with a bit width of powers of 2 between 1 and 32.  
Boolean is the same as an unsigned int of bit width 1.  
Signed numbers will generate a second state within BCI2000 of bit width 1, which holds their sign. If they are negative, their sign will be 1,
and if they are positive, their sign will be 0. Use signed numbers if your value will ever be negative.


### Other scripting

Try to avoid calling any of the methods directly. UnityBCI2000 should ideally be able to run and control BCI2000 without any intervention.


Properties
---

### UnityBCI2000

`Timeout`
The timeout, in milliseconds, for sending and receiving commands, default 1000.

`TelnetIP`
The IP at which to connect to the Operator, default 127.0.0.1
Note: Don't use "localhost" when setting this, as this causes errors due to ambiguity in name resolution.

`TelnetPort`
The port to connect to the Operator, default 3999.

`OperatorPath`
The path to the Operator module to start up when there is no running operator at the IP and port specified, will always connect on localhost, at the previously given port.

`LogFile`
The path to the file to write the log. This is overwritten on `Connect()`, default "logFile.txt", in the application's working directory.

`LogStates`
Whether or not to log commands to set a state, as well as the received Prompt if the command produces no errors, default false.

`LogPrompts`
Whether or not to log any Prompt, in which case only received output and errors will be logged, default false.

### BCI2000StateSender

`UnityBCI2000 object`
The object which has the attached `UnityBCI2000` script.

`Custom Variable Supplier`
The component which inherits from `CustomVariableBase` which supplies custom variables.

Global and screen coordinates and their scales.

`Camera`
The camera to use for finding screen position.

`Is on screen`
Is the object on screen.


`Velocity`
The velocity of the object in any direction.

`Custom Variables`
Lists the names of any added custom variables.

Other Notes
---
`StateVariable` is a class which holds values necessary for sending states to BCI2000.  
They are stored in two places, a central list within `UnityBCI2000` which is only used for checking if a state already exists,
and within the `BCI2000StateSender` which 'owns' the state, where they are stored within objects called `SendStateVariable`,
which update the state with a new value every frame. This is done so that one state can be changed by mutiple `BCI2000StateSender`s,
through the use of multiple `SendStateVariables` created using the `AddSendExistingState()` method. 

BCI2000Remote will write its output to a log file that is, by default, located at logFile.txt in the working directory of your Unity application.
