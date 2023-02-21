using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cashier
{
	public class Cashier_Item : MonoBehaviour
	{
		public enum State
		{
			spawned,
			rolling,
			picked
		}

		public ItemType type;

		public State state;
		public Vector3 speed;

		private Vector3 screenPoint;
		private Vector3 offset;

		bool isSelected;

		BoxCollider boxCollider;

		float price = 5;
		bool isDisabled;

		private void Start()
		{
			boxCollider = GetComponent<BoxCollider>();
			// DEBUG
			price = Random.Range(1, 15);
		}

		public float GetPrice()
		{
			return price;
		}

		public void SetSpeed(float newSpeed)
		{
			speed = new Vector3(0, 0, newSpeed);
		}

		// Update is called once per frame
		void FixedUpdate()
		{
			if (state == State.rolling && !isSelected)
			{
				transform.Translate(speed.x * Time.deltaTime, speed.y * Time.deltaTime, speed.z * Time.deltaTime, Space.World);
			}


			if (transform.position.y < -10)
			{
				KillObject();
			}
		}

		void KillObject() {
			if (isDisabled) return;
			isDisabled = true;
			Cashier_Manager.control.LostItem(price);
			Destroy(gameObject, 2);
		}

		void OnMouseDown()
		{
			if (isDisabled) return;
			if (Cashier_Manager.control.gameState != Cashier_Manager.GameState.playing) return;
			isSelected = true;
			screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
			offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

			Cashier_Manager.control.SetSelectedItem(gameObject, type);
			boxCollider.enabled = false;
		}

		void OnMouseDrag()
		{
			Vector3 cursorScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
			Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorScreenPoint) + offset;
			transform.position = cursorPosition;
		}

		private void OnMouseUp()
		{
			if (isDisabled) return;
			boxCollider.enabled = true;
			isSelected = false;
			Cashier_Manager.control.CheckIfMatch();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("DeadZone")){
				KillObject();
			}
		}
	}

}

