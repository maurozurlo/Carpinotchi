using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;
using UnityEngine.SceneManagement;

namespace Cashier
{
    public class Cashier_Manager : MonoBehaviour
    {
        public static Cashier_Manager control;

        public GameObject[] _targets;
        public GameObject[] items = new GameObject[40];
        private GameObject selectedItem;
        public ItemType selectedItemType;

        List<Cashier_Box> targets = new List<Cashier_Box>();

        public enum GameState
        {
            start,
            playing,
            ended
        }

        public GameState gameState;

        public int timeToStart = 3;
        public GameObject spawnSpot;

        [Header("Score")]
        public float score;
        public TextMeshPro scoreCounter;
        
        private const int initialLives = 3;
        private int lives;
        private int baggedItems = 0;
        private int spawnedItems = 0;

        public AudioClip beep;
        public AudioClip[] itemFall;
        public AudioSource AS;

        private int batchCount = 0;
        private float gameTimer = 0f;
        private const float maxGameTime = 180f; // 3 minutes

        // UI
        public GameObject packagesUI;
        public GameObject moneyUI;
        public GameObject timeUI;

        public void SetSelectedItem(GameObject go, ItemType itemType)
        {
            selectedItem = go;
            selectedItemType = itemType;
        }

        public void UnSelectItem()
        {
            selectedItem = null;
            selectedItemType = ItemType._;
        }

        public GameObject GameOverScreen;

        public TextMeshProUGUI ringed, money, broken, gameOverText;
        public TextMeshProUGUI UI_Messager;

        private void Awake()
        {
            if (!control)
            {
                control = this;
            }

            foreach (GameObject target in _targets)
            {
                targets.Add(target.GetComponent<Cashier_Box>());
            }
        }

        public void RestartGame()
        {
            StartGame();
        }

        public void GoBackHome()
        {
            SceneManager.LoadScene("House_00");
        }

        private void Start()
        {
            StartGame();
        }

        private void StartGame()
        {
            GameOverScreen.SetActive(false);
            DisplayHideUI(true);
            baggedItems = 0;
            score = 0;
            spawnedItems = 0;
            lives = initialLives;
            batchCount = 0;
            gameTimer = 0f;
            UpdateVisuals();
            StartCoroutine(nameof(ShowInitialMessage));
        }

        private void Update()
        {
            if (gameState == GameState.playing)
            {
                gameTimer += Time.deltaTime;
                if (gameTimer >= maxGameTime)
                {
                    EndGame();
                }
            }
        }

        IEnumerator ShowInitialMessage()
        {
            for (int i = 0; i < timeToStart; i++)
            {
                ShowStartSign(timeToStart - i);
                yield return new WaitForSeconds(1);
            }
            HideSign();
            gameState = GameState.playing;
            StartCoroutine(nameof(SpawnBatches));
        }

        void PlayBeep(int seconds)
        {
            float scale = Mathf.Pow(2f, 1.0f / 12f);
            AS.pitch = Mathf.Pow(scale, seconds);
            AS.PlayOneShot(beep);
        }

        public void ShowStartSign(int seconds)
        {
            UI_Messager.text = $"El juego empieza en {seconds} segundos!\nMove los objetos a sus bolsitas!";
            PlayBeep(seconds);
        }

        public void HideSign()
        {
            UI_Messager.text = "";
        }

        [System.Serializable]
        public class ItemBatch
        {
            public int itemCount;
            public float spawnInterval;
            public float restTime;
        }

        public List<ItemBatch> easyBatches;
        public List<ItemBatch> mediumBatches;
        public List<ItemBatch> hardBatches;

        IEnumerator SpawnBatches()
        {
            while (gameState == GameState.playing)
            {
                ItemBatch batch = GetNextBatch();
                for (int i = 0; i < batch.itemCount; i++)
                {
                    SpawnItem();
                    yield return new WaitForSeconds(batch.spawnInterval);
                }
                batchCount++;
                yield return new WaitForSeconds(batch.restTime);
            }
        }

        ItemBatch GetNextBatch()
        {
            if (batchCount < 3)
            {
                return easyBatches[Random.Range(0, easyBatches.Count)];
            }

            float difficultyChance = Mathf.Clamp01(batchCount / 20f);
            float roll = Random.value;

            if (roll < 0.3f - difficultyChance * 0.2f) return easyBatches[Random.Range(0, easyBatches.Count)];
            if (roll < 0.7f - difficultyChance * 0.3f) return mediumBatches[Random.Range(0, mediumBatches.Count)];
            return hardBatches[Random.Range(0, hardBatches.Count)];
        }

        void SpawnItem()
        {
            int num = Random.Range(0, items.Length);
            GameObject obj = items[num];
            GameObject item = Instantiate(obj, spawnSpot.transform.position, Quaternion.identity);
            spawnedItems++;
        }

        void UpdateVisuals()
        {
            scoreCounter.text = "$" + Mathf.CeilToInt(score).ToString();
            ringed.text = baggedItems == 0 ? "-" : baggedItems.ToString();
            money.text = score == 0 ? "-" : "$" + Mathf.CeilToInt(score).ToString();
            broken.text = lives.ToString();
        }

        void EndGame()
        {
            gameState = GameState.ended;
            GameOverScreen.SetActive(true);
            UI_Messager.text = "¡Tiempo agotado!";
        }

        public void CheckIfMatch()
        {
            foreach (Cashier_Box target in targets)
            {
                if (target.selected && (target.type & selectedItemType) != 0)
                {
                    score += selectedItem.GetComponent<Cashier_Item>().GetPrice();
                    baggedItems++;
                    UpdateVisuals();
                    AS.PlayOneShot(beep);
                    Destroy(selectedItem);
                }
            }

            UnSelectItem();
        }

        public void LostItem(float moneyPenalty)
        {
            if (gameState != GameState.playing) return;
            if (baggedItems >= 1)
            {
                score -= moneyPenalty;
            }
            lives--;
            AS.PlayOneShot(itemFall[Random.Range(0, itemFall.Length)]);


            if (lives == -1 || score < 0)
            {
                GameOver();
            }
            else
            {
                UpdateVisuals();
            }
        }

        public void GameOver()
        {
            if (score < 0) score = 0;
            StopAllCoroutines();
            UpdateVisuals();
            DisplayHideUI(false);
            gameState = GameState.ended;
            UnSelectItem();
            GameOverScreen.SetActive(true);
            gameOverText.text = $"Felicidades, lograste hacer {score:C}";
        }

        void DisplayHideUI(bool newValue)
        {
            packagesUI.SetActive(newValue);
            moneyUI.SetActive(newValue);
            timeUI.SetActive(newValue);
        }
    }
}
