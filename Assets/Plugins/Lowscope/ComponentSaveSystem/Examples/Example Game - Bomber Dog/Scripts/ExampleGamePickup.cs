using System.Collections;
using UnityEngine;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGamePickup : MonoBehaviour, ISaveable
    {
        private enum PickupTypes
        {
            Bomb,
            Heart,
            Gem
        }

        [System.Serializable]
        public struct SaveData
        {
            public bool pickedUp;
            public float activeRespawnTime;
        }

        [SerializeField] private PickupTypes pickupType;
        [SerializeField] private bool respawn = false;
        [SerializeField] private Vector2 respawnTime = new Vector2(10, 20);
        [SerializeField] private GameObject visual;
        [SerializeField] private Collider aCollider = null;
        [SerializeField] private ExampleGameObjectPulser pulser;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip audioPickup;

        private bool pickedUp;
        private float activeRespawnTime = 0;
        private const float halfPI = 1.57079632679f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "Player" && !pickedUp)
            {
                var player = other.GetComponent<ExampleGamePlayer>();

                bool canBePickedUp = false;

                audioSource.PlayOneShot(audioPickup);

                switch (pickupType)
                {
                    case PickupTypes.Bomb:
                        canBePickedUp = player.AdjustBombs(1);
                        break;
                    case PickupTypes.Heart:
                        canBePickedUp = player.AdjustHealth(1);
                        break;
                    case PickupTypes.Gem: canBePickedUp = player.AdjustGems(1); break;
                    default:
                        break;
                }

                if (!canBePickedUp)
                    return;

                StartCoroutine(ChangeScale(1, 0, () => SetActive(false)));

                pickedUp = true;

                if (respawn)
                {
                    StartCoroutine(RespawnTimer());
                }
            }
        }

        private IEnumerator ChangeScale(float from, float to, System.Action exec)
        {
            float t = 0;
            float totalT = 0.2f;
            Vector3 fromScale = new Vector3(from, from, from);
            Vector3 toScale = new Vector3(to, to, to);

            while (t <= totalT)
            {
                yield return null;
                this.transform.localScale = Vector3.Lerp(fromScale, toScale, ExampleGameEasings.QuadraticEaseOut(t / totalT));
                t += Time.deltaTime;
            }

            exec.Invoke();
        }

        private void SetActive(bool active)
        {
            visual.SetActive(active);
            aCollider.enabled = active;
            pulser.enabled = active;
        }

        private IEnumerator RespawnTimer()
        {
            if (activeRespawnTime <= 0)
                activeRespawnTime = Random.Range(respawnTime.x, respawnTime.y);

            while (activeRespawnTime > 0)
            {
                yield return null;
                activeRespawnTime -= Time.deltaTime;
            }

            SetActive(true);
            StartCoroutine(ChangeScale(0, 1, () => pickedUp = false));
        }

        // Similar to saving when visibility state.
        // Altough the user may restart the game when the animation is playing

        public string OnSave()
        {
            return JsonUtility.ToJson(new SaveData()
            {
                pickedUp = this.pickedUp,
                activeRespawnTime = this.activeRespawnTime
            });
        }

        public void OnLoad(string data)
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(data);

            this.activeRespawnTime = saveData.activeRespawnTime;
            this.pickedUp = saveData.pickedUp;

            if (respawn && this.pickedUp)
            {
                StartCoroutine(RespawnTimer());
            }

            SetActive(!this.pickedUp);
        }

        public bool OnSaveCondition()
        {
            // Only save when object has been picked up, or if it can respawn
            return pickedUp || respawn;
        }
    }
}