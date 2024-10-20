using Agents.Components;
using ProjectDawn.Navigation;
using Scenario.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Agents.Systems
{
    [BurstCompile]
    public partial struct AgentCommandSystem : ISystem
    {
        private BufferLookup<ScenarioCommandComponent> _bufferLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _bufferLookup = state.GetBufferLookup<ScenarioCommandComponent>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _bufferLookup.Update(ref state);
            foreach (var (agentBody, transform, commandPointer, entity) in SystemAPI.Query<RefRW<AgentBody>, RefRO<LocalTransform>, RefRW<AgentScenarioCommandPointerComponent>>().WithEntityAccess())
            {
                if (agentBody.ValueRO is { IsStopped: false }) continue;
                if (commandPointer.ValueRO.NextCommandIndex == -1) continue;
                if(!_bufferLookup.HasBuffer(entity)) continue;
                var scenarioCommandBuffer = _bufferLookup[entity];
                if (commandPointer.ValueRO.NextCommandIndex >= scenarioCommandBuffer.Length)
                {
                    commandPointer.ValueRW.NextCommandIndex = -1;
                    continue;
                }
                var currentCommand = scenarioCommandBuffer[commandPointer.ValueRO.NextCommandIndex];
                if(currentCommand.Kind != ScenarioCommandKindEnum.AddDestination || !currentCommand.XValue.HasValue || !currentCommand.YValue.HasValue) continue;
                    
                var x = currentCommand.XValue.Value;
                var y = transform.ValueRO.Position.y;
                var z = currentCommand.YValue.Value;
                    
                agentBody.ValueRW.SetDestination(new float3(x, y, z));
                commandPointer.ValueRW.NextCommandIndex = commandPointer.ValueRO.NextCommandIndex + 1;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}