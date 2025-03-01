using System;
using System.Collections.Generic;
using LiteNetLib;
using UnityEngine;

namespace Rover656.Survivors.Server {
    public class ServerLevelManager : MonoBehaviour {

        public int listeningPort = 1337;
        
        private EventBasedNetListener _listener;
        private NetManager _netManager;

        private readonly object _levelLock = new();
        
        private readonly Dictionary<NetPeer, ServerLevel> _levels = new();
        private readonly Dictionary<NetPeer, ServerLevel> _pendingLevels = new();

        private void OnEnable() {
            _listener = new EventBasedNetListener();

            _listener.ConnectionRequestEvent += ListenerOnConnectionRequestEvent;
            _listener.PeerConnectedEvent += ListenerOnPeerConnectedEvent;
            _listener.NetworkReceiveEvent += ListenerOnNetworkReceiveEvent;
            _listener.PeerDisconnectedEvent += ListenerOnPeerDisconnectedEvent;
            _listener.NetworkLatencyUpdateEvent += ListenerOnNetworkLatencyUpdateEvent;

            _netManager = new NetManager(_listener);
            _netManager.Start(listeningPort);
        }

        private void ListenerOnNetworkLatencyUpdateEvent(NetPeer peer, int latency)
        {
            // latency in MS
            lock (_levelLock)
            {
                if (_levels.TryGetValue(peer, out var level))
                {
                    level.PeerNetworkDelay = latency / 1000f;
                }
            }
        }

        private void OnDisable() {
            lock (_levelLock) {
                _netManager.Stop();
                _levels.Clear();
            }
        }

        private void Update() {
            // Receive all incoming network messages
            _netManager.PollEvents();
            
            // Perform all level updates.
            // TODO: It'd be more optimal to run these in their own threads, however for the sake of implementing this
            //       purely within Unity with little effort, this will suffice.
            lock (_levelLock) {
                foreach (var level in _levels) {
                    level.Value.Update();
                }
            }
        }

        private void ListenerOnConnectionRequestEvent(ConnectionRequest request)
        {
            var peer = request.Accept();
            var newLevel = new ServerLevel(_netManager, peer);
            newLevel.DeserializeWorld(request.Data);
            
            _pendingLevels.Add(peer, newLevel);
        }

        private void ListenerOnPeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectinfo) {
            lock (_levelLock) {
                _levels.Remove(peer);
            }
        }

        private void ListenerOnNetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliverymethod) {
            // Propagate events into the level.
            lock (_levelLock) {
                _levels[peer].NetPacketProcessor.ReadAllPackets(reader, _levels[peer]);
            }
        }

        private void ListenerOnPeerConnectedEvent(NetPeer peer) {
            lock (_levelLock) {
                if (_pendingLevels.Remove(peer, out var level)) {
                    _levels.Add(peer, level);
                } else {
                    peer.Disconnect();
                }
            }
        }
    }
}