using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Agents.Components;
using Scenario.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Scenario.Systems
{
    public partial class ReadScenarioFileSystem : SystemBase
    {
        private bool _executed = false;
        
        protected override void OnUpdate()
        {
            if (_executed) return;
            
            // Circumvent C# culture specific parsing
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            
            var parametersFound = SystemAPI.TryGetSingleton<ParametersComponent>(out var parameters);
            if (!parametersFound) return;

            var random = new Random(parameters.Seed);
            var spawnCommandList = new List<ScenarioCommandComponent>();
            var agentScenarioCommandDictionary = new Dictionary<uint, List<ScenarioCommandComponent>>();
            var scenarioFileProcessedTagComponent = new ScenarioFileProcessedTagComponent();
            var scenarioHelperFound = SystemAPI.TryGetSingleton<ScenarioHelperComponent>(out var scenarioHelperComponent);
            if (!scenarioHelperFound) return;
            
            EntityCommandBuffer ecb = new(Allocator.Persistent);

            var readScenarioFileEntity = ecb.CreateEntity();
            
            if (parameters.ScenarioFilePath != "")
            {
                Debug.Log("Reading scenario file " + parameters.ScenarioFilePath);
                var lines = File.ReadAllLines(parameters.ScenarioFilePath.Value);
                Debug.Log("Read " + lines.Length + " lines");
                for (var i = lines.Length - 1; i >= 0; i--)
                {
                    var line = lines[i];
                    var segments = line.Split(' ');
                    if(segments.Length < 1) continue;
                    
                    var foundMatch = Enum.TryParse(segments[0], out ScenarioCommandKindEnum kind);
                    if (!foundMatch) continue;

                    var scenarioCommand = new ScenarioCommandComponent()
                    {
                        Kind = kind,
                    };

                    switch (kind)
                    {
                        case ScenarioCommandKindEnum.Spawn:
                        {
                            if(segments.Length != 4) continue;
                            scenarioCommand.IdValue = uint.Parse(segments[1]);
                            if (scenarioCommand.IdValue == 0) throw new ArgumentException("Agent Id 0 isn't allowed");
                            scenarioCommand.XValue = float.Parse(segments[2]);
                            scenarioCommand.YValue = float.Parse(segments[3]);
                            spawnCommandList.Add(scenarioCommand);
                            continue;
                        }
                        case ScenarioCommandKindEnum.Broadcast:
                        {
                            if(segments.Length != 2) continue;
                            scenarioFileProcessedTagComponent.BroadcastTime = double.Parse(segments[1]);
                            continue;
                        }
                        case ScenarioCommandKindEnum.Outage:
                        {
                            if(segments.Length != 4) continue;
                            scenarioFileProcessedTagComponent.OutagePositionX = float.Parse(segments[1]);
                            scenarioFileProcessedTagComponent.OutagePositionZ = float.Parse(segments[2]);
                            scenarioFileProcessedTagComponent.OutageRadius = float.Parse(segments[3]);
                            continue;
                        }
                        case ScenarioCommandKindEnum.EndSimulation:
                        {
                            if(segments.Length != 2) continue;
                            scenarioFileProcessedTagComponent.EndSimulationTime = double.Parse(segments[1]);
                            continue;
                        }
                        case ScenarioCommandKindEnum.AddDestination:
                        {
                            if(segments.Length != 4) continue;
                            scenarioCommand.IdValue = uint.Parse(segments[1]);
                            scenarioCommand.XValue = float.Parse(segments[2]);
                            scenarioCommand.YValue = float.Parse(segments[3]);
                            if (segments.Length == 5)
                            {
                                scenarioCommand.SecondsValue = uint.Parse(segments[4]);
                            }

                            break;
                        }
                    }

                    if (agentScenarioCommandDictionary.ContainsKey(scenarioCommand.IdValue.Value))
                    {
                        agentScenarioCommandDictionary[scenarioCommand.IdValue.Value].Add(scenarioCommand);
                    }
                    else
                    {
                        agentScenarioCommandDictionary[scenarioCommand.IdValue.Value] =
                            new List<ScenarioCommandComponent>()
                            {
                                scenarioCommand
                            };
                    }
                }
            }
            
            Debug.Log("Scenario settings: " + JsonUtility.ToJson(scenarioFileProcessedTagComponent));
            Debug.Log("Executing " + spawnCommandList.Count + " spawn commands");
            
            foreach (var spawnCommand in spawnCommandList)
            {
                var entity = ecb.Instantiate(scenarioHelperComponent.AgentRepresentationPrefabEntity);
                ecb.SetComponent(entity, new ProtocolComponent()
                {
                    Id = spawnCommand.IdValue.Value,
                    HasMessage = false,
                    PhaseChangedTime = random.NextDouble(-5, 10),
                    Hops = 0,
                    Phase = 0
                });
                ecb.SetComponent(entity, new LocalTransform()
                {
                    Position = new float3(spawnCommand.XValue.Value,0.01f,spawnCommand.YValue.Value),
                    Scale = 1f,
                    Rotation = quaternion.identity
                });
                var agentCommandBuffer = ecb.AddBuffer<ScenarioCommandComponent>(entity);
                if (!agentScenarioCommandDictionary.ContainsKey(spawnCommand.IdValue.Value)) continue;
                var agentCommands = agentScenarioCommandDictionary[spawnCommand.IdValue.Value];
                Debug.Log("Agent" + spawnCommand.IdValue.Value + " has " + agentCommands.Count + " commands queued.");
                agentCommands.Reverse(); // Adding to buffer array is first in last out
                foreach (var agentCommand in agentCommands)
                {
                    agentCommandBuffer.Add(agentCommand);
                }
                agentCommandBuffer.Length = agentCommands.Count;
                agentCommands.Capacity = agentCommands.Count;
                ecb.AddComponent(entity, new AgentScenarioCommandPointerComponent()
                {
                    NextCommandIndex = agentCommandBuffer.Length > 0 ? 0 : -1
                });
            }
            
            ecb.AddComponent(readScenarioFileEntity, scenarioFileProcessedTagComponent);
            
            ecb.Playback(World.EntityManager);
            
            _executed = true;
        }
    }
}