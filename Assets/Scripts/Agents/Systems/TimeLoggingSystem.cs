using System;
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
            var realTime = (DateTime.Now - DateTimeOffset.FromUnixTimeMilliseconds(timeStarted).DateTime).TotalSeconds;
            Debug.Log(realTime);
            
            if (lastRealTimeLogged + 5 > realTime)
            {
                return;
            }
            
            Debug.Log("Simulated time: " + simulatedTime + " Real time: " + realTime);
            lastRealTimeLogged = realTime;
        }
    }
}