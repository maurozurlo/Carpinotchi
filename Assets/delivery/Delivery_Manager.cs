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
    public GameObject rightSidewalkGizmo, leftSidewalkGizmo;
    public Vector2 distance = new Vector2(50, 100);
    public float noise;

    //Audio
    AudioSource AS;

    // Time
    public float timeLeft;

    int packagesDelivered = 0;
    
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
        timeLeft += amount * 10;
        packagesDelivered += amount;
        UpdateUI();
        ShowPickupSign(amount);
        StartCoroutine(HideSignAfterSeconds(AC_Pickup.length));
    }

    void UpdateUI() {
        UI_Packages.text = $"Paquetes entregados: {packagesDelivered}";
    }

    void UpdateTimeUI() {
        System.TimeSpan time = System.TimeSpan.FromSeconds(timeLeft);
        UI_Time.text = $"Tiempo restante: {time.ToString("hh':'mm':'ss")}";
    }

    public void ShowPickupSign(int points) {
        UI_Stay.text = $"Muy bien, conseguiste {points} puntos!";
        AS.PlayOneShot(AC_Pickup);
    }

    public void ShowStaySign(int seconds) {
        UI_Stay.text = $"No te muevas por {seconds} segundos!";
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
        int indice = Random.Range(0, customerPrefabs.Length);
        GameObject customer = customerPrefabs[indice];

        Vector3 posicionDelJugador = BikeMovement.control.playerPosition();

        float numeroRandom = Random.Range(distance.x, distance.y);

        //posicionDelJugador.z;
        Vector3 posicion = new Vector3(11.76f, -0.75f, posicionDelJugador.z + numeroRandom);

        GameObject newCustomer = Instantiate(customer, posicion, Quaternion.identity);
    }
}
