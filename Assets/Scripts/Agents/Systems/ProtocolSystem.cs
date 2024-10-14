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
        private ComponentLookup<ProtocolComponent> _protocolComponentLookup;
        private BufferLookup<Child> _childrenBufferLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        { 
            _random = new Random(1337);
            _protocolComponentLookup = state.GetComponentLookup<ProtocolComponent>();
            _childrenBufferLookup = state.GetBufferLookup<Child>();
            
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
            
            _protocolComponentLookup.Update(ref state);
            _childrenBufferLookup.Update(ref state);
            
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
                foreach (var distanceHit in outHits)
                {
                    if (!_protocolComponentLookup.HasComponent(distanceHit.Entity)) return;
                    var otherProtocolComponent = _protocolComponentLookup[distanceHit.Entity];
                    if (otherProtocolComponent.hasMessage)
                    {
                        protocolComponent.ValueRW.hasMessage = true;
#if UNITY_EDITOR
                        if(!_childrenBufferLookup.HasBuffer(entity)) continue;
                        foreach (var child in _childrenBufferLookup[entity])
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