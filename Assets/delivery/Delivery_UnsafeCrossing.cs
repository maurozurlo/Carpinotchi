using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_UnsafeCrossing : MonoBehaviour
{
    public Delivery_Streetlight streetlight;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player")) return;
		streetlight.UnsafeCrossing();
	}
}
