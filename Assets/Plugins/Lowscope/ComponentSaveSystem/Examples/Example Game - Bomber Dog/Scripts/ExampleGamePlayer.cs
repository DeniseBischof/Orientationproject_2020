using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable]
public class UnityEventInt : UnityEvent<int> { }

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGamePlayer : MonoBehaviour, ExampleGameIDamageable, ISaveable
    {
        [System.Serializable]
        public struct SaveData
        {
            public int bombCount;
            public int gemCount;
            public int health;
            public bool isMoving;
            public float activeMoveDirection;
            public Vector3 oldPosition;
            public Vector3 newPosition;
            public float walkTime;
            public float jumpRandomization;

            public bool transitioningLevel;
            public float transitionLevelTime;
            public string transitionLevelName;
            public Vector3 transitionLevelPosition;
        }

        [SerializeField] private bool lockMovement = false;

        [SerializeField] private CharacterController controller;
        [SerializeField] private Transform visual;
        [SerializeField] private Transform visualHead;

        [SerializeField] private int maxBombCount = 10;
        [SerializeField] private int maxHealthCount = 5;
        [SerializeField] private float stepLength = 1;

        [SerializeField] private float speed;
        [SerializeField] private float jumpWobbleFrequency;
        [SerializeField] private float lookAtSpeed = 6;

        [SerializeField] private LayerMask objectsThatCanBlock;
        [SerializeField] private LayerMask objectsYouInteractWith;

        [SerializeField] private UnityEventInt OnHealthChanged;
        [SerializeField] private UnityEventInt OnBombCountChanged;
        [SerializeField] private UnityEventInt OnGemCountChanged;
        [SerializeField] private UnityEvent OnDamaged;
        [SerializeField] private UnityEvent OnDeath;
        [SerializeField] private UnityEvent OnExitLevel;
        [SerializeField] private UnityEvent OnEnterLevel;

        [SerializeField] private AudioSource audioSource;

        [SerializeField] private AudioClip jumpThud;
        [SerializeField] private AudioClip placeBomb;

        [SerializeField] private string bombPrefabResourceName;

        // In case project uses custom gravity.
        private const float gravity = -9.81f;
        private const float halfPI = 1.57079632679f;

        private float jumpRandomization = 1;
        private float moveTime = 0;
        private int bombCount = 0;
        private int gemCount = 0;
        private int health = 5;
        private bool isMoving = false;

        private bool transitioningLevel = false;
        private float transitionLevelTime = 0;
        private string transitionLevelname;
        private float activeMoveDirection;

        private Vector3 oldPosition;
        private Vector3 newPosition;
        private Vector3 transitionLevelPosition;
        private bool queuedBombDrop;
        private bool queuedMoveUp;

        public bool AdjustBombs(int amount)
        {
            if (bombCount + amount > maxBombCount)
                return false;

            if (bombCount + amount < 0)
                return false;

            bombCount += amount;

            OnBombCountChanged.Invoke(bombCount);

            return true;
        }

        public bool AdjustGems(int amount)
        {
            if (gemCount + amount < 0)
                return false;

            gemCount += amount;

            OnGemCountChanged.Invoke(gemCount);

            return true;
        }

        public bool AdjustHealth(int amount)
        {
            health = Mathf.Clamp(health + amount, 0, maxHealthCount);
            if (health == 0)
            {
                OnDeath.Invoke();
            }

            OnHealthChanged.Invoke(health);

            if (amount < 0)
            {
                OnDamaged.Invoke();
            }

            return true;
        }

        private void Awake()
        {
            oldPosition = this.transform.position;
            newPosition = this.transform.position;

            AdjustHealth(0);
            AdjustBombs(0);
            AdjustGems(0);

            Application.targetFrameRate = 144;
        }

        private IEnumerator ShakeObject(Transform transform)
        {
            float t = 0;
            float duration = UnityEngine.Random.Range(0.25f, 0.4f);

            while (t < duration)
            {
                yield return null;
                transform.localRotation = Quaternion.Euler(Vector3.one * (Mathf.Sin(t - (duration * 0.5f)) * 20) * (1 - (t / duration)));
                t += Time.deltaTime;
            }
        }

        private void Update()
        {
            // This has to be done in case the player saves/leaves the game when
            // transitioning between levels. However, normally in these kind of games you
            // would just opt-in for having save points, instead of always saving.
            // But this is how you could do it.
            if (transitioningLevel)
            {
                if (transitionLevelTime > 0)
                {
                    transitionLevelTime -= Time.deltaTime;
                }
                else
                {
                    if (gameObject.scene.name != transitionLevelname)
                    {
                        SceneManager.LoadScene(transitionLevelname);
                        return;
                    }

                    newPosition = transitionLevelPosition;
                    oldPosition = transitionLevelPosition + new Vector3(0, 0, stepLength);
                    moveTime = 0;
                    isMoving = true;
                    transitioningLevel = false;

                    OnEnterLevel.Invoke();
                    // Transition to scene.
                }
            }

            // Player movement
            float moveLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
            float moveRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0;
            float horizontalMovement = !transitioningLevel ? moveLeft + moveRight : 0;

            if (lockMovement)
                horizontalMovement = 0;

            if (!isMoving)
            {
                if (horizontalMovement != 0)
                {
                    Vector3 origin = transform.position + new Vector3(0, 0.5f, 0);
                    Vector3 direction = new Vector3(horizontalMovement, 0, 0);

                    RaycastHit raycastHit;

                    bool foundBlockingObject = Physics.Raycast(origin, direction, out raycastHit, stepLength, objectsThatCanBlock, QueryTriggerInteraction.Ignore);

                    if (foundBlockingObject)
                    {
                        ExampleGameIBlockable getInterface = raycastHit.collider.GetComponent<ExampleGameIBlockable>();

                        if (getInterface != null)
                        {
                            foundBlockingObject = getInterface.BlockPlayer();
                        }
                    }

                    if (!foundBlockingObject)
                    {
                        activeMoveDirection = horizontalMovement;
                        oldPosition = this.transform.position;
                        newPosition = this.transform.position + new Vector3(horizontalMovement * stepLength, 0, 0);
                        isMoving = true;
                    }

                    BounceTile();
                }
            }
            else
            {
                queuedBombDrop = isMoving && !transitioningLevel && Input.GetKey(KeyCode.Space);
                queuedMoveUp = isMoving && !transitioningLevel && Input.GetKey(KeyCode.UpArrow);

                moveTime = Mathf.Clamp(moveTime + (Time.deltaTime * speed), 0, 1);
                controller.transform.position = Vector3.Lerp(oldPosition, newPosition, moveTime);

                if (moveTime == 1)
                {
                    activeMoveDirection = 0;
                    jumpRandomization = UnityEngine.Random.Range(10, 20) * (jumpRandomization > 0 ? -1 : 1);

                    audioSource.pitch = UnityEngine.Random.Range(0.98f, 1.02f);
                    audioSource.PlayOneShot(jumpThud);

                    if (BounceTile())
                    {
                        isMoving = false;
                        moveTime = 0;
                        oldPosition = transform.position;
                        newPosition = transform.position;
                    }
                    else
                    {
                        // Fall further down if there is no tile below the player to bounce.

                        moveTime = 0;
                        oldPosition = transform.position;

                        RaycastHit hitInfo;

                        if (Physics.Raycast(transform.position, Vector3.down * 10, out hitInfo, 10, objectsThatCanBlock))
                        {
                            newPosition = hitInfo.point;
                        }
                    }
                }
            }

            // Look left or right when moving
            Quaternion targetRotation = Quaternion.Euler(0, Mathf.Lerp(240, 120, Mathf.InverseLerp(-1, 1, activeMoveDirection)), 0);
            this.transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * lookAtSpeed);

            float jumpEffect = Mathf.Abs(Mathf.Sin(((moveTime * jumpWobbleFrequency) * Mathf.PI)));
            Vector3 visualPosition = new Vector3(0, jumpEffect, 0);
            visual.transform.localPosition = visualPosition;

            // Make the head and body bobble a bit
            visualHead.transform.localRotation = Quaternion.Euler(Mathf.Cos(Time.time * 1.7f), Mathf.Cos(Time.time * 1.3f), Mathf.Cos(Time.time * 1.4f));
            visual.transform.localRotation = Quaternion.Euler(Mathf.Cos(Time.time * 1.2f) + (jumpEffect * jumpRandomization), Mathf.Cos(Time.time * 1.5f), Mathf.Cos(Time.time * 1.8f));

            // None of the actions below should be executed if the player is moving between tiles.
            if (!(isMoving || transitioningLevel))
            {
                //Input checking for actions, Using GetKey so the user can keep holding space to keep placing bombs
                if (Input.GetKeyDown(KeyCode.Space) || queuedBombDrop)
                {
                    // Example of spawning a prefab using a custom spawner. Instead of using Resources as a source.
                    // If you want to know where this "ExampleCustomPrefabSpawner" is being initialzed, look for it in the scene.
                    if (bombCount > 0)
                    {
                        var spawnBomb = SaveMaster.SpawnSavedPrefab(InstanceSource.Custom, "ExplodingBomb", "ExampleCustomPrefabSpawner");
                        spawnBomb.transform.position = this.transform.position + new Vector3(0, 0.5f, 0);
                        AdjustBombs(-1);
                        audioSource.PlayOneShot(placeBomb);
                    }

                    queuedBombDrop = false;
                }

                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || queuedMoveUp)
                {
                    foreach (var item in Physics.OverlapSphere(transform.position, 0.5f, objectsYouInteractWith))
                    {
                        var interactable = item.GetComponent<ExampleGameIInteractable>();
                        if (interactable != null)
                        {
                            interactable.OnInteract(this.gameObject);
                        }
                    }

                    queuedMoveUp = false;
                }
            }
        }

        private bool BounceTile()
        {
            var origin = transform.position + new Vector3(0, 0.25f, 0);
            var direction = new Vector3(0, -0.5f, 0);

            RaycastHit hitInfo;
            if (Physics.Raycast(origin, direction, out hitInfo, 1f, objectsThatCanBlock))
            {
                StartCoroutine(ShakeObject(hitInfo.transform));
                return true;
            }

            return false;
        }

        public void Damage(int amount)
        {
            AdjustHealth(-amount);
        }

        public void MoveToLevel(string targetLevel, Vector3 position)
        {
            isMoving = true;
            moveTime = 0;
            oldPosition = this.transform.position;
            newPosition = this.transform.position + new Vector3(0, 0, stepLength);

            // Storing the transition level data
            transitioningLevel = true;
            transitionLevelTime = 0.5f;
            transitionLevelname = targetLevel;
            transitionLevelPosition = position;

            OnExitLevel.Invoke();
        }

        public string OnSave()
        {
            return JsonUtility.ToJson(new SaveData()
            {
                activeMoveDirection = this.activeMoveDirection,
                bombCount = this.bombCount,
                gemCount = this.gemCount,
                health = this.health,
                isMoving = this.isMoving,
                jumpRandomization = this.jumpRandomization,
                newPosition = this.newPosition,
                oldPosition = this.oldPosition,
                walkTime = this.moveTime,
                transitioningLevel = this.transitioningLevel,
                transitionLevelName = this.transitionLevelname,
                transitionLevelTime = this.transitionLevelTime,
                transitionLevelPosition = this.transitionLevelPosition
            });
        }

        public void OnLoad(string data)
        {
            this.enabled = false;

            SaveData saveData = JsonUtility.FromJson<SaveData>(data);

            newPosition = saveData.newPosition;
            oldPosition = saveData.oldPosition;
            moveTime = saveData.walkTime;
            activeMoveDirection = saveData.activeMoveDirection;
            bombCount = saveData.bombCount;
            gemCount = saveData.gemCount;
            health = saveData.health;
            isMoving = saveData.isMoving;
            jumpRandomization = saveData.jumpRandomization;

            transitioningLevel = saveData.transitioningLevel;
            transitionLevelname = saveData.transitionLevelName;
            transitionLevelTime = saveData.transitionLevelTime;
            transitionLevelPosition = saveData.transitionLevelPosition;

            if (!isMoving)
            {
                this.transform.position = newPosition;
            }

            AdjustHealth(0);
            AdjustBombs(0);
            AdjustGems(0);

            this.enabled = true;
        }

        public bool OnSaveCondition()
        {
            return true;
        }
    }
}