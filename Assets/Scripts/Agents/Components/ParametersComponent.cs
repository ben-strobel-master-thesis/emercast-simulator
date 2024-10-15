using Unity.Collections;
using Unity.Entities;

namespace Agents.Components
{
    public struct ParametersComponent : IComponentData
    {
        public uint Seed;
        
        [System.NonSerialized]
        public FixedString512Bytes ScenarioFilePath;

        public double ConnectivityRange;
        
        public double Phase0Duration;
        public double Phase0PreDelay;
        
        public double Phase1Duration;
        public double Phase1PreDelay;
        
        public double Phase2Duration;
        public double Phase2PreDelay;
        
        public double Phase3Duration;
        public double Phase3PreDelay;
        
        public double Phase4Duration;
        public double Phase4PreDelay;
    }
}