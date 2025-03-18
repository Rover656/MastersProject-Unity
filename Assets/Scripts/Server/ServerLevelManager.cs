using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LiteNetLib;
using UnityEngine;

namespace Rover656.Survivors.Server {
    public class ServerLevelManager : MonoBehaviour {

        public int listeningPort = 1337;
        
        private EventBasedNetListener _listener;
        private NetManager _netManager;
        
        private readonly ConcurrentDictionary<NetPeer, ServerLevel> _levels = new();
        private readonly ConcurrentDictionary<NetPeer, ServerLevel> _pendingLevels = new();

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
            if (_levels.TryGetValue(peer, out var level))
            {
                level.PeerNetworkDelay = latency / 1000f;
            }
        }

        private void OnDisable() {
            _netManager.Stop();
            _levels.Clear();
            _pendingLevels.Clear();
        }

        private void Update() {
            // Receive all incoming network messages
            _netManager.PollEvents();
            
            // Perform all level updates.
            // TODO: It'd be more optimal to run these in their own threads, however for the sake of implementing this
            //       purely within Unity with little effort, this will suffice.
            foreach (var level in _levels) {
                level.Value.Update();
            }
            
            _netManager.TriggerUpdate();
        }

        private void ListenerOnConnectionRequestEvent(ConnectionRequest request)
        {
            var peer = request.Accept();
            var newLevel = new ServerLevel(_netManager, peer);
            newLevel.DeserializeWorld(request.Data);

            if (!_pendingLevels.TryAdd(peer, newLevel))
            {
                Debug.LogError("Failed to add level to pending levels dictionary.");
                peer.Disconnect();
            }
        }

        private void ListenerOnPeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectinfo) {
            if (!_levels.TryRemove(peer, out _))
            {
                Debug.LogError("Failed to remove level from levels dictionary.");
            }
        }

        private void ListenerOnNetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliverymethod) {
            // Propagate events into the level.
            _levels[peer].NetPacketProcessor.ReadAllPackets(reader, _levels[peer]);
        }

        private void ListenerOnPeerConnectedEvent(NetPeer peer) {
            if (_pendingLevels.Remove(peer, out var level)) {
                if (!_levels.TryAdd(peer, level))
                {
                    Debug.LogError("Failed to add level to levels dictionary.");
                    peer.Disconnect();
                }
            } else {
                peer.Disconnect();
            }
        }
    }
}