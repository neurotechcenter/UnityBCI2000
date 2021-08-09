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
`TelnetIp:TelnetPort`. This is useful for if you already have an operator open that you want to use. If you also
have modules started within a running BCI2000 instance, set `DontStartModules` to `true`, this will preserve
your running modules.


### Using built-in variables

`BCI2000StateSender` has some variables built in. To use these, just select their check boxes in the inspector.

### Adding custom variables

To add custom variables,  add a script to the same object as a `BCI2000StateSender`, which inherits from `CustomVariableBase`.
Then, add the `CustomVariableBase` to the "Custom Variable Supplier" field of the `BCI2000StateSender`

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
        public override void AddCustomVariables() //Copy this for more variables
        {
            customVariables.Add(new CustomVariable(
                "<Name>",
                new Func<float>(() => <Value>),
                <Scale>,
                UnityBCI2000.StateType.<Type>
                ));
        }
    }

Example

    public class CustomVariableSupplier1 : CustomVariableBase
    {
        public override void AddCustomVariables() //Copy this for more variables
        {
            customVariables.Add(new CustomVariable(
                "Custom variable 1",
                new Func<float>(() => {return 65 / 5;}),
                100,
                UnityBCI2000.StateType.SignedInt16
                ));
    
            customVariables.Add(new CustomVariable(
                "Custom variable 2: Frame count",
                new Func<float>(() => Time.frameCount),
                1,
                UnityBCI2000.StateType.UnsignedInt32
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


BCI2000Remote will write its output to a log file that is, by default, located at logFile.txt in the working directory of your Unity application.



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