using System.Collections.Concurrent;
using System.Collections.Generic;
using LiteNetLib;
using Rover656.Survivors.Common.World;
using UnityEngine;

namespace Rover656.Survivors.Server {
    public class ServerLevelManager : MonoBehaviour {

        public int listeningPort = 1337;
        
        private EventBasedNetListener _listener;
        private NetManager _netManager;
        
        private readonly ConcurrentDictionary<NetPeer, ServerLevel> _levels = new();

        private void OnEnable() {
            _listener = new EventBasedNetListener();

            _listener.ConnectionRequestEvent += ListenerOnConnectionRequestEvent;
            _listener.PeerConnectedEvent += ListenerOnPeerConnectedEvent;
            _listener.NetworkReceiveEvent += ListenerOnNetworkReceiveEvent;
            _listener.PeerDisconnectedEvent += ListenerOnPeerDisconnectedEvent;
            _listener.NetworkLatencyUpdateEvent += ListenerOnNetworkLatencyUpdateEvent;

            _netManager = new NetManager(_listener);
            _netManager.ChannelsCount = 4;
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

        private void ListenerOnConnectionRequestEvent(ConnectionRequest request) {
            Debug.Log($"New connection request from {request.RemoteEndPoint}");
            request.AcceptIfKey("IAMASECRETKEY");
        }

        private void ListenerOnPeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectinfo) {
            Debug.Log($"Peer {peer.Id} disconnected from server: {disconnectinfo.Reason}.");
            if (!_levels.TryRemove(peer, out var level))
            {
                Debug.LogError("Failed to remove level from levels dictionary.");
            }

            // If a benchmark just disconnected, ensure we save its last performance metrics
            if (level.LevelMode != LevelMode.StandardPlay) {
                level.BasicPerformanceMonitor.SaveToFile();
            }
        }

        private void ListenerOnNetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliverymethod) {
            // Propagate events into the level.
            _levels[peer].NetPacketProcessor.ReadAllPackets(reader, _levels[peer]);
        }

        private void ListenerOnPeerConnectedEvent(NetPeer peer) {
            var newLevel = new ServerLevel(_netManager, peer);

            if (!_levels.TryAdd(peer, newLevel))
            {
                Debug.LogError("Failed to add level to levels dictionary.");
                peer.Disconnect();
            }
        }
    }
}