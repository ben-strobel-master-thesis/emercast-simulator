using Agents.Components;
using Scenario.Components;
using Unity.Entities;
using UnityEngine;

namespace Agents.Authoring
{
    public class ProtocolAuthoringAuthoring : MonoBehaviour
    {
        private class ProtocolAuthoringBaker : Baker<ProtocolAuthoringAuthoring>
        {
            public override void Bake(ProtocolAuthoringAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                
                AddComponent(entity, new ProtocolComponent()
                {
                    Id = 0,
                    HasMessage = false,
                    PhaseChangedTime = 0,
                    Hops = 0,
                    Phase = 0
                });
                AddBuffer<ScenarioCommandComponent>(entity);
            }
        }
    }
}