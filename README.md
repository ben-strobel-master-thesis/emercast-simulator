# Emercast Simulator

This repository contains the Emercast simulator, which simulates the behavior of Emercast system in an emergency scenario in the city center of munich.
It does so by simulating thousands of "agents" each of which carry a simulated instance of the Emercast Android Application.
The agents move through the simulated city according to a movement plan specified in a "scenariofile".
The simulator then simulates an abstracted interaction between the instances and creates metrics and logs in the process.

This repository also contains [code](https://github.com/ben-strobel-master-thesis/emercast-simulator/tree/main/Assets/Editor) to import data WKT data [provided by the state of bavario](https://geodaten.bayern.de/opengeodata/OpenDataDetail.html?pn=lod2) and parse to create unity objects that are used for the simulation.

## Run

- Download the latest tarball from https://benstrobel.b-cdn.net/master-thesis/emercast-simulator/latest.tar
- Extract the archive into a folder
- Run the default scenario
- While logging to the console: ```./EmercastSimulator.x86_64 -batchmode -nographics -timestamps -logFile - -ScenarioFile ./default.emerscenario```
- While logging to a file ```./EmercastSimulator.x86_64 -batchmode -nographics -timestamps -logFile ./test.log -ScenarioFile ./default.emerscenario```
- With graphics, while logging to the console: ```./EmercastSimulator.x86_64 -timestamps -logFile - -ScenarioFile ./default.emerscenario```

I suggest having a look at the [Script Utils Repository](https://github.com/ben-strobel-master-thesis/script-utils) which uses this repository to run a batch simulation.

### Arguments

See native unity command line arguments here: [https://docs.unity3d.com/Manual/PlayerCommandLineArguments.html](https://docs.unity3d.com/Manual/PlayerCommandLineArguments.html)

Simulator specfic arguments:
- -Scenariofile \<x\> Sets the path of the scenariofile that should be read to the string x
- -Seed \<x\> Sets the seed to the uint x *(Optional, default random)*
- -ConnectivityRange \<x\> Sets the max range for two agents to be able to connect/maintain a connection *(Optional, default 10)*
- -Phase\<X\>Duration \<s\> Sets the duration of protocol phase x (0-4) to s seconds *(Optional, default 3,1,0.25,0.25,0.5)*

### Scenariofile commands:
- Spawn \<Id\> \<x\> \<y\>
- AddDestination \<Id\> \<x\> \<y\>
- Broadcast \<seconds\>
- Outage \<x\> \<y\> \<radius\>
- EndSimulation \<seconds\>
