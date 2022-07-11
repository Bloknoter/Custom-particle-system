using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CustomParticleSystem
{
    [AddComponentMenu("Custom particle system/Particle system")]
    public class CustomParticleSystem : MonoBehaviour
    {

        #region Listeners code
        private event Particle.OnParticleCollision collisionEnterEvent;

        public void AddCollisionEnterListener(Particle.OnParticleCollision listener)
        {
            collisionEnterEvent += listener;
        }


        private event Particle.OnParticleCollision collisionExitEvent;

        public void AddCollisionExitListener(Particle.OnParticleCollision listener)
        {
            collisionExitEvent += listener;
        }


        private event Particle.OnParticleTrigger triggerEnterEvent;

        public void AddTriggerEnterListener(Particle.OnParticleTrigger listener)
        {
            triggerEnterEvent += listener;
        }


        private event Particle.OnParticleTrigger triggerExitEvent;

        public void AddTriggerExitListener(Particle.OnParticleTrigger listener)
        {
            triggerExitEvent += listener;
        }

        private void OnParticleCollisionEnter(Collision2D collision)
        {
            collisionEnterEvent?.Invoke(collision);
        }

        private void OnParticleCollisionExit(Collision2D collision)
        {
            collisionExitEvent?.Invoke(collision);
        }

        private void OnParticleTriggerEnter(Collider2D collider)
        {
            triggerEnterEvent?.Invoke(collider);
        }

        private void OnParticleTriggerExit(Collider2D collider)
        {
            triggerExitEvent?.Invoke(collider);
        }

        #endregion

        [Header("Main")]
        [SerializeField]
        private GameObject particle;

        [SerializeField]
        private bool playOnStart;

        [SerializeField]
        [Min(0)]
        private float particlesLifeTime = 5f;

        [Header("Emission")]
        [Space(10)]
        [SerializeField]
        [Min(0)]
        private int emission = 10;

        [SerializeField]
        private Transform emissionPoint;

        [SerializeField]
        [Range(0, 180)]
        private float emissionAngleRange;


        [Header("Force")]
        [Space(10)]
        [SerializeField]
        private float force;

        [SerializeField]
        private int angle = 0;

        private enum OrientationType { Local, World }

        [SerializeField]
        private OrientationType forceType;

        [Header("Physics")]
        [Space(10)]
        [SerializeField]
        private bool detectCollisionEnter;

        [SerializeField]
        private bool detectCollisionExit;

        [SerializeField]
        private bool detectTriggerEnter;

        [SerializeField]
        private bool detectTriggerExit;

        private List<Particle> unusedParticles;

        private bool isWorking = false;

        public bool IsWorking { get { return isWorking; } }

        private void Start()
        {
            unusedParticles = new List<Particle>((int)(emission * particlesLifeTime) / 2);
            if (playOnStart)
                StartWork();
        }

        private bool isEmiting;
        private void Update()
        {
            if(isWorking)
            {
                if(!isEmiting)
                {
                    isEmiting = true;
                    StartCoroutine(Emiting());
                }
            }
        }

        public void StartWork()
        {
            isWorking = true;
        }

        public void StopWork()
        {
            isWorking = false;
            StopAllCoroutines();
            isEmiting = false;
        }

        private IEnumerator Emiting()
        {
            for(int i = 0; i < emission;i++)
            {
                if (isWorking)
                    CreateParticle();
                yield return new WaitForSecondsRealtime(1f / emission);
            }
            isEmiting = false;
        }

        private void CreateParticle()
        {
            if(unusedParticles.Count == 0)
            {
                GameObject newparticle = Instantiate(particle);
                Particle particleComp = newparticle.GetComponent<Particle>();
                particleComp.ParticleTransform.position = emissionPoint.position;
                if(forceType == OrientationType.Local)
                {
                    particleComp.ParticleTransform.rotation = Quaternion.Euler(0, 0, emissionPoint.rotation.eulerAngles.z + angle + Random.Range(-emissionAngleRange / 2, emissionAngleRange / 2));
                }
                else
                {
                    particleComp.ParticleTransform.rotation = Quaternion.Euler(0, 0, angle + Random.Range(-emissionAngleRange / 2, emissionAngleRange / 2));
                }                
                particleComp.LifeTime = particlesLifeTime;
                particleComp.myParticleSystem = this;
                if (detectCollisionEnter)
                    particleComp.AddCollisionEnterListener(OnParticleCollisionEnter);
                if (detectCollisionExit)
                    particleComp.AddCollisionExitListener(OnParticleCollisionExit);
                if (detectTriggerEnter)
                    particleComp.AddTriggerEnterListener(OnParticleTriggerEnter);
                if (detectTriggerExit)
                    particleComp.AddTriggerExitListener(OnParticleTriggerExit);
                particleComp.AddLifeFinishedListener(OnParticleLifeFinished);
                particleComp.ParticleRigidbody.AddForce(particleComp.ParticleTransform.up * force, ForceMode2D.Impulse);
                particleComp.StartLife();
                /*activeParticles[firstEmptyIndex] = newparticle;
                firstEmptyIndex++;*/
            }
            else
            {
                unusedParticles[unusedParticles.Count - 1].ParticleTransform.position = emissionPoint.position;
                if (forceType == OrientationType.Local)
                {
                    unusedParticles[unusedParticles.Count - 1].ParticleTransform.rotation = Quaternion.Euler(0, 0, emissionPoint.rotation.eulerAngles.z + angle + Random.Range(-emissionAngleRange / 2, emissionAngleRange / 2));
                }
                else
                {
                    unusedParticles[unusedParticles.Count - 1].ParticleTransform.rotation = Quaternion.Euler(0, 0, angle + Random.Range(-emissionAngleRange / 2, emissionAngleRange / 2));
                }
                unusedParticles[unusedParticles.Count - 1].LifeTime = particlesLifeTime;
                unusedParticles[unusedParticles.Count - 1].StartLife();
                unusedParticles[unusedParticles.Count - 1].ParticleRigidbody.velocity = Vector2.zero;
                unusedParticles[unusedParticles.Count - 1].ParticleRigidbody.AddForce(unusedParticles[unusedParticles.Count - 1].ParticleTransform.up * force, ForceMode2D.Impulse);
                unusedParticles.RemoveAt(unusedParticles.Count - 1);
            }
        }

        private void OnParticleLifeFinished(Particle particle)
        {
            unusedParticles.Add(particle);
        }

        private void OnDestroy()
        {
            for(int i = 0; i < unusedParticles.Count;i++)
            {
                if (unusedParticles[i] != null)
                    Destroy(unusedParticles[i]);
            }
        }

    }
}
