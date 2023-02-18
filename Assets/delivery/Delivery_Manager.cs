using System.Collections;
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
    public TextMeshProUGUI UI_Money;
    public TextMeshProUGUI UI_Time;
    public TextMeshProUGUI UI_PackagesDelivered;
    public GameObject UI_GameOver;
    public TextMeshProUGUI UI_GameOverText;
    //Sign
    public TextMeshProUGUI UI_Messager;
    public AudioClip AC_Stay;
    // Pickup
    public AudioClip AC_Pickup;

    // StopLight
    public Delivery_Streetlight stopLight;

    // Customers
    [Header("Customers")]
    public float spawnTime = 2;
    public GameObject[] customerPrefabs;
    public GameObject customerSpawnArea;
    public Vector2 distance = new Vector2(50, 100);
    public float noise;
    public int maxActiveCustomers = 5;
    // TODO: Make this private after test;
    public int streetNumber = 0;
    public Vector3 distanceToNextCrossing;
    public float threshold;
    public float lastPos = 47.9f;
    public float lastSpawn;

    //Audio
    AudioSource AS;

    // Time
    public int initialSeconds = 10;
    public int maxPackages = 20;
    public float timeLeft = 10;
    public float secondsPerPackage = 0.5f;
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
        InvokeRepeating("CheckIfNeedToSpawn", timeToStartGame * 3, timeToStartGame);
    }

    private void Update() {
        if(gameState == GameState.Playing) {
            timeLeft -= Time.deltaTime;
            UpdateTimeUI();
            if ((int)Mathf.Floor(timeLeft) == 0) {
                GameOver();
            }
        }
    }

    public void GameOver() {
        gameState = GameState.Ended;
        StopAllCoroutines();
        player.enabled = false;
        timeLeft = 0;
        stopLight.Cleanup();

        UI_GameOver.SetActive(true);
        UI_GameOverText.text = $"Conseguiste ${moneyEarned}";
        HideSign();

    }

    public void AddToPackagesDelivered(int amount) {
        
        float remainingSeconds = Mathf.Clamp(Mathf.CeilToInt(initialSeconds - (secondsPerPackage * Mathf.Min(packagesDelivered, maxPackages))), 2, 10);
        timeLeft += remainingSeconds;
        packagesDelivered += amount;
        int moneyToEarn = Mathf.CeilToInt(Mathf.Log(10) * packagesDelivered);
        moneyEarned += moneyToEarn;
        StartCoroutine(HideSignAfterSeconds(AC_Pickup.length));
        UpdateUI();
        ShowPickupSign(moneyToEarn);

        StartCoroutine("WaitAndSpawnCustomer");
    }

    void UpdateUI() {
        UI_Money.text = $"${moneyEarned}";
        UI_PackagesDelivered.text = $"{packagesDelivered}";
    }

    void UpdateTimeUI() {
        string _time = FormatTimeSpan(System.TimeSpan.FromSeconds(timeLeft));
        UI_Time.text = (int)Mathf.Floor(timeLeft) == 0 ? "-" : _time;
    }

    private string FormatTimeSpan(System.TimeSpan time) {
        return ((time < System.TimeSpan.Zero) ? "-" : "") + time.ToString(@"mm\:ss");
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

    IEnumerator WaitAndSpawnCustomer()
    {
        yield return new WaitForSeconds(Random.Range(spawnTime, spawnTime * 1.5f));
        SpawnCustomer();
    }


    public void SpawnCustomer() {
        GameObject[] customers = GameObject.FindGameObjectsWithTag("Target");
        if (customers.Length >= maxActiveCustomers) {
            return;
        }
		int i = Random.Range(0, customerPrefabs.Length);
        GameObject customer = customerPrefabs[i];
        Vector3 pos = GetRandomPointAheadOfPlayer();

        if (pos.z == 0)
        {
            return;
        }
        GameObject newCustomer = Instantiate(customer, pos, Quaternion.Euler(new Vector3(0,90,0)));
        lastSpawn = Time.realtimeSinceStartup;
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

    public void MoveSpawnArea()
    {
        streetNumber++;
        //customerSpawnArea.transform.position += distanceToNextCrossing;
    }

    public Vector3 GetRandomPointAheadOfPlayer()
    {
        float playerPosition = player.transform.position.z;
        float posX = -12.2f;

        float streetMultiplier = streetNumber * distanceToNextCrossing.z;
        // Min Distance
        float _minDistance = -70; // Beginning of block
        float minDistance = _minDistance + streetMultiplier;
		float finalMinDistance;

		if (minDistance > playerPosition) {
            finalMinDistance = minDistance;
            Debug.Log($"MIN: Inicio de cuadra + streetMultiplier = {finalMinDistance}");
        } else {
            finalMinDistance = playerPosition;
            Debug.Log($"MIN: Posicion del player = {finalMinDistance}");
        }
        
        // Max Distance
        float maxDistance = 90; // End of block
        float finalMaxDistance = maxDistance + streetMultiplier;
        Debug.Log($"MAX: {finalMaxDistance}");

        float newMinDistance = finalMinDistance + threshold;
        Debug.Log($"MIN: finalMin + threshold = {newMinDistance}");

        if (newMinDistance > finalMaxDistance) {
            Debug.Log($"New Min es ({newMinDistance}). Es mayor a la Max ({finalMaxDistance}), no puedo spawnear mas en esta cuadra");
            return Vector3.zero;
        }

        // Random Point Between Max / Min
        float posZ = Random.Range(lastPos + threshold, newMinDistance);
        Debug.Log($"{posZ} es un numero entre {lastPos} (ultimo spawneo) + {threshold} y new min: ${newMinDistance}");

        if (posZ < finalMinDistance)
        {
            Debug.Log($"Si {posZ} es menor a final min distance; entonces usamos final min distance {finalMinDistance}");
            posZ = Random.Range(finalMinDistance, finalMaxDistance);
            Debug.Log($"La pos final es {posZ}");
        }

        Vector3 finalPos = new Vector3(posX, -0.75f, posZ);

        lastPos = posZ;
        
        return finalPos;
    }

    void CheckIfNeedToSpawn()
    {
        if (lastSpawn < (Time.realtimeSinceStartup + timeToStartGame * 5))
        {
            Debug.Log(lastSpawn);
            SpawnCustomer();
        }
    }
}
