using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CustomParticleSystem
{
    [AddComponentMenu("Custom particle system/Particle")]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Particle : MonoBehaviour
    {
        #region Listeners code
        public delegate void OnParticleCollision(Collision2D collision);

        private event OnParticleCollision collisionEnterEvent;

        public void AddCollisionEnterListener(OnParticleCollision listener)
        {
            collisionEnterEvent += listener;
        }


        private event OnParticleCollision collisionExitEvent;

        public void AddCollisionExitListener(OnParticleCollision listener)
        {
            collisionExitEvent += listener;
        }


        public delegate void OnParticleTrigger(Collider2D collider);

        private event OnParticleTrigger triggerEnterEvent;

        public void AddTriggerEnterListener(OnParticleTrigger listener)
        {
            triggerEnterEvent += listener;
        }


        private event OnParticleTrigger triggerExitEvent;

        public void AddTriggerExitListener(OnParticleTrigger listener)
        {
            triggerExitEvent += listener;
        }

        public delegate void OnLifeFinished(Particle particle);

        private event OnLifeFinished LifeFinishedEvent;

        public void AddLifeFinishedListener(OnLifeFinished listener)
        {
            LifeFinishedEvent += listener;
        }
        #endregion

        [Header("Main")]
        [SerializeField]
        private Transform particleTransform;

        [SerializeField]
        private SpriteRenderer particleSpriteRenderer;

        public Transform ParticleTransform
        { get { return particleTransform; } }

        [SerializeField]
        [Min(0)]
        private float size;

        [SerializeField]
        private Color startColor;


        [Header("Lifetime")]
        [SerializeField]
        private AnimationCurve sizeOverLifeTime;

        [SerializeField]
        private Gradient colorOverLifeTime;


        [Header("Physics")]
        [Space(7)]
        [SerializeField]
        private Rigidbody2D particleRigidbody;

        public Rigidbody2D ParticleRigidbody
        { get { return particleRigidbody; } }

        [SerializeField]
        private bool detectOtherParticles;

        public CustomParticleSystem myParticleSystem;

        private float lifetime = 1f;
        
        public float LifeTime
        {
            get { return lifetime; }
            set
            {
                if (value < 0)
                    lifetime = 0;
                else
                    lifetime = value;
            }
        }

        private bool isLife;

        private float passedLifeTime;

        private void Start()
        {
            SetupStartProperties();
        }

        private void Update()
        {
            if(isLife)
            {
                particleTransform.localScale = new Vector3(sizeOverLifeTime.Evaluate(passedLifeTime / lifetime) * size, sizeOverLifeTime.Evaluate(passedLifeTime / lifetime) * size, 1f);
                particleSpriteRenderer.color = colorOverLifeTime.Evaluate(passedLifeTime / lifetime) * startColor;
            }
            passedLifeTime += Time.deltaTime;
        }

        public void StartLife()
        {
            gameObject.SetActive(true);
            passedLifeTime = 0;
            isLife = true;
            SetupStartProperties();
            StartCoroutine(Life());
        }

        private void SetupStartProperties()
        {
            particleTransform.localScale = new Vector3(sizeOverLifeTime.Evaluate(0) * size, sizeOverLifeTime.Evaluate(0) * size, 1);
            particleSpriteRenderer.color = colorOverLifeTime.Evaluate(passedLifeTime / lifetime) * startColor;
        }

        private IEnumerator Life()
        {
            yield return new WaitForSecondsRealtime(lifetime);
            gameObject.SetActive(false);
            isLife = false;
            LifeFinishedEvent?.Invoke(this);
            if (myParticleSystem == null)
                Destroy(gameObject);
        }

        #region Physics callbacks
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isLife) return;
            if (collision.gameObject.GetComponent<Particle>() != null)
            {
                if(detectOtherParticles)
                    collisionEnterEvent?.Invoke(collision);
            }
            else
                collisionEnterEvent?.Invoke(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!isLife) return;
            if (collision.gameObject.GetComponent<Particle>() != null)
            {
                if (detectOtherParticles)
                    collisionExitEvent?.Invoke(collision);
            }
            else
                collisionExitEvent?.Invoke(collision);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isLife) return;
            if (collision.gameObject.GetComponent<Particle>() != null)
            {
                if (detectOtherParticles)
                    triggerEnterEvent?.Invoke(collision);
            }
            else
                triggerEnterEvent?.Invoke(collision);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!isLife) return;
            if (collision.gameObject.GetComponent<Particle>() != null)
            {
                if (detectOtherParticles)
                    triggerExitEvent?.Invoke(collision);
            }
            else
                triggerExitEvent?.Invoke(collision);
        }
        #endregion
    }
}
