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
UnityBCI2000 provides an interface througn which one can control and interact with BCI2000
from a Unity application.

Usage
---

Add the `UnityBCI2000.cs` script to one `GameObject`. This is the central script which communicates with BCI2000.  
Before starting, you must set the operator and modules to start up, by specifying the fields
`OperatorPath` and `Module[1-3]`. All other properties have default settings and will function without change.  
If you also have modules started within a running BCI2000 instance, set `DontStartModules` to `true`, this will preserve
your running modules.

Within the `Start()` methods of your scripts, add states and events to UnityBCI2000. 
You can now access BCI2000's states and events using the included methods of the UnityBCI2000 object.
