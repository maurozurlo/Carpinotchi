using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Delivery_Manager : MonoBehaviour
{
    public enum GameState {
        Start,
        Playing,
        Ended
    }

    public GameState gameState = GameState.Start;

    bool isFirstGame = true;
    public GameObject firstCustomerGO;

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
    public GameObject[] customerPrefabs;
    public const int blocks = 8;
    List<float> customerPositions = new List<float>();
    public float threshold = 20;


    //Audio
    AudioSource AS;

    // Time
    public int initialSeconds = 10;
    public int maxPackages = 20;
    public float initialTimeLeft = 30;
    float timeLeft;
    public float secondsPerPackage = 0.5f;
    public int timeToStartGame = 3;

    // Score
    int packagesDelivered = 0;
    int moneyEarned = 0;

    //Player
    public Delivery_Bike player;
    private Vector3 originalPlayerPos;
    private Quaternion originalPlayerRot;

    // First Customer Data
    Vector3 _fcp;
    Quaternion _fcr;
    int _fcpp;
    int _fcss;
    
    private void Awake() {
        control = this;
        AS = GetComponent<AudioSource>();
        // Store player initial pos/rot
        originalPlayerPos = player.transform.position;
        originalPlayerRot = player.transform.rotation;
        StartCoroutine("StartGame");

        Delivery_Customer firstCustomer = firstCustomerGO.GetComponent<Delivery_Customer>();
        _fcp = firstCustomer.transform.position;
        _fcr = firstCustomer.transform.rotation;
        _fcpp = firstCustomer.GetPoints();
        _fcss = firstCustomer.staySeconds;
    }

    IEnumerator StartGame()
    {
        SpawnCustomers();
        UpdateUI();
        Camera.main.GetComponent<CameraShake>().StopShake();
        if (!isFirstGame)
        {
            SpawnFirstCustomer();
        }
        for (int i = 0; i < timeToStartGame; i++)
        {
            ShowStartSign(timeToStartGame - i);
            yield return new WaitForSeconds(1);
        }
        HideSign();
        gameState = GameState.Playing;
        player.enabled = true;
        timeLeft = initialTimeLeft;
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

    public void AddToPackagesDelivered(int index, int amount) {
        
        float remainingSeconds = Mathf.Clamp(Mathf.CeilToInt(initialSeconds - (secondsPerPackage * Mathf.Min(index, maxPackages))), 2, 10);
        timeLeft += remainingSeconds;
        packagesDelivered++;
        moneyEarned += amount;
        StartCoroutine(HideSignAfterSeconds(AC_Pickup.length));
        UpdateUI();
        ShowPickupSign(amount);
    }

    void UpdateUI() {
        UI_Money.text = moneyEarned > 0 ? $"${moneyEarned}" : "-";
        UI_PackagesDelivered.text = packagesDelivered > 0 ? $"{packagesDelivered}" : "-";
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
        float scale = Mathf.Pow(2f, 1.0f / 12f);
        AS.pitch = Mathf.Pow(scale, 1);
        AS.PlayOneShot(AC_Pickup);
    }

    void PlayBeep(int seconds)
    {
        float scale = Mathf.Pow(2f, 1.0f / 12f);
        AS.pitch = Mathf.Pow(scale, seconds);
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

    void SpawnCustomers()
    {
        // Spawn amount of customers per blocks
        int iteration = 0;
        for (int block = 1; block < blocks; block++)
        {
            int customersInThisBlock = Mathf.FloorToInt(4.64286f - (0.47619f * block));
            for (int customer = 0; customer < customersInThisBlock; customer++)
            {
                iteration++;
                SpawnCustomer(block, iteration);
            }
            iteration++;
        }
    }

    void SpawnFirstCustomer()
    {
        int i = Random.Range(0, customerPrefabs.Length);
        GameObject customerPrefab = customerPrefabs[i];
        GameObject newCustomer = Instantiate(customerPrefab, _fcp, _fcr);
        newCustomer.GetComponent<Delivery_Customer>().SetValues(0, _fcpp, _fcss);
        customerPositions.Add(_fcp.z);
    }

    public void SpawnCustomer(int block, int index) {
		int i = Random.Range(0, customerPrefabs.Length);
        GameObject customer = customerPrefabs[i];
        object pos = GetCustomerSpawnPoint(block);
        if (pos is Vector3 vector) {
            int points = GetPointsFromIndex(index);
            int staySeconds = GetStaySecondsFromIndex(index);
            GameObject newCustomer = Instantiate(customer, vector, Quaternion.Euler(new Vector3(0, 90, 0)));
            newCustomer.GetComponent<Delivery_Customer>().SetValues(index, points, staySeconds);
            customerPositions.Add(vector.z);
        }
    }

    int GetPointsFromIndex(int index) {
        return Mathf.CeilToInt(Mathf.Log(10) * index);
    }

    int GetStaySecondsFromIndex(int index) {
        return Mathf.Min(Mathf.FloorToInt((0.198102f * index) + 2.39992f), 8);
    }

    bool checkDistance(float otherCustomerPos) {
        foreach (float customerPos in customerPositions)
        {
            float absoluteDistance = Mathf.Abs(otherCustomerPos - customerPos);
            bool check = absoluteDistance >= threshold;
            if (!check) return check;
        }
        return true;
    }

    public dynamic GetCustomerSpawnPoint(int block)
    {
        const float posX = -12.2f;
        const float posY = -0.75f;
        float streetMultiplier = block * 200;
        // Min Distance
        float minDistance = -70;
        float finalMinDistance = minDistance + streetMultiplier;  // Beginning of block
        // Max Distance
        float maxDistance = 90; 
        float finalMaxDistance = maxDistance + streetMultiplier; // End of block
        // Random Point Between Max / Min
        float posZ = Random.Range(finalMinDistance, finalMaxDistance);
        Vector3 finalPos = new Vector3(posX, posY, posZ);
        return checkDistance(posZ) ? finalPos : (Vector3?)null;
    }

    public void ResetGame()
    {
        isFirstGame = false;
        customerPositions.Clear();
        StopAllCoroutines();
        stopLight.Cleanup();
        // Store player initial pos/rot
        player.transform.position = originalPlayerPos;
        player.transform.rotation = originalPlayerRot;
        player.InitSpeed();

//        Camera.main.GetComponent<CameraShake>().StopAllCoroutines();
        Camera.main.GetComponent<CameraShake>().enabled = false;
        Camera.main.GetComponent<SmoothFollow>().enabled = true;
        
        stopLight.ResetPositions();
        
        UI_GameOver.SetActive(false);
        packagesDelivered = 0;
        moneyEarned = 0;

        foreach (GameObject customer in GameObject.FindGameObjectsWithTag("Target"))
        {
            DestroyImmediate(customer);
        }

        foreach (GameObject car in GameObject.FindGameObjectsWithTag("Obstacle"))
        {
            DestroyImmediate(car);
        }

        StartCoroutine("StartGame");
    }

    public void GoBackHome()
    {
        SceneManager.LoadScene("House_00");
    }
}
