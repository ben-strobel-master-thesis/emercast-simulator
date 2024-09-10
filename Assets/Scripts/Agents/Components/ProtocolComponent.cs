using Unity.Entities;

namespace Agents.Components
{
    public struct ProtocolComponent : IComponentData
    {
        public bool hasMessage;
        
        public double lastScanTime;
    }
}