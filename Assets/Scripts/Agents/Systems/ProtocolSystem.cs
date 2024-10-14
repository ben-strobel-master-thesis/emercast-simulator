using Agents.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Agents.Systems
{
    public partial struct ProtocolSystem : ISystem
    {
        public const uint LayerAgentsBit = (1u << 6);
        private Random _random;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        { 
            _random = new Random(1337);
            
            foreach (var protocolComponent in SystemAPI.Query<RefRW<ProtocolComponent>>())
            {
                protocolComponent.ValueRW.lastScanTime = _random.NextDouble(-5, 10);
            }
            
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var currentTime = SystemAPI.Time.ElapsedTime;
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            
            // TODO Only agents
            var collisionFilter = new CollisionFilter()
            {
                BelongsTo = LayerAgentsBit,
                CollidesWith = LayerAgentsBit
            };
            
            var ecb = new EntityCommandBuffer(Allocator.Persistent);
            
            foreach (var (protocolComponent, transform, entity) in SystemAPI.Query<RefRW<ProtocolComponent>, RefRO<LocalTransform>>().WithNone<MaterialColor>().WithEntityAccess())
            {
                if (protocolComponent.ValueRO.hasMessage) continue;

                if (protocolComponent.ValueRO.lastScanTime == 0)
                {
                    protocolComponent.ValueRW.lastScanTime = _random.NextDouble(0, 5);
                }
                
                if (currentTime - protocolComponent.ValueRO.lastScanTime < 5) continue;
                var outHits = new NativeList<DistanceHit>(Allocator.Temp);
                collisionWorld.OverlapSphere(transform.ValueRO.Position, 10, ref outHits, collisionFilter);
                protocolComponent.ValueRW.lastScanTime = currentTime;
                if(outHits.Length == 0) continue;
                var protocolComponentLookup = state.GetComponentLookup<ProtocolComponent>();
                var childrenBufferLookup = state.GetBufferLookup<Child>();
                foreach (var distanceHit in outHits)
                {
                    if (!protocolComponentLookup.HasComponent(distanceHit.Entity)) return;
                    var otherProtocolComponent = protocolComponentLookup[distanceHit.Entity];
                    if (otherProtocolComponent.hasMessage)
                    {
                        protocolComponent.ValueRW.hasMessage = true;
#if UNITY_EDITOR
                        if(!childrenBufferLookup.HasBuffer(entity)) continue;
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
            }
            
            ecb.Playback(state.EntityManager);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}