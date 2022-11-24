using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExt;
using AttributeExt;

namespace Adrenaline
{
    public sealed class ObstacleReceiver : MonoBehaviour
    {
        [SerializeField] MonoBehaviour ownerActor;
        [SerializeField] bool useTagBasedRestriction = false;
        [SerializeField, TagSelector] List<string> allowableObstacleTags;
        IObstacleReceiverActor owner;
#if UNITY_EDITOR
        public MonoBehaviour OwnerActorScript { get { return ownerActor; } }
#endif
        internal IObstacleReceiverActor Owner 
        { 
            get 
            {
                if (owner == null)
                {
                    owner = GetComponentInParent<IObstacleReceiverActor>();
                }
                return owner; 
            } 
        }
        internal bool UseTagBasedRestriction { get { return useTagBasedRestriction; } }
        internal List<string> AllowableObstacleTags { get { return allowableObstacleTags; } }
        void Awake()
        {
            owner = ownerActor as IObstacleReceiverActor;
        }
    }
}