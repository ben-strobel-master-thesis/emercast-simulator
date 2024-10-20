using Unity.Entities;

namespace Agents.Components
{
    public struct AgentScenarioCommandPointerComponent : IComponentData
    {
        public int NextCommandIndex;
    }
}