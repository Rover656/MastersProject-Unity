using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Rover656.Survivors.Client
{
    public class PlayerUI : MonoBehaviour {
        
        public SlicedFilledImage PlayerHealthBar;

        public TextMeshProUGUI TimeText;

        public SlicedFilledImage ExperienceBar;
        
        private ClientLevelManager _clientLevelManager;
        private ClientLevel Level => _clientLevelManager.Level;

        public GameObject inventoryItemPrefab;
        public List<KeyValuePair<string, Sprite>> itemSprites = new();
        private Dictionary<string, Sprite> _itemSpriteMap = new();
        
        private void Start()
        {
            // Copy KVP from Unity into index
            foreach (var pair in itemSprites)
            {
                _itemSpriteMap.Add(pair.Key, pair.Value);
            }
            
            _clientLevelManager = FindAnyObjectByType<ClientLevelManager>();
        }

        private void Update()
        {
            // Update healthbar width
            PlayerHealthBar.fillAmount = Level.Player.Health / (float)Level.Player.MaxHealth;

            // TODO
            ExperienceBar.fillAmount = 0.5f;
            
            // Update clock
            int minutes = (int)(Level.GameTime / 60);
            int secs = (int)(Level.GameTime % 60);
            TimeText.text = $"{minutes:D2}:{secs:D2}";
        }
    }
}