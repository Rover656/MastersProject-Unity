using System;
using System.Collections.Generic;
using Rover656.Survivors.Common;
using Rover656.Survivors.Common.Registries;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rover656.Survivors.Client
{
    public class PlayerUI : MonoBehaviour {
        
        public SlicedFilledImage PlayerHealthBar;

        public TextMeshProUGUI TimeText;
        public TextMeshProUGUI LevelText;

        public SlicedFilledImage ExperienceBar;
        
        private ClientLevelManager _clientLevelManager;
        private ClientLevel Level => _clientLevelManager.Level;

        public RectTransform playerItemsGrid;

        public GameObject inventoryItemPrefab;

        [Serializable]
        public struct NamedSprite {
            public string itemName;
            public Sprite sprite;
        }
        
        public List<NamedSprite> itemSprites = new();
        private Dictionary<string, Sprite> _itemSpriteMap = new();
        
        private void Start()
        {
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
            PlayerHealthBar.fillAmount = Level.Player.Health / (float)Level.Player.MaxHealth;

            // TODO
            ExperienceBar.fillAmount = Level.Player.Experience / (float)Level.Player.NextExperienceMilestone;
            LevelText.text = $"Level {Level.Player.Level}";
            
            // Update clock
            int minutes = (int)(Level.GameTime / 60);
            int secs = (int)(Level.GameTime % 60);
            TimeText.text = $"{minutes:D2}:{secs:D2}";
        }

        public void UpdateItems() {
            // Destroy any existing items in the grid
            foreach (Transform child in playerItemsGrid) {
                Destroy(child.gameObject);
            }
            
            // Fill all player items
            foreach (var item in Level.Player.Inventory) {
                string itemName = Level.Registries.GetNameFrom(SurvivorsRegistries.Items, item.Item);
                    
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
    }
}