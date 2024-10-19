using Scenario.Components;
using Unity.Entities;
using UnityEngine;

namespace Scenario.Authoring
{
    public class ScenarioHelperAuthoring : MonoBehaviour
    {
        public GameObject agentRepresentation;
        
        private class ScenarioHelperAuthoringBaker : Baker<ScenarioHelperAuthoring>
        {
            public override void Bake(ScenarioHelperAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ScenarioHelperComponent()
                {
                    AgentRepresentationPrefabEntity = GetEntity(authoring.agentRepresentation, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}