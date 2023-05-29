using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Fire_Manager : MonoBehaviour
{
    public enum GameState
    {
        Start,
        Playing,
        Ended
    }

    public GameState gameState = GameState.Start;

    public static Fire_Manager control;
    public TextMeshProUGUI UI_Money;
    public TextMeshProUGUI UI_Time;
    public TextMeshProUGUI UI_Building;
    public GameObject UI_GameOver;
    public TextMeshProUGUI UI_GameOverText;
    //Sign
    public TextMeshProUGUI UI_Messager;
    public AudioClip AC_Stay;


    // Fire
    [Header("Fire")]
    public GameObject fireSpawnPoint;
    public Vector2 fireDisplacement = new Vector2(8.05f, 5.06f);
    public GameObject fireParticlePrefab;

    public bool[] fireSpots = new bool[9];

    //Audio
    AudioSource AS;

    public AudioClip fireLoop;

    // Time
    public int initialSeconds = 10;
    public float initialTimeLeft = 30;
    float timeLeft;
    public float secondsPerFire = 0.5f;
    public int timeToStartGame = 3;

    // Score
    int buildingHealth = 100;
    int moneyEarned = 0;
    int firesPutOut = 0;
    int firesSpawned = 0;
    public int maxFires = 20;

    private void Awake()
    {
        control = this;
        AS = GetComponent<AudioSource>();

        StartCoroutine(StartGame());

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
        //player.enabled = true;
        timeLeft = initialTimeLeft;
        StartCoroutine(SpawnFires());
        UpdateUI();
        AS.clip = fireLoop;
        AS.loop = true;
        AS.volume = 0.1f;
        AS.Play();
    }


    private void Update()
    {
        if (gameState == GameState.Playing)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimeUI();
            if ((int)Mathf.Floor(timeLeft) == 0)
            {
                GameOver();
            }
        }
    }

    public void GameOver()
    {
        AS.volume = 1;
        AS.clip = null;
        AS.loop = false;
        AS.Stop();
        gameState = GameState.Ended;
        StopCoroutine("SpawnFires");
        timeLeft = 0;

        UI_GameOver.SetActive(true);
        UI_GameOverText.text = $"Conseguiste ${moneyEarned}";
        HideSign();

    }


    void UpdateUI()
    {
        UI_Money.text = $"${moneyEarned}";
        UI_Building.text = $"%{buildingHealth}";
    }

    void UpdateTimeUI()
    {
        StringBuilder stringBuilder = new StringBuilder();
        if ((int)Mathf.Floor(timeLeft) == 0)
        {
            UI_Time.text = "-";
        }
        else
        {
            stringBuilder.Append(FormatTimeSpan(System.TimeSpan.FromSeconds(timeLeft)));
            UI_Time.text = stringBuilder.ToString();
        }
    }

    private string FormatTimeSpan(System.TimeSpan time)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append((time < System.TimeSpan.Zero) ? "-" : "");
        stringBuilder.Append(time.ToString(@"mm\:ss"));
        return stringBuilder.ToString();
    }

    void PlayBeep(int seconds)
    {
        float scale = Mathf.Pow(2f, 1.0f / 12f);
        AS.pitch = Mathf.Pow(scale, seconds);
        AS.PlayOneShot(AC_Stay);
    }

    public void ShowStartSign(int seconds)
    {
        UI_Messager.text = $"El juego empieza en {seconds} segundos!\nMantene apreta sobre los fuegos para apagarlos!";
        PlayBeep(seconds);
    }

    public void HideSign()
    {
        UI_Messager.text = "";
    }

    IEnumerator SpawnFires()
    {
        if (gameState != GameState.Playing) yield return null;
        if (firesSpawned == maxFires) yield return null;

        // Find Place To Spawn Fire
        // Check if any fire spot is available
        bool isAvailable = false;
        foreach (bool spot in fireSpots)
        {
            if (!spot)
            {
                isAvailable = true;
                break;
            }
        }

        // Spawn fire if spot is available
        if (isAvailable)
        {
            int randomSpot = UnityEngine.Random.Range(0, fireSpots.Length);
            while (fireSpots[randomSpot])
            {
                randomSpot = UnityEngine.Random.Range(0, fireSpots.Length);
            }
            fireSpots[randomSpot] = true;
            GameObject fire = Instantiate(fireParticlePrefab, GetFireSpawnPosition(randomSpot), Quaternion.Euler(new Vector3(-90, 0, 0)));
            firesSpawned++;
            fire.GetComponent<Fire_Fire>().index = randomSpot;

            // Adjust the volume based on the number of active fires
            int activeFireCount = GetActiveFireCount();
            float volume = Mathf.Lerp(0.1f, 1f, (float)activeFireCount / fireSpots.Length);
            AS.volume = volume;

        }

        yield return new WaitForSeconds(secondsPerFire);
        StartCoroutine(SpawnFires());
    }

    Vector3 GetFireSpawnPosition(int index)
    {
        // Define the matrix size
        int rows = 3;
        int cols = 3;

        // Calculate the row and column based on the index
        int row = index / cols;
        int col = index % cols;

        // Calculate the position using the displacements and matrix values
        float xPos = fireSpawnPoint.transform.position.x - (col - (cols - 1) / 2f) * fireDisplacement.x;
        float yPos = fireSpawnPoint.transform.position.y - (rows - 1 - row) * fireDisplacement.y;
        float zPos = 4.54f;

        return new Vector3(xPos, yPos, zPos);
    }

    public void ResetGame()
    {
        StopCoroutine("SpawnFires");

        Camera.main.GetComponent<CameraShake>().StopAllCoroutines();
        Camera.main.GetComponent<SmoothFollow>().enabled = true;

        UI_GameOver.SetActive(false);

        moneyEarned = 0;
        foreach (GameObject fire in GameObject.FindGameObjectsWithTag("Target"))
        {
            if (fire.CompareTag("Target"))
                DestroyImmediate(fire);
        }
        StartCoroutine(StartGame());
    }

    public void GoBackHome()
    {
        SceneManager.LoadScene("House_00");
    }

    public void DrainBuildingHealth(int health)
    {
        buildingHealth -= health;
        UpdateUI();
    }

    public void FireDestroyed(int index)
    {
        fireSpots[index] = false;
        firesPutOut++;
        moneyEarned += GetPointsFromIndex(firesPutOut);
        float remainingSeconds = Mathf.Clamp(Mathf.CeilToInt(initialSeconds - (secondsPerFire * Mathf.Min(index, firesSpawned))), 2, 10);
        timeLeft += remainingSeconds;
        UpdateUI();

        // Adjust the volume based on the number of active fires
        int activeFireCount = GetActiveFireCount();
        float volume = Mathf.Lerp(0.1f, 1f, (float)activeFireCount / fireSpots.Length);
        AS.volume = volume;
    }

    int GetActiveFireCount()
    {
        int count = 0;
        foreach (bool spot in fireSpots)
        {
            if (spot)
            {
                count++;
            }
        }
        return count;
    }

    int GetPointsFromIndex(int index)
    {
        return Mathf.CeilToInt(Mathf.Log(10) * index);
    }
}
