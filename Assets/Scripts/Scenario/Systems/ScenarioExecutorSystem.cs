using Agents.Components;
using Scenario.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Application = UnityEngine.Application;

namespace Scenario.Systems
{
    public partial struct ScenarioExecutorSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ScenarioFileProcessedTagComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var scenarioFileProcessedTagComponent = SystemAPI.GetSingleton<ScenarioFileProcessedTagComponent>();
            var scenarioFileProcessedTagEntity = SystemAPI.GetSingletonEntity<ScenarioFileProcessedTagComponent>();
            var simulatedTime = state.WorldUnmanaged.Time.ElapsedTime;

            if (scenarioFileProcessedTagComponent.BroadcastTime != 0f && simulatedTime > scenarioFileProcessedTagComponent.BroadcastTime)
            {
                scenarioFileProcessedTagComponent.BroadcastTime = 0f;
                
                var ecb = new EntityCommandBuffer(Allocator.Persistent);
                ecb.SetComponent(scenarioFileProcessedTagEntity, scenarioFileProcessedTagComponent);
                foreach (var (protocolComponent, transform, entity) in SystemAPI.Query<RefRW<ProtocolComponent>, RefRO<LocalTransform>>().WithNone<MaterialColor>().WithEntityAccess())
                {
                    if (math.distance(new float3(scenarioFileProcessedTagComponent.BroadcastPositionX, 0, scenarioFileProcessedTagComponent.BroadcastPositionZ), transform.ValueRO.Position) > scenarioFileProcessedTagComponent.BroadcastRadius) continue;
                    protocolComponent.ValueRW.HasMessage = true;
                    foreach (var child in state.EntityManager.GetBuffer<Child>(entity))
                    {
#if UNITY_EDITOR
                        ecb.AddComponent(child.Value, new URPMaterialPropertyBaseColor()
                        {
                            Value = new float4(0,255,0,255)
                        });  
#endif
                    }
                }
                
                Debug.Log("Applied event to agents");
                
                ecb.Playback(state.WorldUnmanaged.EntityManager);
            }
            
            if (simulatedTime > scenarioFileProcessedTagComponent.EndSimulationTime)
            {
                Debug.Log("EndSimulationTime exceeded. Stopping simulation");
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}