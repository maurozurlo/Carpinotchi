using System.Collections;
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
    public TextMeshProUGUI UI_PackagesDelivered;
    //Stay
    public TextMeshProUGUI UI_Messager;
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
    public int timeToStartGame = 3;

    // Score
    int packagesDelivered = 0;
    int moneyEarned = 0;

    //Player
    public Delivery_Bike player;
    
    private void Awake() {
        control = this;
        AS = GetComponent<AudioSource>();
        StartCoroutine("StartGame");
    }

    private void Update() {
        if(gameState == GameState.Playing) {
            timeLeft -= Time.deltaTime;
            UpdateTimeUI();
            if ((int)Math.Floor(timeLeft) == 0) {
                GameOver();
            }
        }
    }

    public void GameOver() {
        gameState = GameState.Ended;
        player.enabled = false;
        timeLeft = 0;
        
        UI_Messager.text = $"Fin del juego!!\nConseguiste ${moneyEarned}.";

    }

    public void AddToPackagesDelivered(int amount) {
        timeLeft += amount * 10;
        packagesDelivered += amount;
        int moneyToEarn = Mathf.CeilToInt(Mathf.Log(10) * packagesDelivered);
        moneyEarned += moneyToEarn;
        StartCoroutine(HideSignAfterSeconds(AC_Pickup.length));
        int howManyToSpawn = Mathf.Clamp(Mathf.CeilToInt((Mathf.Log(4) * packagesDelivered) - packagesDelivered), 1, 4);
        for (int i = 0; i < howManyToSpawn; i++) {
            SpawnCustomer();
        }
        UpdateUI();
        ShowPickupSign(moneyToEarn);
    }

    void UpdateUI() {
        UI_Money.text = $"${moneyEarned}";
        UI_PackagesDelivered.text = $"{packagesDelivered}";
    }

    void UpdateTimeUI() {
        String _time = FormatTimeSpan(TimeSpan.FromSeconds(timeLeft));
        UI_Time.text = (int)Math.Floor(timeLeft) == 0 ? "-" : _time;
    }

    private string FormatTimeSpan(TimeSpan time) {
        return ((time < TimeSpan.Zero) ? "-" : "") + time.ToString(@"mm\:ss");
    }

    public void ShowPickupSign(int money) {
        UI_Messager.text = $"Muy bien, conseguiste ${money}!";
        AS.PlayOneShot(AC_Pickup);
    }

    void PlayBeep(int seconds)
    {
        float pitch = 1.43f - (.15f * seconds);
        AS.pitch = pitch;
        AS.PlayOneShot(AC_Stay);
    }

    public void ShowStaySign(int seconds) {
        UI_Messager.text = $"No te muevas por {seconds} segundos!";
        PlayBeep(seconds);
    }

    public void ShowStartSign(int seconds)
    {
        UI_Messager.text = $"El juego empieza en {seconds} segundos!\nMantene presionada la pantalla para frenar!";
        PlayBeep(seconds);
    }

    public void HideSign() {
        UI_Messager.text = "";
    }

    IEnumerator HideSignAfterSeconds(float seconds) {
        yield return new WaitForSeconds(seconds);
        HideSign();
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
    }

    IEnumerator StartGame()
    {
        for (int i = 0; i < timeToStartGame; i++)
        {
            ShowStartSign(timeToStartGame - i);
            yield return new WaitForSeconds(1);
        }
        HideSign();
        gameState = GameState.Playing;
        player.enabled = true;
    }
}
