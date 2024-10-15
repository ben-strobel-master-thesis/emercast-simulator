using Agents.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Agents.Systems
{
    public partial class EmergencyEventSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (Input.GetKeyUp(KeyCode.K))
            {
                var pos = new float3(0, 0, 0);
                
                var ecb = new EntityCommandBuffer(Allocator.Persistent);
                
                Debug.Log("K Pressed. Executing Event...");
                var appliedTo = 0;
                foreach (var (protocolComponent, transform, entity) in SystemAPI.Query<RefRW<ProtocolComponent>, RefRO<LocalTransform>>().WithNone<MaterialColor>().WithEntityAccess())
                {
                    if (math.distance(pos, transform.ValueRO.Position) > 300) continue;
                    appliedTo++;
                    protocolComponent.ValueRW.HasMessage = true;
                    foreach (var child in EntityManager.GetBuffer<Child>(entity))
                    {
#if UNITY_EDITOR
                        ecb.AddComponent(child.Value, new URPMaterialPropertyBaseColor()
                        {
                            Value = new float4(0,255,0,255)
                        });  
#endif
                    }
                }
                Debug.Log("Applied event to " + appliedTo + " agents");
                
                ecb.Playback(World.EntityManager);
            }
        }
    }
}