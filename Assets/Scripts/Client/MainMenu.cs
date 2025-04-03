using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Rover656.Survivors.Client {
    public class MainMenu : MonoBehaviour {
        public string clientGameScene = "ClientLevel";
        public string integratedClientGameScene = "IntegratedLevel";
        public TMP_InputField remoteIPField;
        public Toggle integratedServerToggle;

        private void Start()
        {
            Application.targetFrameRate = 60;
        }

        public void PlayStandard() {
            // TODO: Set any standard parameters.
            StartLevel();
        }

        public void StartStandardBenchmark() {
            // TODO
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

            if (integratedServerToggle.isOn) {
                SceneManager.LoadScene(integratedClientGameScene);
            } else {
                SceneManager.LoadScene(clientGameScene);
            }
        }
    }
}