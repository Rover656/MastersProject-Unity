using System;
using System.Collections.Generic;
using Rover656.Survivors.Common;
using Rover656.Survivors.Common.Entities;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Entity;
using UnityEngine;

namespace Rover656.Survivors.Client {
    public class ClientLevelManager : MonoBehaviour {
        // TODO: This will be in the stage scene and will bootstrap the logical level.
        public GameObject playerPrefab;

        private ClientLevel _level;

        // Map between entity ID and GameObject.
        private readonly Dictionary<Guid, GameObject> _gameObjects = new();

        private void Start() {
            _level = new ClientLevel(null, this);
            
            // Add entities that were added during initialization.
            foreach (var entity in _level.Entities)
            {
                SpawnEntity(entity);
            }
        }

        private void Update() {
            // TODO: Temporary input logic for player

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
            
            // Trigger level system updates.
            _level.Update();
        }
        
        public void SpawnEntity(AbstractEntity entity) {
            GameObject newEntity = null;
            if (entity is Player) {
                newEntity = Instantiate(playerPrefab, entity.Position, Quaternion.identity);
            }

            if (newEntity != null) {
                _gameObjects.Add(entity.Id, newEntity);
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
    }
}