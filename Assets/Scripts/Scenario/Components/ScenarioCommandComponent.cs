using Unity.Entities;

namespace Scenario.Components
{
    public struct ScenarioCommandComponent : IBufferElementData
    {
        public ScenarioCommandKindEnum Kind;
        
        public uint? IdValue;
        public float? XValue;
        public float? YValue;
        public float? RadiusValue;
        public double? SecondsValue;
    }
}