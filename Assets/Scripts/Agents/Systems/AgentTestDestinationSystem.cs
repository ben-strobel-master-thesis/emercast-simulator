using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Agents.Systems
{
    [BurstCompile]
    public partial struct AgentTestDestinationSystem : ISystem
    {
        private Random _random;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _random = new Random(1337);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (agentBody, transform) in SystemAPI.Query<RefRW<AgentBody>, RefRO<LocalTransform>>())
            {
                if (agentBody.ValueRO.IsStopped || agentBody.ValueRO.Destination is { x: < 0.1f and > -0.1f, y: < 0.1f and > -0.1f })
                {
                    var x = _random.NextFloat(-1000, 1000);
                    var y = transform.ValueRO.Position.y;
                    var z = _random.NextFloat(-1000, 1000);
                    agentBody.ValueRW.SetDestination(new float3(x, y, z));
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}