using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rover656.Survivors.Common;
using Rover656.Survivors.Common.Items;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Entity;
using Terresquall;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Rover656.Survivors.Client {
    public class ClientLevelManager : MonoBehaviour {
        public ClientLevel Level { get; private set; }

        [Serializable]
        public struct NamedPrefab {
            public string name;
            public GameObject prefab;
        }

        public GameObject damageParticlePrefab;

        public List<NamedPrefab> entityPrefabs = new();
        private readonly Dictionary<string, GameObject> _entityPrefabMap = new();

        // Map between entity ID and GameObject.
        private readonly Dictionary<Guid, GameObject> _gameObjects = new();

        private GameObject _playerInstance;

        public GameObject backgroundPrefab;
        private Tilemap backgroundReferenceTilemap;
        private Dictionary<Vector2Int, GameObject> _backgrounds = new();

        private void Start()
        {
            // Copy KVP from Unity into index
            foreach (var pair in entityPrefabs)
            {
                _entityPrefabMap.Add(pair.name, pair.prefab);
            }
            
            Level = new ClientLevel(this, ClientRuntimeOptions.RemoteEndpoint, ClientRuntimeOptions.LevelMode, ClientRuntimeOptions.MaxPlayTime);
            
            // Add entities that were added during initialization.
            foreach (var entity in Level.Entities)
            {
                SpawnEntity(entity);
            }
            
            _playerInstance = _gameObjects[Level.Player.Id];
            
            backgroundReferenceTilemap = backgroundPrefab.GetComponentInChildren<Tilemap>();
        }

        private Vector2 _simulatedMovementVector = Vector2.zero;

        private void Update() {
            // To avoid the potential for this feature to have a performance impact on benchmarks, only run it on the standard play.
            // for benchmarks, only spawn the middle one for consistency
            if (Level.LevelMode == LevelMode.StandardPlay) {
                // Get background size (our cells are 1 -> 1 with world bounds)
                var size = backgroundReferenceTilemap.cellBounds.size;
            
                // Get the player position in terms of background coordinates
                var playerPosition = Level.Player.Position;
                var backgroundPlayerPosition = new Vector2Int(Mathf.RoundToInt(playerPosition.x / size.x), Mathf.RoundToInt(playerPosition.y / size.y));
            
                // Get all ordinal neighbors of the background player position
                for (var x = -1; x <= 1; x++) {
                    for (var y = -1; y <= 1; y++) {
                        var bgPos = new Vector2Int(backgroundPlayerPosition.x + x, backgroundPlayerPosition.y + y);
                    
                        if (!_backgrounds.ContainsKey(bgPos)) {
                            _backgrounds.Add(bgPos, Instantiate(backgroundPrefab, new Vector3(bgPos.x * size.x, bgPos.y * size.y, 0), Quaternion.identity));
                        }
                    }
                }
            } else {
                Instantiate(backgroundPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            }

            if (Level.LevelMode == LevelMode.StandardPlay) {
                var joyInput = VirtualJoystick.GetAxis(11);
                if (joyInput.magnitude > 0)
                {
                    Level.Player.SetMovementVector(joyInput);
                }
                else
                {
                    var playerMovementVector = Vector2.zero;
                    if (Input.GetKey(KeyCode.W)) {
                        playerMovementVector.y += 1;
                    }

                    if (Input.GetKey(KeyCode.S)) {
                        playerMovementVector.y -= 1;
                    }

                    if (Input.GetKey(KeyCode.A)) {
                        playerMovementVector.x -= 1;
                    }

                    if (Input.GetKey(KeyCode.D)) {
                        playerMovementVector.x += 1;
                    }
                
                    playerMovementVector.Normalize();
                    Level.Player.SetMovementVector(playerMovementVector);
                }
            } else {
                if (_simulatedMovementVector.magnitude < 1f || Level.EveryNSeconds(1.5f)) {
                    var shouldRandomMove = true;
                    if (Level.EntitiesByTag.TryGetValue(GeneralEntityTags.Experience, out var experiencePickups)) {
                        if (experiencePickups.Count > 0) {
                            // Move towards the closest shard
                            var closestShard = experiencePickups
                                .OrderBy(e => (e.Position - Level.Player.Position).magnitude)
                                .First();
                            
                            _simulatedMovementVector = closestShard.Position - Level.Player.Position;
                            _simulatedMovementVector.Normalize();
                            shouldRandomMove = false;
                        }
                    }
                    
                    // Generate random movement vector
                    if (shouldRandomMove) {
                        var randomX = UnityEngine.Random.Range(-1f, 1f);
                        var randomY = UnityEngine.Random.Range(-1f, 1f);
                        _simulatedMovementVector = new Vector2(randomX, randomY);
                        _simulatedMovementVector.Normalize();
                    }
                }
                
                Level.Player.SetMovementVector(_simulatedMovementVector);
            }
            
            // Trigger level updates.
            Level.Update();
        }
        
        public void SpawnEntity(AbstractEntity entity)
        {
            // Will be captured after initialization
            if (Level == null) {
                return;
            }
            
            var entityTypeName = Level.Registries.GetNameFrom(FrameworkRegistries.EntityTypes, entity.Type);
            if (_entityPrefabMap.TryGetValue(entityTypeName, out var prefab))
            {
                var entityObject = Instantiate(prefab, entity.Position, Quaternion.identity);
                
                if (Level.HasTag(entity, GeneralEntityTags.FaceMovementVector))
                {
                    entityObject.transform.up = entity.MovementVector;
                }
                
                _gameObjects.Add(entity.Id, entityObject);
            }
            else
            {
                Debug.LogError($"Error spawning entity: Missing prefab mapping for entity type '{entityTypeName}'.");
            }
        }

        public void UpdateEntityDirection(AbstractEntity entity) {
            if (_gameObjects.TryGetValue(entity.Id, out var representative)) {
                if (Level.HasTag(entity, GeneralEntityTags.FaceMovementVector))
                {
                    representative.transform.up = entity.MovementVector;
                }
            }
        }

        public void UpdateEntityPosition(AbstractEntity entity) {
            if (_gameObjects.TryGetValue(entity.Id, out var representative)) {
                representative.transform.position = entity.Position;
            }
        }

        public void DestroyEntity(AbstractEntity entity) {
            if (_gameObjects.Remove(entity.Id, out var representative)) {
                Destroy(representative);
            }
        }

        public void SpawnDamageParticle(Guid atEntity, int damage) {
            if (_gameObjects.TryGetValue(atEntity, out var entity)) {
                var damageText = Instantiate(damageParticlePrefab, entity.transform.position + Vector3.up * 0.5f, Quaternion.identity);
                damageText.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(damage.ToString());                
            }
        }

        public void UpdateItemsList() {
            _playerInstance.GetComponent<PlayerUI>().UpdateItems();
        }

        public void ReturnToMainMenu(int delaySeconds = 0) {
            if (delaySeconds > 0) {
                StartCoroutine(ReturnToMenuAfter(delaySeconds));
            } else {
                SceneManager.LoadScene("MainMenu");
            }
        }

        private IEnumerator ReturnToMenuAfter(int seconds) {
            yield return new WaitForSeconds(seconds);
            SceneManager.LoadScene("MainMenu");
        }
        
        public void QueueItemChoices(Item item1, Item item2) {
            PlayerUI.Instance.QueueItemChoices(item1, item2);
        }
    }
}