using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Agents.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(AgentSeekingSystemGroup))]
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
                if (agentBody.ValueRO.IsStopped)
                {
                    var x = _random.NextFloat(-25, 1000);
                    var y = transform.ValueRO.Position.y;
                    var z = _random.NextFloat(-1500, -500);
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