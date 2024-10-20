# Emercast Simulator

## Download

Download the latest tarball from [https://benstrobel-de.storage.bunnycdn.com/master-thesis/emercast-simulator/latest.tar](https://benstrobel-de.storage.bunnycdn.com/master-thesis/emercast-simulator/latest.tar)

## How to run

1. Extract the archive into a folder
2.  Run the default scenario
- While logging to the console: ```./EmercastSimulator.x86_64 -batchmode -nographics -timestamps -logFile - -ScenarioFile ./default.emerscenario```
- While logging to a file ```./EmercastSimulator.x86_64 -batchmode -nographics -timestamps -logFile ./test.log -ScenarioFile ./default.emerscenario```
- With graphics, while logging to the console: ```./EmercastSimulator.x86_64 -timestamps -logFile - -ScenarioFile ./default.emerscenario```

### Command line arguments:

See native unity command line arguments here: [https://docs.unity3d.com/Manual/PlayerCommandLineArguments.html](https://docs.unity3d.com/Manual/PlayerCommandLineArguments.html)

Simulator specfic arguments:
- -Scenariofile \<x\> Sets the path of the scenariofile that should be read to the string x
- -Seed \<x\> Sets the seed to the uint x *(Optional, default random)*
- -ConnectivityRange \<x\> Sets the max range for two agents to be able to connect/maintain a connection *(Optional, default 10)*
- -Phase\<X\>Duration \<s\> Sets the duration of protocol phase x (0-4) to s seconds *(Optional, default 5,2,1,3,1)*
- -Phase\<X\>Delay \<s\> Sets the delay after a protocol phase x (0-4) to s seconds *(Optional, default 0,0,0,0,0)*

### The scenariofile supports the following commands:
- Spawn \<Id\> \<x\> \<y\>
- AddDestination \<Id\> \<x\> \<y\>
- Broadcast \<x\> \<y\> \<r\> \<s\>
- EndSimulation \<s\>
