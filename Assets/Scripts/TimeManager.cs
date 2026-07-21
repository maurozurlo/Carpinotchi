using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour {
    public static TimeManager control;

    [SerializeField]
    private float hourLength = 3600;

    void Awake() {
        if (control == null) {
            control = this;
            DontDestroyOnLoad(gameObject);
        } else {
            DestroyImmediate(this);
        }
    }

    // Start is called before the first frame update
    void Start() {
        StartCoroutine("DecreaseAllStats");
    }


    IEnumerator DecreaseAllStats() {
        DecreaseStats();
        yield return new WaitForSeconds(hourLength);
        StartCoroutine("DecreaseAllStats");
    }

    public float GetHourLength() {
        return hourLength;
    }

    public void DecreaseStats() {
        int amount = Pet.control.isSick ? -2 : -1;
        Pet.control.energy.ChangeValue(amount);
        Pet.control.hygiene.ChangeValue(amount);
        Pet.control.hunger.ChangeValue(amount);
        Pet.control.sanity.ChangeValue(amount);
        Pet.control.EvaluateSickness();

        if (SaveManager.control != null) {
            SaveManager.control.SaveGame();
        }
    }
}