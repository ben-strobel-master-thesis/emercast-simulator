using Agents.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;

namespace Agents.Systems
{
    public partial struct ProtocolSystem : ISystem
    {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var random = new Random(1337);
            
            foreach (var protocolComponent in SystemAPI.Query<RefRW<ProtocolComponent>>())
            {
                protocolComponent.ValueRW.lastScanTime = random.NextDouble(-5, 10);
            }
            
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            return;
            var currentTime = SystemAPI.Time.ElapsedTime;
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            
            // TODO Only agents
            var collisionFilter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
            };
            
            var protocolComponentLookup = state.GetComponentLookup<ProtocolComponent>();
            var childrenBufferLookup = state.GetBufferLookup<Child>();
            
            var ecb = new EntityCommandBuffer(Allocator.Persistent);
            
            foreach (var (protocolComponent, transform, entity) in SystemAPI.Query<RefRW<ProtocolComponent>, RefRO<LocalTransform>>().WithNone<MaterialColor>().WithEntityAccess())
            {
                if (protocolComponent.ValueRO.hasMessage) return;
                if (currentTime - protocolComponent.ValueRO.lastScanTime < 5) return;
                var outHits = new NativeList<DistanceHit>();
                collisionWorld.OverlapSphere(transform.ValueRO.Position, 10, ref outHits, collisionFilter);
                foreach (var distanceHit in outHits)
                {
                    var otherProtocolComponent = protocolComponentLookup[distanceHit.Entity];
                    if (otherProtocolComponent.hasMessage)
                    {
                        protocolComponent.ValueRW.hasMessage = true;
#if UNITY_EDITOR
                        foreach (var child in childrenBufferLookup[entity])
                        {
                            ecb.AddComponent(child.Value, new URPMaterialPropertyBaseColor()
                            {
                                Value = new float4(0,255,0,255)
                            });   
                        }                  
#endif
                        break;
                    }
                }
                
                protocolComponent.ValueRW.lastScanTime = currentTime;
            }
            
            ecb.Playback(state.EntityManager);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}