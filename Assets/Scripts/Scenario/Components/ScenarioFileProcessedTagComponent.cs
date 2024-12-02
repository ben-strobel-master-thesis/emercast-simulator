using Unity.Entities;

namespace Scenario.Components
{
    public struct ScenarioFileProcessedTagComponent : IComponentData
    {
        public double BroadcastTime;
        
        public float OutagePositionX;
        public float OutagePositionZ;
        public float OutageRadius;
        
        public double EndSimulationTime;
    }
}