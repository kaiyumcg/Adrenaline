using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExt;
using System;

namespace Adrenaline
{
    public interface IObstacleReceiverActor
    {
        public Transform ModelRoot { get; }
        public Transform ModelRotationRoot { get; }
        public bool HasDeathBeenStarted { get; }
        public bool IsDead { get; }
        public GameObstacle Killer { get; set; }
        public void OnStartDeath();
        public void OnInteract(GameObstacle obstacle);
        public void Damage(float damageAmount);
        public float Life { get; set; }
    }
}