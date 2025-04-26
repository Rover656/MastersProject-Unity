using Rover656.Survivors.Common.World;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Rover656.Survivors.Client {
    public class MainMenu : MonoBehaviour {
        public string clientGameScene = "ClientLevel";
        public string integratedClientGameScene = "IntegratedLevel";
        public TMP_InputField remoteIPField;
        public Toggle integratedServerToggle;

        public int benchmarkRunTime = 3 * 60; // 3 minutes
        public int standardGameRunTime = 5 * 60; // 5 minutes
        
        private void Start()
        {
            Application.targetFrameRate = 60;
            
            remoteIPField.text = ClientRuntimeOptions.RemoteEndpoint;
            integratedServerToggle.isOn = ClientRuntimeOptions.RunIntegratedServer;
        }

        public void PlayStandard() {
            ClientRuntimeOptions.LevelMode = LevelMode.StandardPlay;
            ClientRuntimeOptions.MaxPlayTime = standardGameRunTime;
            StartLevel();
        }

        public void StartStandardBenchmark() {
            ClientRuntimeOptions.LevelMode = LevelMode.StandardBenchmark;
            ClientRuntimeOptions.MaxPlayTime = benchmarkRunTime;
            StartLevel();
        }

        public void StartLocalBenchmark() {
            ClientRuntimeOptions.LevelMode = LevelMode.LocalBenchmark;
            ClientRuntimeOptions.MaxPlayTime = benchmarkRunTime;
            StartLevel();
        }
        
        public void StartRemoteBenchmark() {
            ClientRuntimeOptions.LevelMode = LevelMode.RemoteBenchmark;
            ClientRuntimeOptions.MaxPlayTime = benchmarkRunTime;
            StartLevel();
        }

        private void StartLevel() {
            ClientRuntimeOptions.RemoteEndpoint = remoteIPField.text;
            ClientRuntimeOptions.RunIntegratedServer = integratedServerToggle.isOn;

            SceneManager.LoadScene(ClientRuntimeOptions.RunIntegratedServer
                ? integratedClientGameScene
                : clientGameScene);
        }
    }
}