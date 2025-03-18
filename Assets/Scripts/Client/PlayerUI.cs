using System;
using TMPro;
using UnityEngine;

namespace Rover656.Survivors.Client
{
    public class PlayerUI : MonoBehaviour {
        
        public RectTransform PlayerHealthBar;
        private float _playerHealthBarMaxWidth;

        public TextMeshProUGUI TimeText;

        private ClientLevelManager _clientLevelManager;
        private ClientLevel Level => _clientLevelManager.Level;
        
        private void Start()
        {
            _playerHealthBarMaxWidth = PlayerHealthBar.rect.width;
            _clientLevelManager = FindAnyObjectByType<ClientLevelManager>();
        }

        private void Update()
        {
            // Update healthbar width
            PlayerHealthBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _playerHealthBarMaxWidth * (Level.Player.Health / (float)Level.Player.MaxHealth));
            
            // Update clock
            int minutes = (int)(Level.GameTime / 60);
            int secs = (int)(Level.GameTime % 60);
            TimeText.text = $"{minutes:D2}:{secs:D2}";
        }
    }
}