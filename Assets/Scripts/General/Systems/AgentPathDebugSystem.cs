using Agents.Components;
using ProjectDawn.Navigation;
using Scenario.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace General.Systems
{
    public partial struct AgentPathDebugSystem : ISystem
    {
        private BufferLookup<ScenarioCommandComponent> _bufferLookup;
        private bool hasExecuted;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _bufferLookup = state.GetBufferLookup<ScenarioCommandComponent>(true);
            
            // Change to false for debugging (only visible in unity editor, scene view)
            hasExecuted = true;
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (hasExecuted) return;
            _bufferLookup.Update(ref state);
            foreach (var (agentBody, transform, commandPointer, entity) in SystemAPI
                         .Query<RefRW<AgentBody>, RefRO<LocalTransform>, RefRW<AgentScenarioCommandPointerComponent>>()
                         .WithEntityAccess())
            {
                if(!_bufferLookup.HasBuffer(entity)) continue;
                var scenarioCommandBuffer = _bufferLookup[entity];
                for (var i = 0; i < scenarioCommandBuffer.Length - 1; i++)
                {
                    var cmd = scenarioCommandBuffer[i];
                    var nextCmd = scenarioCommandBuffer[i + 1];
                    Debug.DrawLine(
                        new Vector3(cmd.XValue.Value,0.1f,cmd.YValue.Value), 
                        new Vector3(nextCmd.XValue.Value,0.1f,nextCmd.YValue.Value), 
                    Color.green,
                        120000);
                    hasExecuted = true;
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}