using Agents.Components;
using Scenario.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Agents.Systems
{
    public partial struct ProtocolSystem : ISystem
    {
        public const uint LayerAgentsBit = (1u << 6);
        private ComponentLookup<ProtocolComponent> _protocolComponentLookup;
        private ComponentLookup<LocalTransform> _transformComponentLookup;
        private BufferLookup<Child> _childrenBufferLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        { 
            _protocolComponentLookup = state.GetComponentLookup<ProtocolComponent>();
            _transformComponentLookup = state.GetComponentLookup<LocalTransform>(true);
            _childrenBufferLookup = state.GetBufferLookup<Child>();
            
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<ParametersComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var currentTime = SystemAPI.Time.ElapsedTime;
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var parameters = SystemAPI.GetSingleton<ParametersComponent>();
            
            var collisionFilter = new CollisionFilter()
            {
                BelongsTo = LayerAgentsBit,
                CollidesWith = LayerAgentsBit
            };
            
            _protocolComponentLookup.Update(ref state);
            _transformComponentLookup.Update(ref state);
            _childrenBufferLookup.Update(ref state);
            
            var ecb = new EntityCommandBuffer(Allocator.Persistent);
            
            foreach (var (protocolComponent, transform, entity) in SystemAPI.Query<RefRW<ProtocolComponent>, RefRO<LocalTransform>>().WithNone<MaterialColor>().WithEntityAccess())
            {
                if (protocolComponent.ValueRO.Phase != 0 && currentTime - protocolComponent.ValueRO.PhaseChangedTime > 10)
                {
                    protocolComponent.ValueRW.Phase = 0;
                    protocolComponent.ValueRW.PhaseChangedTime = currentTime;
                    protocolComponent.ValueRW.OtherEntity = Entity.Null;
                    EmitProtocolTimedOutEvent(protocolComponent.ValueRO.Id, currentTime);
                    continue;
                }
                
                if (protocolComponent.ValueRO.Phase == 0)
                {
                    if(currentTime - protocolComponent.ValueRO.PhaseChangedTime < parameters.Phase0Duration) continue;
                    var outHits = new NativeList<DistanceHit>(Allocator.Temp);
                    collisionWorld.OverlapSphere(transform.ValueRO.Position, 10, ref outHits, collisionFilter);
                    protocolComponent.ValueRW.PhaseChangedTime = currentTime;
                    if(outHits.Length == 0) continue;
                    foreach (var distanceHit in outHits)
                    {
                        if (!_protocolComponentLookup.HasComponent(distanceHit.Entity)) return;
                        var otherProtocolComponent = _protocolComponentLookup[distanceHit.Entity];
                        if (otherProtocolComponent.HasMessage != protocolComponent.ValueRO.HasMessage)
                        {
                            protocolComponent.ValueRW.Phase = 1;
                            protocolComponent.ValueRW.OtherEntity = distanceHit.Entity;
                            break;
                        }
                    }
                } 
                else if (protocolComponent.ValueRO.Phase == 1)
                {
                    if(currentTime - protocolComponent.ValueRO.PhaseChangedTime < parameters.Phase1Duration) continue;
                    var otherProtocolComponent = _protocolComponentLookup[protocolComponent.ValueRO.OtherEntity];
                    if(otherProtocolComponent.OtherEntity != entity) continue;
                    var otherTransform = _transformComponentLookup[otherProtocolComponent.OtherEntity];
                    if (math.distance(transform.ValueRO.Position, otherTransform.Position) < parameters.ConnectivityRange)
                    {
                        if(otherProtocolComponent.Phase < protocolComponent.ValueRO.Phase) continue;
                        protocolComponent.ValueRW.Phase = 2;
                        protocolComponent.ValueRW.PhaseChangedTime = currentTime;
                        if (otherProtocolComponent.Phase == 2)
                        {
                            EmitConnectionEstablishedEvent(protocolComponent.ValueRO.Id, otherProtocolComponent.Id, currentTime);
                        }
                    }
                    else
                    {
                        EmitConnectionOutOfRangeEvent(protocolComponent.ValueRO.Id, otherProtocolComponent.Id, currentTime);
                        protocolComponent.ValueRW.Phase = 0;
                        protocolComponent.ValueRW.PhaseChangedTime = currentTime;
                        protocolComponent.ValueRW.OtherEntity = Entity.Null;
                    }
                } 
                else if (protocolComponent.ValueRO.Phase == 2)
                {
                    if(currentTime - protocolComponent.ValueRO.PhaseChangedTime < parameters.Phase2Duration) continue;
                    var otherProtocolComponent = _protocolComponentLookup[protocolComponent.ValueRO.OtherEntity];
                    if(otherProtocolComponent.OtherEntity != entity) continue;
                    var otherTransform = _transformComponentLookup[otherProtocolComponent.OtherEntity];
                    if (math.distance(transform.ValueRO.Position, otherTransform.Position) < parameters.ConnectivityRange)
                    {
                        if(otherProtocolComponent.Phase < protocolComponent.ValueRO.Phase) continue;
                        protocolComponent.ValueRW.Phase = 3;
                        protocolComponent.ValueRW.PhaseChangedTime = currentTime;
                    }
                    else
                    {
                        EmitConnectionOutOfRangeEvent(protocolComponent.ValueRO.Id, otherProtocolComponent.Id, currentTime);
                        protocolComponent.ValueRW.Phase = 0;
                        protocolComponent.ValueRW.PhaseChangedTime = currentTime;
                        protocolComponent.ValueRW.OtherEntity = Entity.Null;
                    }
                } 
                else if (protocolComponent.ValueRO.Phase == 3)
                {
                    if(currentTime - protocolComponent.ValueRO.PhaseChangedTime < parameters.Phase3Duration) continue;
                    var otherProtocolComponent = _protocolComponentLookup[protocolComponent.ValueRO.OtherEntity];
                    if(otherProtocolComponent.OtherEntity != entity) continue;
                    var otherTransform = _transformComponentLookup[otherProtocolComponent.OtherEntity];
                    if (math.distance(transform.ValueRO.Position, otherTransform.Position) < parameters.ConnectivityRange)
                    {
                        if(otherProtocolComponent.Phase < protocolComponent.ValueRO.Phase) continue;
                        protocolComponent.ValueRW.Phase = 4;
                        protocolComponent.ValueRW.PhaseChangedTime = currentTime;
                         if (protocolComponent.ValueRO.Id > otherProtocolComponent.Id) // Simulating one of the two peers, being the server
                         {
                            if (protocolComponent.ValueRO.HasMessage && !otherProtocolComponent.HasMessage)
                            {
                                otherProtocolComponent.HasMessage = true;
                                otherProtocolComponent.Hops = protocolComponent.ValueRO.Hops + 1;
                                EmitMessageTransmittedEvent(protocolComponent.ValueRO.Id, otherProtocolComponent.Id, protocolComponent.ValueRO.Hops + 1, currentTime);
#if UNITY_EDITOR
                                if(!_childrenBufferLookup.HasBuffer(otherProtocolComponent.OtherEntity)) continue;
                                foreach (var child in _childrenBufferLookup[otherProtocolComponent.OtherEntity])
                                {
                                    ecb.AddComponent(child.Value, new URPMaterialPropertyBaseColor()
                                    {
                                        Value = new float4(0,255,0,255)
                                    });
                                }
#endif
                            } 
                            else if (otherProtocolComponent.HasMessage && !protocolComponent.ValueRO.HasMessage)
                            {
                                protocolComponent.ValueRW.HasMessage = true;
                                protocolComponent.ValueRW.Hops = otherProtocolComponent.Hops + 1;
                                EmitMessageTransmittedEvent(otherProtocolComponent.Id, protocolComponent.ValueRO.Id, otherProtocolComponent.Hops + 1, currentTime);
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
                            }
                         }
                    }
                    else
                    {
                        EmitConnectionOutOfRangeEvent(protocolComponent.ValueRO.Id, otherProtocolComponent.Id, currentTime);
                        protocolComponent.ValueRW.Phase = 0;
                        protocolComponent.ValueRW.PhaseChangedTime = currentTime;
                        protocolComponent.ValueRW.OtherEntity = Entity.Null;
                    }
                } 
                else if (protocolComponent.ValueRO.Phase == 4)
                {
                    if(currentTime - protocolComponent.ValueRO.PhaseChangedTime < parameters.Phase4Duration) continue;
                    protocolComponent.ValueRW.Phase = 0;
                    protocolComponent.ValueRW.PhaseChangedTime = currentTime;
                }
            }
            
            ecb.Playback(state.EntityManager);
        }

        private void EmitConnectionEstablishedEvent(uint id1, uint id2, double currentTime)
        {
            Debug.Log($"EVENTS|CONNECTION_ESTABLISHED|{id1}|{id2}|{currentTime}");
        }
        
        private void EmitConnectionOutOfRangeEvent(uint id1, uint id2, double currentTime)
        {
            Debug.Log($"EVENTS|CONNECTION_OUT_OF_RANGE|{id1}|{id2}|{currentTime}");
        }
        
        private void EmitProtocolTimedOutEvent(uint id1, double currentTime)
        {
            Debug.Log($"EVENTS|PROTOCOL_TIMED_OUT|{id1}|{currentTime}");
        }

        private void EmitMessageTransmittedEvent(uint id1, uint id2, uint hop, double currentTime)
        {
            Debug.Log($"EVENTS|MESSAGE_TRANSMITTED|{id1}|{id2}|{hop}|{currentTime}");
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}