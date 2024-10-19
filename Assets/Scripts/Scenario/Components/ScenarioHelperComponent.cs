using Unity.Entities;

namespace Scenario.Components
{
    public struct ScenarioHelperComponent : IComponentData
    {
        public Entity AgentRepresentationPrefabEntity;
    }
}