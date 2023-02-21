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

		public Vector2 repeatingTime;

		public GameObject spawnSpot;

		[Header("Score")]
		public float score;
		public TextMeshPro scoreCounter;
		public int lives = 3;
		private int initialLives;
		public int baggedItems = 0;
		public int spawnedItems = 0;

		public AudioClip beep;
		public AudioClip[] itemFall;
		public AudioSource AS;

		public Vector2 itemSpeed = new Vector2(5, 15);
		public float currSpeed;
		public int level = 0;
		public float levelSpeedMultiplier = 1.1f;

		public float repeatingTimeMultiplier = .1f;
		public float currRepeatingTime;

		private float initialRepeatingTime, initialLevelSpeed;

		public int increaseLevelEvery = 7;


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

			initialLives = lives;
			initialRepeatingTime = repeatingTime.y;
			initialLevelSpeed = currSpeed;
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
			baggedItems = 0;
			score = 0;
			spawnedItems = 0;
			lives = initialLives;
			level = 0;
			currRepeatingTime = initialRepeatingTime;
			currSpeed = initialLevelSpeed;
			UpdateVisuals();
			StartCoroutine("ShowInitialMessage");

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
			InvokeRepeating("IncreaseSpeed", timeToStart, increaseLevelEvery);
			StartCoroutine("SpawnItems");
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

		void IncreaseSpeed()
		{
			level++;
			float newSpeed = Mathf.Pow(levelSpeedMultiplier, level) + currSpeed;
			currSpeed = Mathf.Min(newSpeed, itemSpeed.y);
		}

		public void CheckIfMatch()
		{
			foreach (Cashier_Box target in targets)
			{
				if (target.selected && target.type == selectedItemType)
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
			gameState = GameState.ended;
			UnSelectItem();
			GameOverScreen.SetActive(true);
			gameOverText.text = $"Felicidades, lograste hacer {score:C}";
		}


		IEnumerator SpawnItems()
		{
			if (gameState != GameState.playing) yield return null;
			if (currRepeatingTime > 0)
			{
				float minus = Mathf.Pow(repeatingTimeMultiplier, level) - 1;
				currRepeatingTime -= minus;
			}
			float spawnTime = Mathf.Max(currRepeatingTime, repeatingTime.x);

			yield return new WaitForSeconds(spawnTime);
			int num = Random.Range(0, items.Length);
			GameObject obj = items[num];
			GameObject item = Instantiate(obj, spawnSpot.transform.position, Quaternion.identity);
			item.GetComponent<Cashier_Item>().SetSpeed(currSpeed);
			item.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 180), 0));
			spawnedItems++;
			StartCoroutine("SpawnItems");
		}

		string Money() {
			string formattedNumber = score.ToString("C2"); // "C2" specifies currency format with two decimal places

			// Remove the currency symbol from the formatted string
			formattedNumber = formattedNumber.Replace(CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, string.Empty);

			// Add leading zeros if the number is less than 100
			if (score < 100)
			{
				formattedNumber = formattedNumber.PadLeft(6, '0'); // 6 is the total length of the formatted string, including the decimal point
			}

			// Add the currency symbol back to the formatted string
			formattedNumber = "$" + formattedNumber;
			return formattedNumber;
		}

		void UpdateVisuals()
		{ // or any other float value you want to format as currency
			string formattedNumber = Money();
			scoreCounter.text = formattedNumber;

			ringed.text = baggedItems == 0 ? "-" : baggedItems.ToString();
			money.text = score == 0 ? "-" : "$" + Mathf.CeilToInt(score).ToString();
			broken.text = lives.ToString();
		}
	}
}
