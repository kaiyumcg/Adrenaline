using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExt;
using AttributeExt2;
using UnityEngine.Events;

namespace Adrenaline
{
    [RequireComponent(typeof(Rigidbody))]
    public class GameObstacle : MonoBehaviour
    {
        [SerializeField] bool hardObstacle = false;
        [SerializeField] bool makeInactiveAfterReceiverDeath = true;
        [SerializeField] bool justInteract = true;
        [SerializeField] float interactionCooldownTime = 0.2f;
        [SerializeField] bool oneHitKill = true;
        [SerializeField] float damageAmount = 2f;
        [SerializeField] bool useTagBasedDetection = false;
        [SerializeField, Tag] List<string> receiverTags;
        [SerializeField] bool useDelay = false;
        [SerializeField] float delayAmount = 1.5f;
        [SerializeReference, SerializeReferenceButton] List<IEffectPlay> startEffects, interactEffects, receiverDeathEffects, damageReceiverEffects;
        [SerializeField] UnityEvent onStart, onReceiverDeath;
        [SerializeField] UnityEvent onInteract;
        [SerializeField] UnityEvent<float> onReceiverDamage;
        public bool UsedByReciever { get { return used; } }
        public Transform _Transform { get { return _transform; } }
        public UnityEvent OnStart { get { return onStart; } }
        public UnityEvent OnInteract { get { return onInteract; } }
        public UnityEvent OnReceiverDeath { get { return onReceiverDeath; } }
        public UnityEvent<float> OnReceiverDamage { get { return onReceiverDamage; } }

        #region ForOverrideSupport
        public bool HardObstacle { get { return hardObstacle; }  set { hardObstacle = value; } }
        public bool MakeInactiveAfterReceiverDeath { get { return makeInactiveAfterReceiverDeath; } set { makeInactiveAfterReceiverDeath = value; } }
        public bool JustInteract { get { return justInteract; } set { justInteract = value; } }
        public float InteractionCooldownTime { get { return interactionCooldownTime; } set { interactionCooldownTime = value; } }
        public bool OneHitKill { get { return oneHitKill; } set { oneHitKill = value; } }
        public float DamageAmount { get { return damageAmount; } set { damageAmount = value; } }
        public bool UseTagBasedDetection { get { return useTagBasedDetection; } set { useTagBasedDetection = value; } }
        public List<string> ReceiverTags { get { return receiverTags; } set { receiverTags = value; } }
        public bool UseDelay { get { return useDelay; } set { useDelay = value; } }
        public float DelayAmount { get { return delayAmount; } set { delayAmount = value; } }
        public List<IEffectPlay> StartEffects { get { return startEffects; } set { startEffects = value; } }
        public List<IEffectPlay> InteractEffects { get { return interactEffects; } set { interactEffects = value; } }
        public List<IEffectPlay> ReceiverDeathEffects { get { return receiverDeathEffects; } set { receiverDeathEffects = value; } }
        public List<IEffectPlay> DamageReceiverEffects { get { return damageReceiverEffects; } set { damageReceiverEffects = value; } }
        #endregion

        bool used = false;
        Transform _transform;
        bool startedAlready = false;
        float intCoolTimer = 0.0f;
        Coroutine interactionCooldownHandle = null;
        bool withinCoolDown = false;

        private void OnDisable()
        {
            ResetObstacle();
        }

        public void StartObstacleExternally()
        {
            if (startedAlready) { return; }
            gameObject.SetActive(true);
            startedAlready = true;
            _transform = transform;
            used = false;
            StartCoroutine(COR());
            IEnumerator COR()
            {
                if (useDelay)
                {
                    yield return new WaitForSeconds(delayAmount);
                }
                OnStartObstacle();
            }
        }

        IEnumerator Start()
        {
            startedAlready = true;
            intCoolTimer = 0.0f;
            interactionCooldownHandle = null;
            withinCoolDown = false;
            _transform = transform;
            used = false;
            if (useDelay)
            {
                yield return new WaitForSeconds(delayAmount);
            }
            OnStartObstacle();
        }
        protected virtual void OnStartObstacle() 
        { 
            onStart?.Invoke();
            startEffects.ExForEachSafeCustomClass((eff) =>
            {
                if (eff != null) { eff.SpawnAndPlay(this); }
            });
        }
        protected virtual void OnStartReceiverDeath(IObstacleReceiverActor reciever) 
        { 
            onReceiverDeath?.Invoke();
            receiverDeathEffects.ExForEachSafeCustomClass((eff) =>
            {
                if (eff != null) { eff.SpawnAndPlay(this); }
            });
        }
        protected virtual void OnJustInteractWithActor(IObstacleReceiverActor reciever)
        {
            onInteract?.Invoke();
            interactEffects.ExForEachSafeCustomClass((eff) =>
            {
                if (eff != null) { eff.SpawnAndPlay(this); }
            });
        }
        protected virtual void OnDamageActor(IObstacleReceiverActor reciever)
        {
            onReceiverDamage?.Invoke(damageAmount);
            damageReceiverEffects.ExForEachSafeCustomClass((eff) =>
            {
                if (eff != null) { eff.SpawnAndPlay(this); }
            });
        }
        public virtual void ResetObstacle()
        {
            used = false;
            startedAlready = false;
            intCoolTimer = 0.0f;
            interactionCooldownHandle = null;
            withinCoolDown = false;
            StopAllCoroutines();
        }
        private void OnCollisionEnter(Collision col)
        {
            if (!hardObstacle) { return; }
            ObstacleExec(col.collider);
        }
        void OnTriggerEnter(Collider col)
        {
            if (hardObstacle) { return; }
            ObstacleExec(col);
        }
        void ObstacleExec(Collider col)
        {
            if (used) { return; }
            if (useTagBasedDetection)
            {
                var recieverHasAnyTag = false;
                receiverTags.ExForEachSafeCustomClass((rtag) =>
                {
                    if (col.CompareTag(rtag))
                    {
                        recieverHasAnyTag = true;
                    }
                });
                if (!recieverHasAnyTag) { return; }
            }

            var volume = col.GetComponent<ObstacleReceiver>();
            if (volume == null) { return; }
            if (volume.UseTagBasedRestriction)
            {
                var attackerHasAnyTag = false;
                var currentTag = _transform.tag;
                volume.AllowableObstacleTags.ExForEachSafeCustomClass((rtag) =>
                {
                    if (currentTag == rtag)
                    {
                        attackerHasAnyTag = true;
                    }
                });
                if (!attackerHasAnyTag) { return; }
            }

            var owner = volume.Owner;
            if (owner == null || owner.IsDead || owner.HasDeathBeenStarted) { return; }
            if (justInteract)
            {
                if (!withinCoolDown)
                {
                    withinCoolDown = true;
                    OnJustInteractWithActor(owner);
                    owner.OnInteract(this);
                    interactionCooldownHandle = StartCoroutine(CooldownInteract());
                }
                return;
            }

            if (oneHitKill)
            {
                owner.Life = 0.0f;
            }
            else
            {
                owner.Damage(damageAmount);
                owner.Life -= damageAmount;
                OnDamageActor(owner);
            }
            if (owner.Life <= 0.0f)
            {
                if (makeInactiveAfterReceiverDeath) { used = true; }
                owner.Killer = this;
                owner.OnStartDeath();
                OnStartReceiverDeath(owner);
            }

            IEnumerator CooldownInteract()
            {
                intCoolTimer = 0.0f;
                while (true)
                {
                    intCoolTimer += Time.deltaTime;
                    if (intCoolTimer > interactionCooldownTime)
                    {
                        intCoolTimer = 0.0f;
                        break;
                    }
                    yield return null;
                }
                withinCoolDown = false;
            }
        }
    }
}