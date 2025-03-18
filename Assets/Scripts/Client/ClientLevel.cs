using System;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Common.Events;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Entity;
using Rover656.Survivors.Framework.Events;
using UnityEngine;
using Environment = Rover656.Survivors.Framework.Systems.Environment;

namespace Rover656.Survivors.Client {
    public class ClientLevel : AbstractLevel {
        public override SystemEnvironment SystemEnvironment => SystemEnvironment.Local;

        public override Environment Environment => Environment.Local;

        private readonly ClientLevelManager _clientLevelManager;

        public ClientLevel(NetManager netManager, ClientLevelManager clientLevelManager) : base(netManager) {
            _clientLevelManager = clientLevelManager;
            
            // Spawn the player
            Player = AddNewEntity(EntityTypes.Player.Create());

            // Add an example enemy (will be the job of the director system soon)
            AddNewEntity(EntityTypes.Bat.Create(), new Vector2(1, 2));
            AddNewEntity(EntityTypes.Bat.Create(), new Vector2(2, 1));
            // AddNewEntity(EntityTypes.Bat.Create(), new Vector2(1, 1));
            // AddNewEntity(EntityTypes.Bat.Create(), new Vector2(0, 2));
            // AddNewEntity(EntityTypes.Bat.Create(), new Vector2(2, 0));
        }
        
        #region Unity Scene Updates
        
        // Handles entity spawn, movement and destruction.

        protected override void OnEntitySpawn(EntitySpawnEvent entitySpawnEvent) {
            base.OnEntitySpawn(entitySpawnEvent);
            _clientLevelManager?.SpawnEntity(entitySpawnEvent.Entity);
        }

        protected override void OnEntityPositionChanged(AbstractEntity entity, Vector2 position) {
            base.OnEntityPositionChanged(entity, position);
            _clientLevelManager?.UpdateEntityPosition(entity);
        }

        protected override void OnEntityDestroyed(AbstractEntity entity) {
            base.OnEntityDestroyed(entity);
            _clientLevelManager?.DestroyEntity(entity);
        }

        protected override void OnEntityHealthChanged(EntityHealthChangedEvent healthChangedEvent)
        {
            base.OnEntityHealthChanged(healthChangedEvent);

            if (healthChangedEvent.Delta > 0) {
                _clientLevelManager?.SpawnDamageParticle(healthChangedEvent.EntityId, healthChangedEvent.Delta);
            }
        }

        #endregion

        public override void Update() {
            // Poll incoming messages from the remote.
            if (NetManager?.IsRunning ?? false) {
                NetManager.PollEvents();
            } else {
                if (EveryNSeconds(4)) {
                    ConnectToRemoteServer();
                }
            }
            
            // Run default update logic.
            base.Update();
            
            // Ensure network messages are sent after every update
            NetManager?.TriggerUpdate();
        }

        private void ConnectToRemoteServer() {
            // Immediately begin collecting any new events to update the remote state once it is established.
            BeginNetworkEventQueue();
            
            var listener = new EventBasedNetListener();
            listener.PeerConnectedEvent += OnPeerConnected;
            listener.NetworkReceiveEvent += OnNetworkReceived;
            listener.PeerDisconnectedEvent += OnPeerDisconnected;

            NetManager = new NetManager(listener);
            NetManager.Start();

            var levelData = new NetDataWriter();
            SerializeWorld(levelData);
            
            NetPeer = NetManager.Connect("127.0.0.1", 1337, levelData);
        }

        private void OnPeerConnected(NetPeer peer) {
            if (peer.Id == NetPeer.Id) {
                // Finish queueing messages for the remote and fire them all.
                EndNetworkEventQueue();
            }
        }

        private void OnNetworkReceived(NetPeer peer, NetPacketReader reader, byte channel,
            DeliveryMethod deliveryMethod) {
            NetPacketProcessor.ReadAllPackets(reader, this);
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) {
            if (peer.Id == NetPeer.Id) {
                // The remote has disconnected.
                NetManager.Stop();
                NetManager = null;
                NetPeer = null;

                ForceOnloadAll();
            }
        }
    }
}