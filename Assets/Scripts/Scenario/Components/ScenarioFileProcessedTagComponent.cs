using Unity.Entities;

namespace Scenario.Components
{
    public struct ScenarioFileProcessedTagComponent : IComponentData
    {
        public double BroadcastTime;
        public float BroadcastPositionX;
        public float BroadcastPositionZ;
        public float BroadcastRadius;
        
        public double EndSimulationTime;
    }
}