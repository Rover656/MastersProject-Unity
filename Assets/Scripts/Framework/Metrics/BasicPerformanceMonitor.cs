using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LiteNetLib;
using UnityEditor;
using UnityEngine;

namespace Rover656.Survivors.Framework.Metrics {
    public class BasicPerformanceMonitor {
        private string _runIdentifier;
        private List<Metrics> _metrics = new();

        public BasicPerformanceMonitor(string runIdentifier) {
            _runIdentifier = runIdentifier + "-" + Guid.NewGuid().ToString("N");
        }

        public void Report(float gameTime, int entityCount, int updatesThisSecond, int eventsThisSecond, int systemCount,
            float systemRunTime, int ping, NetStatistics netStatistics) {
            // TODO: Create and record metrics.

            _metrics.Add(new Metrics() {
                Time = gameTime,
                EntityCount = entityCount,
                UpdatesThisSecond = updatesThisSecond,
                EventsThisSecond = eventsThisSecond,
                SystemCount = systemCount,
                SystemRunTime = systemRunTime,
                RemotePeerPing = ping,
                BytesReceived = netStatistics?.BytesReceived ?? 0,
                BytesSent = netStatistics?.BytesSent ?? 0,
                PacketsSent = netStatistics?.PacketsSent ?? 0,
                PacketsReceived = netStatistics?.PacketsReceived ?? 0,
                PacketLossPercent = netStatistics?.PacketLossPercent ?? 0,
            });
        }

        public void SaveToFile() {
            var csv = ToCsv();

            // The target file path e.g.
#if UNITY_EDITOR
            var folder = Application.streamingAssetsPath;

            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
#else
            var folder = Application.persistentDataPath;
#endif

            var filePath = Path.Combine(folder, _runIdentifier + ".csv");

            using (var writer = new StreamWriter(filePath, false)) {
                writer.Write(csv);
            }

            Debug.Log($"Metrics saved to: {filePath}");

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        private string ToCsv() {
            var sb = new StringBuilder(
                "Time,EntityCount,Frames,Events,SystemCount,SystemRunTime,RemotePeerPing,BytesReceived,BytesSent,PacketsSent,PacketsReceived,PacketLossPercent");
            foreach (var metric in _metrics) {
                sb.Append("\n")
                    .Append(metric.Time)
                    .Append(",")
                    .Append(metric.EntityCount)
                    .Append(",")
                    .Append(metric.UpdatesThisSecond)
                    .Append(",")
                    .Append(metric.EventsThisSecond)
                    .Append(",")
                    .Append(metric.SystemCount)
                    .Append(",")
                    .Append(metric.SystemRunTime)
                    .Append(",")
                    .Append(metric.RemotePeerPing)
                    .Append(",")
                    .Append(metric.BytesReceived)
                    .Append(",")
                    .Append(metric.BytesSent)
                    .Append(",")
                    .Append(metric.PacketsSent)
                    .Append(",")
                    .Append(metric.PacketsReceived)
                    .Append(",")
                    .Append(metric.PacketLossPercent);
            }

            return sb.ToString();
        }

        private class Metrics {
            public float Time { get; set; }
            public int EntityCount { get; set; }
            public int UpdatesThisSecond { get; set; }
            public int EventsThisSecond { get; set; }
            public int SystemCount { get; set; }
            public float SystemRunTime { get; set; }
            public int RemotePeerPing { get; set; }
            public long BytesReceived { get; set; }
            public long BytesSent { get; set; }
            public long PacketsSent { get; set; }
            public long PacketsReceived { get; set; }
            public long PacketLossPercent { get; set; }
        }
    }
}