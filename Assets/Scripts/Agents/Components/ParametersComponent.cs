using Unity.Collections;
using Unity.Entities;

namespace Agents.Components
{
    public struct ParametersComponent : IComponentData
    {
        public uint Seed;
        
        [System.NonSerialized]
        public FixedString512Bytes ScenarioFilePath;

        public bool ProtocolEnabled;

        public double ConnectivityRange;
        
        public double Phase0Duration;
        
        public double Phase1Duration;
        
        public double Phase2Duration;
        
        public double Phase3Duration;
        
        public double Phase4Duration;
    }
}