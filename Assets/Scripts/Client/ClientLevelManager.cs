using System;
using System.Collections.Generic;
using Rover656.Survivors.Common;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Entity;
using Terresquall;
using TMPro;
using UnityEngine;

namespace Rover656.Survivors.Client {
    public class ClientLevelManager : MonoBehaviour {
        private ClientLevel _level;
        
        public ClientLevel Level => _level;
        
        [Serializable]
        public struct NamedPrefab {
            public string name;
            public GameObject prefab;
        }

        public GameObject damageParticlePrefab;

        public List<NamedPrefab> entityPrefabs = new();
        private Dictionary<string, GameObject> _entityPrefabMap = new();

        // Map between entity ID and GameObject.
        private readonly Dictionary<Guid, GameObject> _gameObjects = new();

        private void Start()
        {
            // Copy KVP from Unity into index
            foreach (var pair in entityPrefabs)
            {
                _entityPrefabMap.Add(pair.name, pair.prefab);
            }
            
            _level = new ClientLevel(null, this);
            
            // Add entities that were added during initialization.
            foreach (var entity in _level.Entities)
            {
                SpawnEntity(entity);
            }
        }

        private void Update() {
            // TODO: Temporary input logic for player
            
            Vector2 joyInput = VirtualJoystick.GetAxis(11);
            if (joyInput.magnitude > 0)
            {
                _level.Player.SetMovementVector(joyInput.normalized);
            }
            else
            {
                Vector2 playerMovementVector = Vector2.zero;
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
                _level.Player.SetMovementVector(playerMovementVector);
            }
            
            // Trigger level updates.
            _level.Update();
        }
        
        public void SpawnEntity(AbstractEntity entity)
        {
            // Will be captured after initialization
            if (_level == null) {
                return;
            }
            
            var entityTypeName = _level.Registries.GetNameFrom(FrameworkRegistries.EntityTypes, entity.Type);
            if (_entityPrefabMap.TryGetValue(entityTypeName, out var prefab))
            {
                var entityObject = Instantiate(prefab, entity.Position, Quaternion.identity);
                
                if (_level.HasTag(entity, GeneralEntityTags.FaceMovementVector))
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
                if (_level.HasTag(entity, GeneralEntityTags.FaceMovementVector))
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
    }
}