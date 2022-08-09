using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour {
    public static MoneyManager control;
    private int currentMoney = 9999;
    public GameObject money;
    // Start is called before the first frame update
    void Awake() {
        if (!control) {
            control = this;
        }
        else {
            DestroyImmediate(this);
        }
    }

    public int GetMoney() {
        return currentMoney;
    }

    public void SpendMoney(int amount) {
        currentMoney -= amount;
        UpdateGraphics();
    }

    public void WinMoney(int amount) {
        currentMoney += amount;
        UpdateGraphics();
    }

    void UpdateGraphics() {
        money.GetComponent<TextMeshProUGUI>().text = string.Format("{0:C2}", currentMoney);
    }
}
