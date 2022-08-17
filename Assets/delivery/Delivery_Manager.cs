﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

public class Delivery_Manager : MonoBehaviour
{
    public enum GameState {
        Start,
        Playing,
        Ended
    }

    public GameState gameState = GameState.Start;

    public static Delivery_Manager control;
    public TextMeshProUGUI UI_Money;
    public TextMeshProUGUI UI_Time;
    public TextMeshProUGUI UI_PendingPackages;
    //Stay
    public TextMeshProUGUI UI_Stay;
    public AudioClip AC_Stay;
    // Pickup
    public AudioClip AC_Pickup;

    // Customers
    public GameObject[] customerPrefabs;
    public GameObject leftSidewalkGizmo;
    public Vector2 distance = new Vector2(50, 100);
    public float noise;

    //Audio
    AudioSource AS;

    // Time
    public float timeLeft;

    int packagesDelivered = 0;
    int moneyEarned = 0;
    int pendingPackages = 1;
    
    private void Awake() {
        control = this;
        AS = GetComponent<AudioSource>();
    }

    private void Update() {
        if(gameState == GameState.Playing) {
            timeLeft -= Time.deltaTime;
            UpdateTimeUI();
            if (timeLeft < 0) {
                GameOver();
            }
        }
    }

    void GameOver() {
        gameState = GameState.Ended;
    }

    public void AddToPackagesDelivered(int amount) {
        pendingPackages--;

        timeLeft += amount * 10;
        packagesDelivered += amount;
        StartCoroutine(HideSignAfterSeconds(AC_Pickup.length));

        int howManyToSpawn = Mathf.Clamp(Mathf.RoundToInt(packagesDelivered * .3f) >= 1 ? Mathf.RoundToInt(packagesDelivered * .3f) : 1, 0, 4);
        for (int i = 0; i < howManyToSpawn; i++) {
            SpawnCustomer();
        }
        UpdateUI();
        ShowPickupSign(amount);
    }

    void UpdateUI() {
        UI_Money.text = $"${moneyEarned}";
        UI_PendingPackages.text = $"{pendingPackages}";
    }

    void UpdateTimeUI() {
        String _time = FormatTimeSpan(TimeSpan.FromSeconds(timeLeft));
        UI_Time.text = _time;
    }

    private string FormatTimeSpan(TimeSpan time) {
        return ((time < TimeSpan.Zero) ? "-" : "") + time.ToString(@"mm\:ss");
    }

    public void ShowPickupSign(int points) {
        UI_Stay.text = $"Muy bien, conseguiste {points} puntos!";
        AS.PlayOneShot(AC_Pickup);
    }

    public void ShowStaySign(int seconds) {
        UI_Stay.text = $"No te muevas por {seconds} segundos!";
        float pitch = 1.43f - (.15f * seconds);
        AS.pitch = pitch;
        AS.PlayOneShot(AC_Stay);
    }

    public void HideStaySign() {
        UI_Stay.text = "";
    }

    IEnumerator HideSignAfterSeconds(float seconds) {
        yield return new WaitForSeconds(seconds);
        HideStaySign();
    }


    void SpawnCustomer() {
        int i = UnityEngine.Random.Range(0, customerPrefabs.Length);
        GameObject customer = customerPrefabs[i];
        Vector3 playerPos = Delivery_Bike.control.playerPosition();
        float posX = leftSidewalkGizmo.transform.position.x;
        posX += UnityEngine.Random.Range(-noise, noise);
        float posZ = UnityEngine.Random.Range(distance.x, distance.y) + playerPos.z;
        Vector3 pos = new Vector3(posX, -0.75f, posZ);
        GameObject newCustomer = Instantiate(customer, pos, Quaternion.Euler(new Vector3(0,90,0)));
        pendingPackages++;
    }
}
