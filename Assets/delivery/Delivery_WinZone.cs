using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_WinZone : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player")) return;
		Delivery_Manager.control.GameOver();
	}
}
