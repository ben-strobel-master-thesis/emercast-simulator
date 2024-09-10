using Agents.Components;
using Unity.Entities;
using UnityEngine;

namespace Agents.Authoring
{
    public class ProtocolAuthoringAuthoring : MonoBehaviour
    {
        private class ProtocolAuthoringBaker : Baker<ProtocolAuthoringAuthoring>
        {
            public override void Bake(ProtocolAuthoringAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ProtocolComponent()
                {
                    hasMessage = false,
                    lastScanTime = 0f
                });
            }
        }
    }
}