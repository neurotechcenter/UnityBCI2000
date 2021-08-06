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
Before starting, you must set the operator and modules to start up, by specifying the fields
`OperatorPath` and `Module[1-3]`. All other properties have default settings and will function without change.
Alternatively, if you leave `OperatorPath` blank, `BCI2000Remote` will attempt to connect to an operator at
`TelnetIp:TelnetPort`. This is useful for if you already have an operator open that you want to use. If you also
have modules started within a running BCI2000 instance, set `DontStartModules` to `true`, this will preserve
your running modules.
BCI2000Remote will write its output to a log file that is, by default, logFile.txt in the working directory of your Unity application.

Properties
---
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



Methods
---
UnityBCI2000 is designed for use without any scripting, but there are functions which can be used to control specific aspects of the program.

<<<<<<< Updated upstream
=======
UnityBCI2000
`AddStateVariable(string name, StateType type)`
Adds a state variable to `UnityBCI2000`. 


BCI2000StateSender
`AddCustomVariable()`
Adds a custom variable to the Sender. This is the only method that you need to call in order to add custom variables.

>>>>>>> Stashed changes

`AddStateVariable(string name, StateType type)`
Adds a state variable to `UnityBCI2000`. 