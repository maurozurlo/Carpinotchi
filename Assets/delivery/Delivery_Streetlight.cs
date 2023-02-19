using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delivery_Streetlight : MonoBehaviour
{
	public GameObject UI_base;
	public GameObject UI_Green, UI_Yellow, UI_Red;
	enum color
	{
		red,
		yellow,
		green,
		_
	}

	public Vector3 distanceToNextCrossing;
	public int currentStreet = 0;

	public Vector2 waitingTime = new Vector2(3, 5);
	float currWaitingTime;

	Delivery_CarSpawner carSpawner;

	public Delivery_UnsafeCrossing UnsafeXing;

	Vector3[] originalPos = { Vector3.zero, Vector3.zero };

	color nextColor;

	void Start()
	{
		carSpawner = GetComponent<Delivery_CarSpawner>();
		currentStreet = 0;

		originalPos[0] = transform.position;
		originalPos[1] = UnsafeXing.transform.position;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player")) return;
		currWaitingTime = Random.Range(waitingTime.x, waitingTime.y);
		StartCoroutine(SetupStreetlight(color.red));
	}

	public void UnsafeCrossing(){
		StopAllCoroutines();		
		Cleanup();
		MoveLanes();
		currentStreet++;
	}

	IEnumerator SetupStreetlight(color color)
	{
		// Start carSpawner
		switch (color)
		{
			case color.red:
				
				nextColor = color.yellow;
				carSpawner.StartSpawning();
				UI_base.SetActive(true);
				UI_Red.SetActive(true);
				break;
			case color.yellow:
				nextColor = color.green;
				UI_Red.SetActive(false);
				UI_Yellow.SetActive(true);
				break;
			case color.green:
				nextColor = color._;
				UI_Yellow.SetActive(false);
				UI_Green.SetActive(true);
				currentStreet++;
				MoveLanes();
				break;
		}
		yield return new WaitForSeconds(currWaitingTime);

		if (nextColor != color._)
		{
			StartCoroutine(SetupStreetlight(nextColor));
		}else if (nextColor == color._){
			Cleanup();
		}
	}

	void MoveLanes()
	{
		transform.position += distanceToNextCrossing;
		UnsafeXing.transform.position += distanceToNextCrossing;
		carSpawner.MoveLanes(distanceToNextCrossing);
	}

	public void Cleanup()
	{				
		carSpawner.StopSpawning();
		UI_base.SetActive(false);
		UI_Green.SetActive(false);
		UI_Red.SetActive(false);
		UI_Yellow.SetActive(false);
	}

	public void ResetPositions(){
		transform.position = originalPos[0];
		UnsafeXing.transform.position = originalPos[1];
		carSpawner.ResetPositions();
	}
}
