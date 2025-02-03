using LiteNetLib;
using Rover656.Survivors.Common.Events;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using Rover656.Survivors.Framework.Entity;
using Rover656.Survivors.Framework.Events;
using UnityEngine;

namespace Rover656.Survivors.Client {
    public class ClientLevel : AbstractLevel {
        public override SystemEnvironment SystemEnvironment => SystemEnvironment.Local;

        // As the local system, we do not need to account for networking delay.
        public override float NetworkDelay => 0;

        private readonly ClientLevelManager _clientLevelManager;

        public ClientLevel(NetManager netManager, ClientLevelManager clientLevelManager) : base(netManager) {
            _clientLevelManager = clientLevelManager;
        }
        
        #region Unity Scene Updates
        
        // Handles entity spawn, movement and destruction.

        protected override void OnEntitySpawn(EntitySpawnEvent entitySpawnEvent) {
            base.OnEntitySpawn(entitySpawnEvent);
            _clientLevelManager?.SpawnEntity(entitySpawnEvent.Entity);
        }

        protected override void OnEntityPositionChanged(AbstractEntity entity, Vector2 position) {
            base.OnEntityPositionChanged(entity, position);
            _clientLevelManager?.UpdateEntityPosition(entity);
        }

        protected override void OnEntityDestroyed(AbstractEntity entity) {
            base.OnEntityDestroyed(entity);
            _clientLevelManager?.DestroyEntity(entity);
        }

        // TODO: Damage & Heal particles.
        protected override void OnEntityHealthChanged(EntityHealthChangedEvent healthChangedEvent)
        {
            base.OnEntityHealthChanged(healthChangedEvent);

            if (healthChangedEvent.Delta > 0) {
                _clientLevelManager?.SpawnDamageParticle(healthChangedEvent.EntityId, healthChangedEvent.Delta);
            }
        }

        #endregion
    }
}