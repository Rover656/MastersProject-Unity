using System;
using Rover656.Survivors.Common.World;
using Rover656.Survivors.Framework;
using UnityEngine;

namespace Rover656.Survivors.Common
{
    public class Player : AbstractEntity<AbstractLevel>
    {
        public override float MovementSpeed => 32f;
    }
}