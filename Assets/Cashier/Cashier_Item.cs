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

		public GameObject cube;

		BoxCollider boxCollider;

		private void Start()
		{
			boxCollider = GetComponent<BoxCollider>();
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
				Destroy(gameObject);
			}
		}
		void OnMouseDown()
		{
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
			boxCollider.enabled = true;
			isSelected = false;
			Cashier_Manager.control.CheckIfMatch();
		}
	}

}

