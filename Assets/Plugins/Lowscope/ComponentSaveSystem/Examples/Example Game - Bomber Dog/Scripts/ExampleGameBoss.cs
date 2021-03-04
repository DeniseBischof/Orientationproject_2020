using Lowscope.Saving;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGameBoss : MonoBehaviour, ExampleGameIDamageable, ExampleGameIBlockable, ISaveable
    {
        [System.Serializable]
        public struct SaveData
        {
            public State activeState;
            public State targetState;
            public float totalTransitionTime;
            public float activeTransitionTime;
            public int currentHealth;
            public Vector3 fromPosition;
            public Vector3 randomFlyOffset;
            public Vector3 targetPosition;
            public bool isParticleAuraActive;
            public bool isParticleDizzyActive;
            public bool gatePlatformHidden;
            public Vector3 position;
            public Quaternion rotation;
            public Quaternion localHeadRotation;
            public Quaternion localBodyRotation;
        }

        public enum State
        {
            None,
            Idle,
            Dizzy,
            Preparing,
            Attacking,
            Dead
        }

        [SerializeField] private Transform gatePlatform;

        [SerializeField] private Transform body;
        [SerializeField] private Transform head;

        [SerializeField] private Transform[] flyPoints;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private LayerMask playerLayer;

        [SerializeField] private ParticleSystem particleSystenAura;
        [SerializeField] private ParticleSystem particleSystemDizzy;

        [SerializeField] private int totalHealth;

        [SerializeField] private GameObject gameBeatMessageScreen;

        private int currentHealth;

        private State activeState = State.None;
        private State targetState = State.None;

        private Vector3 fromPosition;
        private Vector3 randomFlyOffset;
        private Vector3 targetPosition;

        private float totalTransitionTime;
        private float activeTransitionTime;

        private bool gatePlatformHidden;

        private GameObject player;

        private bool loadedGame = false;

        private void Awake()
        {
            currentHealth = totalHealth;
            player = GameObject.FindGameObjectWithTag("Player");
        }

        private void Start()
        {
            // The save system retrieves the data during the Awake() step
            // This means you can check if the game has been saved during Start()
            if (!loadedGame)
            {
                SetState(State.Idle);
                StartCoroutine(MoveGatePlatformAway());
            }
        }

        private IEnumerator MoveGatePlatformAway()
        {
            float t = 0;
            Vector3 from = gatePlatform.transform.position;
            Vector3 to = from + new Vector3(0, 0, 9);

            var particles = gatePlatform.gameObject.GetComponentsInChildren<ParticleSystem>();
            foreach (var item in particles)
            {
                item.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(1f);

            while (t < 1)
            {
                yield return null;
                t += Time.deltaTime;
                gatePlatform.transform.position = Vector3.Lerp(from, to, t);
            }

            gatePlatform.gameObject.SetActive(false);
            gatePlatformHidden = true;
        }

        private void SetState(State state)
        {
            activeState = state;
            activeTransitionTime = 0;

            switch (state)
            {
                case State.Idle:

                    particleSystemDizzy.Stop();
                    particleSystenAura.Play();

                    targetState = State.Preparing;

                    // Takes from 7 to 15 seconds before starting to prepare an attack
                    totalTransitionTime = Random.Range(7, 16);

                    targetPosition = flyPoints[Random.Range(0, flyPoints.Length)].position;

                    fromPosition = this.transform.position;

                    break;
                case State.Dizzy:

                    targetState = State.Idle;

                    transform.rotation = Quaternion.Euler(0, -180f, 0);

                    // If we hit the player, we skip this state
                    foreach (var item in Physics.OverlapSphere(transform.position, 1, playerLayer))
                    {
                        if (item.CompareTag("Player"))
                        {
                            SetState(State.Idle);
                            return;
                        }
                    }

                    // Adjust again for the rotation offset
                    totalTransitionTime = 10;

                    particleSystemDizzy.Play();
                    particleSystenAura.Stop();

                    break;
                case State.Preparing:

                    targetState = State.Attacking;
                    // Takes from 7 to 15 seconds before starting to prepare an attack
                    totalTransitionTime = Random.Range(3, 5);
                    fromPosition = transform.position;

                    break;
                case State.Attacking:

                    targetState = State.Dizzy;

                    fromPosition = this.transform.position;
                    fromPosition.z = player.transform.position.z;

                    RaycastHit hitinfo;

                    if (Physics.Raycast(fromPosition, Vector3.down, out hitinfo, 10, obstacleLayer))
                    {
                        targetPosition = hitinfo.point;
                        targetPosition.z = fromPosition.z;
                    }

                    totalTransitionTime = 0.65f;

                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(129.9f, -180f, 0), Time.deltaTime * 5);

                    break;
                case State.None:
                    break;
                case State.Dead:

                    gameBeatMessageScreen.gameObject.SetActive(true);
                    this.gameObject.SetActive(false);

                    break;
                default:
                    break;
            }
        }

        private void Update()
        {
            float t = activeTransitionTime / totalTransitionTime;

            switch (activeState)
            {
                case State.Idle:

                    LookAt(player.transform.position, new Vector3(0, 0, -1));
                    body.transform.localPosition = new Vector3(0, Mathf.Sin(Time.time) * Mathf.InverseLerp(5, totalTransitionTime, activeTransitionTime), 0);
                    fromPosition = Vector3.Lerp(fromPosition, targetPosition, Time.deltaTime);

                    randomFlyOffset = Mathf.InverseLerp(3, 10, activeTransitionTime) * new Vector3(Mathf.Sin(Time.time * 1.4f)
                        , Mathf.Sin(Time.time * 1.2f)
                        , Mathf.Sin(Time.time * 1.7f));

                    transform.position = fromPosition + randomFlyOffset;

                    break;
                case State.Dizzy:

                    Quaternion bodyRotation = Quaternion.Euler(Mathf.Sin(Time.time * 1.2f) * 20, Mathf.Sin(Time.time * 1.2f) * 20, Mathf.Sin(Time.time * 1.3f) * 20);

                    float startupTime = Mathf.InverseLerp(0, totalTransitionTime, activeTransitionTime);

                    head.transform.localRotation = Quaternion.Lerp(head.localRotation, Quaternion.Lerp(bodyRotation, Quaternion.identity, t), startupTime);
                    body.transform.localRotation = Quaternion.Lerp(body.localRotation, Quaternion.Lerp(bodyRotation, Quaternion.identity, t), startupTime);

                    break;
                case State.Preparing:

                    LookAt(player.transform.position, new Vector3(0, 0, -3));
                    body.transform.localPosition = Vector3.Lerp(body.transform.localPosition, Vector3.zero, Time.deltaTime);
                    Vector3 newPosition = Vector3.Lerp(fromPosition, player.transform.position + new Vector3(0, 5f, 0), ExampleGameEasings.QuadraticInOut(t));
                    newPosition += (Random.insideUnitSphere * 0.03f) * t; // Make it shake
                    transform.position = newPosition;

                    break;
                case State.Attacking:

                    LookAt(this.transform.position, new Vector3(0, -3, -3), 5);
                    transform.position = Vector3.Lerp(fromPosition, targetPosition, ExampleGameEasings.EaseOutBounce(t));

                    break;
                default:
                    break;
            }

            if (activeTransitionTime < totalTransitionTime)
            {
                activeTransitionTime += Time.deltaTime;
            }
            else
            {
                SetState(targetState);
            }
        }

        private void LookAt(Vector3 target, Vector3 offset, float speed = 1)
        {
            Vector3 toPlayerVector = ((target + offset) - transform.position).normalized;
            head.transform.rotation = Quaternion.Lerp(head.transform.rotation, Quaternion.LookRotation(toPlayerVector, Vector3.up), Time.deltaTime * speed);
        }

        public void Damage(int amount)
        {
            // Only damageable when dizzy
            if (activeState != State.Dizzy)
                return;

            if (currentHealth > 0)
            {
                currentHealth -= amount;
                StartCoroutine(PunchScale(0.25f, new Vector3(1.2f, 1.4f, 1.2f)));
            }
            else
            {
                SetState(State.Dead);
            }
        }

        IEnumerator PunchScale(float duration, Vector3 intensity)
        {
            float t = 0;

            while (t < duration)
            {
                yield return null;
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(intensity, Vector3.one, ExampleGameEasings.EaseOutBounce(t / duration));
            }

            transform.localScale = Vector3.one;
        }

        public bool BlockPlayer()
        {
            return false;
        }

        public string OnSave()
        {
            return JsonUtility.ToJson(new SaveData()
            {
                activeState = this.activeState,
                targetState = this.targetState,
                totalTransitionTime = this.totalTransitionTime,
                activeTransitionTime = this.activeTransitionTime,
                currentHealth = this.currentHealth,
                fromPosition = this.fromPosition,
                randomFlyOffset = this.randomFlyOffset,
                targetPosition = this.targetPosition,
                isParticleAuraActive = particleSystenAura.gameObject.activeSelf,
                isParticleDizzyActive = particleSystemDizzy.gameObject.activeSelf,
                gatePlatformHidden = this.gatePlatformHidden,
                localBodyRotation = body.transform.localRotation,
                localHeadRotation = head.transform.localRotation,
                position = transform.position,
                rotation = transform.rotation
            });
        }

        public void OnLoad(string data)
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(data);

            activeState = saveData.activeState;
            currentHealth = saveData.currentHealth;
            fromPosition = saveData.fromPosition;
            randomFlyOffset = saveData.randomFlyOffset;
            targetPosition = saveData.targetPosition;
            particleSystenAura.gameObject.SetActive(saveData.isParticleAuraActive);
            particleSystemDizzy.gameObject.SetActive(saveData.isParticleDizzyActive);
            gatePlatform.gameObject.SetActive(!saveData.gatePlatformHidden);
            gatePlatformHidden = saveData.gatePlatformHidden;
            body.transform.localRotation = saveData.localBodyRotation;
            head.transform.localRotation = saveData.localHeadRotation;
            transform.rotation = saveData.rotation;
            transform.position = saveData.position;

            // The save system retrieves the data during the Awake() step
            // This means you can check if the game has been saved during Start()
            loadedGame = true;

            SetState(activeState);
            targetState = saveData.targetState;
            totalTransitionTime = saveData.totalTransitionTime;
            activeTransitionTime = saveData.activeTransitionTime;
        }

        public bool OnSaveCondition()
        {
            return true;
        }
    }
}