using System;
using Agents.Components;
using Unity.Entities;
using UnityEngine;

namespace Agents.Systems
{
    public partial class TimeLoggingSystem : SystemBase
    {
        private long timeStarted = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        private double lastRealTimeLogged = 0;
        
        protected override void OnUpdate()
        {
            var simulatedTime = World.Time.ElapsedTime;
            var realTime = (DateTime.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(timeStarted).DateTime).TotalSeconds;
            
            if (lastRealTimeLogged + 5 > realTime)
            {
                return;
            }

            var withMessage = 0;
            var withoutMessage = 0;
            foreach (var protocolComponent in SystemAPI.Query<RefRO<ProtocolComponent>>())
            {
                if (protocolComponent.ValueRO.hasMessage)
                {
                    withMessage++;
                }
                else
                {
                    withoutMessage++;
                }
            }
            
            Debug.Log("Simulated time: " + simulatedTime + " Real time: " + realTime + " Msg: " + withMessage + " NoMsg: " + withoutMessage);
            lastRealTimeLogged = realTime;
        }
    }
}