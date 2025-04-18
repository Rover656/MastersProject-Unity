using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Systems;
using UnityEngine;

namespace Rover656.Survivors.Server
{
    public class ServerLevel : AbstractLevel
    {
        public override SystemEnvironment SystemEnvironment => SystemEnvironment.Remote;
        
        public float PeerNetworkDelay { get; set; }
        
        public override Environment Environment => Environment.Remote;

        public override float DeltaTime => base.DeltaTime + PeerNetworkDelay;

        private float _performanceTimer;
        private float _forcedPerformanceTimerAt;
        protected override float PerformanceTimer => _performanceTimer;

        public ServerLevel(NetManager netManager, NetPeer peer) : base(netManager)
        {
            NetPeer = peer;
        }

        public override void Update() {
            base.Update();
            
            // Increment the performance timer so our metrics aren't skewed.
            if (_forcedPerformanceTimerAt < Time.time) {
                _performanceTimer += DeltaTime;
            }
        }

        protected override void DeserializeTickMeta(NetDataReader reader) {
            base.DeserializeTickMeta(reader);

            // Now use this
            _performanceTimer = GameTime;
            _forcedPerformanceTimerAt = Time.time;
        }
    }
}