using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Delivery_Manager : MonoBehaviour
{
    public enum GameState {
        Start,
        Playing,
        Ended
    }

    public GameState gameState = GameState.Start;

    public static Delivery_Manager control;
    public TextMeshProUGUI UI_Packages;
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
        UI_Packages.text = $"Paquetes entregados: {packagesDelivered}";
        UI_PendingPackages.text = $"Pendientes: {pendingPackages}";
    }

    void UpdateTimeUI() {
        System.TimeSpan time = System.TimeSpan.FromSeconds(timeLeft);
        UI_Time.text = $"Tiempo restante: {time:hh':'mm':'ss}";
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
        int i = Random.Range(0, customerPrefabs.Length);
        GameObject customer = customerPrefabs[i];
        Vector3 playerPos = Delivery_Bike.control.playerPosition();
        float posX = leftSidewalkGizmo.transform.position.x;
        posX += Random.Range(-noise, noise);
        float posZ = Random.Range(distance.x, distance.y) + playerPos.z;
        Vector3 pos = new Vector3(posX, -0.75f, posZ);
        GameObject newCustomer = Instantiate(customer, pos, Quaternion.identity);
        pendingPackages++;
    }
}
