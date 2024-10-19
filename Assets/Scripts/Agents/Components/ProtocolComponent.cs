﻿using Unity.Entities;

namespace Agents.Components
{
    public struct ProtocolComponent : IComponentData
    {
        public uint Id;
        
        public uint Phase;
        public double PhaseChangedTime;
        
        public bool HasMessage;
        public uint Hops;
    }
}