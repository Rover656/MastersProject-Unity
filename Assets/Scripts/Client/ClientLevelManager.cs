using System;
using System.Collections.Generic;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using UnityEngine;

namespace Rover656.Survivors.Client
{
    public class ClientLevelManager : MonoBehaviour
    {
        // TODO: This will be in the stage scene and will bootstrap the logical level.

        private ClientLevel _level;

        // Map between entity ID and GameObject.
        private Dictionary<Guid, GameObject> _gameObjects;

        private void Start()
        {
            throw new NotImplementedException();
        }

        private void Update()
        {
            // TODO: Temporary input logic for player

            Vector2 playerMovementVector = Vector2.zero;
            if (Input.GetKeyDown(KeyCode.W))
            {
                playerMovementVector.y -= 1;
            }
            
            if (Input.GetKeyDown(KeyCode.S))
            {
                playerMovementVector.y += 1;
            }
            
            if (Input.GetKeyDown(KeyCode.A))
            {
                playerMovementVector.x -= 1;
            }
            
            if (Input.GetKeyDown(KeyCode.D))
            {
                playerMovementVector.x += 1;
            }
            playerMovementVector.Normalize();

            _level.Player.SetMovementVector(playerMovementVector);
        }

        // TODO: Relevant hooks to the ClientLevel to spawn & delete Prefabs.

        public void SpawnEntity(AbstractEntity<AbstractLevel> entity)
        {
            // TODO: Determine which prefab to instantiate.
        }
    }
}