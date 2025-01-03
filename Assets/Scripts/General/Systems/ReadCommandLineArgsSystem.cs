using System.Globalization;
using System.Threading;
using Agents.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Random = System.Random;

namespace General.Systems
{
    public partial class ReadCommandLineArgsSystem : SystemBase
    {
        private bool _executed = false;
        
        protected override void OnUpdate()
        {
            if (_executed) return;
            var random = new Random();
            
            // Circumvent C# culture specific parsing
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            
            var parametersComponent = new ParametersComponent()
            {
                Seed = (uint)random.Next(0, int.MaxValue),
#if UNITY_EDITOR
                ScenarioFilePath = @".\Assets\Resources\default.emerscenario",
#else
                ScenarioFilePath = "",
#endif
                
                ConnectivityRange = 10,
                
                ProtocolEnabled = true,
                
                Phase0Duration = 3, // Standby
                Phase1Duration = 1, // Establishing Connection
                Phase2Duration = 0.25, // SystemMessage Exchange
                Phase3Duration = 0.25, // Message Exchange
                Phase4Duration = 0.5, // Teardown
            };
            
            var args = System.Environment.GetCommandLineArgs();
            
            for (var i = 0; i <= args.Length - 1; i++)
            {
                var arg = args[i];

                if (arg.ToLower() == "-help" || arg.ToLower() == "help")
                {
                    PrintHelp();
                    return;
                }
                
                if(i == args.Length - 1) continue;
                var nextArg = args[i + 1];
                
                if(nextArg == null || !arg.StartsWith("-")) continue;
                arg = arg[1..];

                switch (arg.ToLower())
                {
                    case "seed":
                        parametersComponent.Seed = uint.Parse(nextArg);
                        break;
                    case "protocol-enabled":
                        parametersComponent.ProtocolEnabled = bool.Parse(nextArg);
                        break;
                    case "scenariofile":
                        parametersComponent.ScenarioFilePath = nextArg;
                        break;
                    case "connectivityrange":
                        parametersComponent.ConnectivityRange = double.Parse(nextArg);
                        break;
                    case "phase0duration":
                    case "phase1duration":
                    case "phase2duration":
                    case "phase3duration":
                    case "phase4duration":
                        var durationIndex = uint.Parse(arg.Replace("phase", "").Replace("duration", ""));
                        var durationValue = double.Parse(nextArg);
                        switch (durationIndex)
                        {
                            case 0:
                                parametersComponent.Phase0Duration = durationValue;
                                break;
                            case 1:
                                parametersComponent.Phase1Duration = durationValue;
                                break;
                            case 2:
                                parametersComponent.Phase2Duration = durationValue;
                                break;
                            case 3:
                                parametersComponent.Phase3Duration = durationValue;
                                break;
                            case 4:
                                parametersComponent.Phase4Duration = durationValue;
                                break;
                        }
                        break;
                }
            }
           
            Debug.Log("Settings after reading command line args: " + ToString(parametersComponent) + " ScenarioFilePath: \"" + parametersComponent.ScenarioFilePath + "\"");
            
            var ecb = new EntityCommandBuffer(Allocator.Persistent);
            
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, parametersComponent);
            
            ecb.Playback(EntityManager);
            
            _executed = true;
        }

        private void PrintHelp()
        {
            Debug.Log("Emercast Simulator by Ben Strobel");
            Debug.Log("");
            Debug.Log("Simulator specfic arguments:\n\n-Scenariofile <x> Sets the path of the scenariofile that should be read to the string x\n-Seed <x> Sets the seed to the uint x (Optional, default random)\n-Protocol-Enabled <x> Overwrites whether the peer to peer protocol between nodes is enabled (Optional, default true)\n-ConnectivityRange <x> Sets the max range for two agents to be able to connect/maintain a connection (Optional, default 10)\n-Phase<X>Duration <s> Sets the duration of protocol phase x (0-4) to s seconds (Optional, default 5,2,1,3,1)\n-Phase<X>Delay <s> Sets the delay after a protocol phase x (0-4) to s seconds (Optional, default 0,0,0,0,0)\n");
            Debug.Log("The scenariofile supports the following commands:\nSpawn <Id> <x> <y>\nAddDestination <Id> <x> <y>\nOutage <x> <y> <r>\nBroadcast <s>\nEndSimulation <s>");
            Debug.Log("For more unity specific command line arguments, see: https://docs.unity3d.com/Manual/PlayerCommandLineArguments.html");
            
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private string ToString(ParametersComponent parameters)
        {
            return JsonUtility.ToJson(parameters);
        }
    }
}