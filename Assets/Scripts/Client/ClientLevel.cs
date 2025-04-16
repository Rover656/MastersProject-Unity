using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using Rover656.Survivors.Common.Events;
using Rover656.Survivors.Common.Registries;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Entity;
using Rover656.Survivors.Framework.Events;
using Rover656.Survivors.Framework.Network;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Environment = Rover656.Survivors.Framework.Systems.Environment;

namespace Rover656.Survivors.Client {
    public class ClientLevel : AbstractLevel {
        public override SystemEnvironment SystemEnvironment => SystemEnvironment.Local;

        public override Environment Environment => Environment.Local;

        private readonly ClientLevelManager _clientLevelManager;

        private string _remoteEndpoint;

        public ClientLevel(ClientLevelManager clientLevelManager, string remoteEndpoint, LevelMode levelMode, int? maxPlayTime) : base(null, levelMode, maxPlayTime) {
            _clientLevelManager = clientLevelManager;
            _remoteEndpoint = remoteEndpoint;
            
            // Spawn the player
            Player = AddNewEntity(EntityTypes.Player.Create());
            
            // Attempt to connect to remote immediately
            ConnectToRemoteServer();
        }
        
        #region Unity Scene Updates

        protected override void OnQuit() {
            if (LevelMode == LevelMode.StandardPlay) {
                // TODO: Show death screen or win screen.
            } else {
                // Ensure benchmark results are flushed.
                BasicPerformanceMonitor.SaveToFile();
                
                // Return to main menu once benchmark concludes.
                SceneManager.LoadScene("MainMenu");
            }
        }

        // Handles entity spawn, movement and destruction.

        protected override void OnEntitySpawn(EntitySpawnEvent entitySpawnEvent) {
            base.OnEntitySpawn(entitySpawnEvent);
            _clientLevelManager?.SpawnEntity(entitySpawnEvent.Entity);
        }

        protected override void OnEntityMovementVectorChanged(AbstractEntity entity, Vector2 movementVector)
        {
            base.OnEntityMovementVectorChanged(entity, movementVector);
            _clientLevelManager?.UpdateEntityDirection(entity);
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
            if (healthChangedEvent.Delta > 0) {
                _clientLevelManager?.SpawnDamageParticle(healthChangedEvent.EntityId, healthChangedEvent.Delta);
            }
            
            base.OnEntityHealthChanged(healthChangedEvent);
        }

        protected override void OnPlayerLevelChanged(PlayerLevelUpEvent levelEvent) {
            if (Player.Level != levelEvent.Level && LevelMode == LevelMode.StandardPlay) {
                // Pause();
                
                // TODO: queue pop ups
            } else {
                for (int i = Player.Level; i < levelEvent.Level; i++) {
                    // TODO: Give random item
                }
            }
            
            base.OnPlayerLevelChanged(levelEvent);
        }

        #endregion

        public override void Update() {
            // Poll incoming messages from the remote.
            if (NetManager?.IsRunning ?? false) {
                NetManager.PollEvents();
            } else {
                // Attempt to reconnect every 30 seconds.
                if (EveryNSeconds(30)) {
                    ConnectToRemoteServer();
                }
            }
            
            // Run default update logic.
            base.Update();
            
            // Ensure network messages are sent after every update
            NetManager?.TriggerUpdate();
        }

        private void ConnectToRemoteServer() {
            // Do not connect to the remote server when benchmarking locally.
            if (LevelMode == LevelMode.LocalBenchmark) {
                return;
            }
            
            var listener = new EventBasedNetListener();
            listener.PeerConnectedEvent += OnPeerConnected;
            listener.NetworkReceiveEvent += OnNetworkReceived;
            listener.PeerDisconnectedEvent += OnPeerDisconnected;
            listener.NetworkErrorEvent += ListenerOnNetworkErrorEvent;

            NetManager = new NetManager(listener);
            NetManager.ChannelsCount = 4;
            NetManager.Start();
            
            // Use _remoteEndpoint, but get the port from the end, separated by a colon.
            string port = null;
            string ip = _remoteEndpoint;

            if (ip.Contains(":")) {
                port = ip.Split(':')[1];
                ip = ip.Split(':')[0];
            }
            
            // Default to 1337 if no port specified
            if (string.IsNullOrEmpty(port)) {
                port = "1337";
            }
            
            // Now create an IPEndPoint
            var ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));
            
            // Connect
            NetPeer = NetManager.Connect(ipEndPoint, "IAMASECRETKEY");
        }

        private void ListenerOnNetworkErrorEvent(IPEndPoint endpoint, SocketError socketerror) {
            Debug.LogError($"Error from {endpoint}: {socketerror}");
        }

        private void OnPeerConnected(NetPeer peer) {
            if (peer.Id == NetPeer.Id) {
                Debug.Log("Connected to remote server, sending initial data.");
                
                // Serialize current world state
                var levelData = new NetDataWriter();
                SerializeWorld(levelData);
                
                Debug.Log("Initial data serialized, queueing future events for sending after initialization.");
                
                // Immediately queue chaanges to this initial state for sending once we've established the remote world.
                BeginNetworkEventQueue();

                var initPacket = new InitGameStatePacket {
                    RawData = levelData.CopyData()
                };
                
                var writer = new NetDataWriter();
                NetPacketProcessor.Write(writer, initPacket);
                NetPeer?.Send(writer, DeliveryMethod.ReliableOrdered);
                Debug.Log("Initial data sent.");
            }
        }

        private void OnNetworkReceived(NetPeer peer, NetPacketReader reader, byte channel,
            DeliveryMethod deliveryMethod) {

            // Do not accept remote input if the game is quit.
            if (HasQuit) {
                return;
            }
            
            NetPacketProcessor.ReadAllPackets(reader, this);
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) {
            if (peer.Id == NetPeer.Id) {
                Debug.LogWarning($"Disconnected from remote server: {disconnectInfo.Reason}.");
                
                // The remote has disconnected.
                NetManager.Stop();
                NetManager = null;
                NetPeer = null;

                if (ShouldBalanceSystems) {
                    ForceOnloadAll();
                }
            }
        }
    }
}