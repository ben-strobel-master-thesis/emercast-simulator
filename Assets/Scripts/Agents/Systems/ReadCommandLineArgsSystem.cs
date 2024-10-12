using Unity.Entities;
using UnityEngine;

namespace Agents.Systems
{
    public partial class ReadCommandLineArgsSystem : SystemBase
    {
        private bool Executed = false;
        
        protected override void OnUpdate()
        {
            if (Executed) return;
            
            var args = System.Environment.GetCommandLineArgs();
            Debug.Log("Command line args: " + string.Join(" ", args));
            Executed = true;
        }
    }
}