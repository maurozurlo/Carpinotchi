using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;

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

		public float timeToStart = 3;

		public Vector2 repeatingTime;

		public GameObject spawnSpot;

		[Header("Score")]
		public float score;
		public TextMeshPro scoreCounter;
		public int lives = 3;
		public int baggedItems = 0;
		public int spawnedItems = 0;

		public AudioClip beep;
		public AudioClip[] itemFall;
		public AudioSource AS;

		public Vector2 itemSpeed = new Vector2(5, 15);
		public float currSpeed;

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

		private void Start()
		{
			StartCoroutine("SpawnItems");
			InvokeRepeating("IncreaseSpeed", timeToStart, 7);
		}

		void IncreaseSpeed()
		{
			float newSpeed = currSpeed * 1.1f;
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
			score -= moneyPenalty;
			lives--;
			UpdateVisuals();
			AS.PlayOneShot(itemFall[Random.Range(0, itemFall.Length)]);
		}


		IEnumerator SpawnItems()
		{
			if (gameState != GameState.playing) yield return null;
			yield return new WaitForSeconds(Random.Range(repeatingTime.x, repeatingTime.y));

			int num = Random.Range(0, items.Length);
			GameObject obj = items[num];
			GameObject item = Instantiate(obj, spawnSpot.transform.position, Quaternion.identity);
			item.GetComponent<Cashier_Item>().SetSpeed(currSpeed);
			item.transform.rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 180), 0));
			spawnedItems++;
			StartCoroutine("SpawnItems");
		}

		void UpdateVisuals()
		{ // or any other float value you want to format as currency
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
			scoreCounter.text = formattedNumber;
		}
	}
}
