using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour {
    public static MoneyManager control;
    private int currentMoney = 9999; // DEBUG MONEY
    GameObject money;
    void Awake() {
        if (!control) {
            control = this;
            DontDestroyOnLoad(gameObject);
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
        GameObject money = GetMoneyDisplay();
        money.GetComponent<TextMeshProUGUI>().text = string.Format("{0:C2}", currentMoney);
    }


    GameObject GetMoneyDisplay() {
        if (money) return money;
        GameObject moneyDisplay = GameObject.Find("MoneyDisplay");
        money = moneyDisplay;
        return money;
    }
}
