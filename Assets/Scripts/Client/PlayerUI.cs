using System;
using System.Collections.Generic;
using Rover656.Survivors.Common.Registries;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Rover656.Survivors.Client
{
    public class PlayerUI : MonoBehaviour {
        
        public static PlayerUI Instance { get; private set; }
        
        [FormerlySerializedAs("PlayerHealthBar")]
        public SlicedFilledImage playerHealthBar;

        [FormerlySerializedAs("TimeText")]
        public TextMeshProUGUI timeText;
        [FormerlySerializedAs("LevelText")]
        public TextMeshProUGUI levelText;

        [FormerlySerializedAs("ExperienceBar")]
        public SlicedFilledImage experienceBar;
        
        private ClientLevelManager _clientLevelManager;
        private ClientLevel Level => _clientLevelManager.Level;

        public RectTransform playerItemsGrid;

        public GameObject inventoryItemPrefab;

        public GameObject winScreen;
        public GameObject loseScreen;

        [Serializable]
        public struct NamedSprite {
            public string itemName;
            public Sprite sprite;
        }
        
        public List<NamedSprite> itemSprites = new();
        private Dictionary<string, Sprite> _itemSpriteMap = new();
        
        private void Start()
        {
            Instance = this;
            
            // Copy KVP from Unity into index
            foreach (var pair in itemSprites)
            {
                _itemSpriteMap.Add(pair.itemName, pair.sprite);
            }
            
            _clientLevelManager = FindAnyObjectByType<ClientLevelManager>();

            UpdateItems();
        }

        private void Update()
        {
            // Update healthbar width
            playerHealthBar.fillAmount = Level.Player.Health / (float)Level.Player.MaxHealth;

            // TODO
            experienceBar.fillAmount = Level.Player.Experience / (float)Level.Player.NextExperienceMilestone;
            levelText.text = $"Level {Level.Player.Level}";
            
            // Update clock
            var minutes = (int)(Level.GameTime / 60);
            var secs = (int)(Level.GameTime % 60);
            timeText.text = $"{minutes:D2}:{secs:D2}";
        }

        public void UpdateItems() {
            // Destroy any existing items in the grid
            foreach (Transform child in playerItemsGrid) {
                Destroy(child.gameObject);
            }
            
            // Fill all player items
            foreach (var item in Level.Player.Inventory) {
                var itemName = Level.Registries.GetNameFrom(SurvivorsRegistries.Items, item.Item);
                    
                var itemObject = Instantiate(inventoryItemPrefab, playerItemsGrid);
                itemObject.name = itemName;
                
                var itemImage = itemObject.GetComponentInChildren<Image>();
                var itemText = itemObject.GetComponentInChildren<TextMeshProUGUI>();
                
                if (_itemSpriteMap.TryGetValue(itemName, out var sprite))
                {
                    itemImage.sprite = sprite;
                }
                
                itemText.text = $"{item.Count}";
            }
        }

        public void ReturnToMenu() {
            SceneManager.LoadScene("MainMenu");
        }
    }
}