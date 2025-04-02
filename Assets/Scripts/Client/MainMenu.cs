using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Rover656.Survivors.Client {
    public class MainMenu : MonoBehaviour {
        public string clientGameScene = "ClientLevel";
        public TMP_InputField remoteIPField;

        private void Start()
        {
            Application.targetFrameRate = 60;
        }

        public void PlayStandard() {
            // TODO: Set any standard parameters.
            StartLevel();
        }

        public void StartLocalBenchmark() {
            // TODO
            StartLevel();
        }
        
        public void StartRemoteBenchmark() {
            // TODO
            StartLevel();
        }

        private void StartLevel() {
            ClientRuntimeOptions.RemoteEndpoint = remoteIPField.text;
            SceneManager.LoadScene(clientGameScene);
        }
    }
}