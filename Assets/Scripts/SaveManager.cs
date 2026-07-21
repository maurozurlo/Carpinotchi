using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveData {
    public int energy, hunger, sanity, hygiene;
    public int money;
    public bool isSick;
    public string lastSavedUtc;
    public List<int> itemIds = new List<int>();
    public List<int> itemQtys = new List<int>();
}

public class SaveManager : MonoBehaviour {
    public static SaveManager control;

    private const string homeSceneName = "House_00";
    private const int maxOfflineTicks = 60;
    private string SavePath => Application.persistentDataPath + "/save.json";

    void Awake() {
        if (control == null) {
            control = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        } else {
            DestroyImmediate(this);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == homeSceneName) {
            LoadGame();
        }
    }

    public void SaveGame() {
        SaveData data = new SaveData {
            energy = Pet.control.energy.GetCurrentValue(),
            hunger = Pet.control.hunger.GetCurrentValue(),
            sanity = Pet.control.sanity.GetCurrentValue(),
            hygiene = Pet.control.hygiene.GetCurrentValue(),
            money = MoneyManager.control.GetMoney(),
            isSick = Pet.control.isSick,
            lastSavedUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
        };

        if (ItemManager.control != null) {
            foreach (Item item in ItemManager.control.itemList) {
                data.itemIds.Add(item.id);
                data.itemQtys.Add(ItemManager.control.GetItemAmount(item.id));
            }
        } else if (File.Exists(SavePath)) {
            // ItemManager is scene-local (House_00 only); if a save fires while we're
            // in a minigame, keep whatever inventory was last written instead of wiping it.
            SaveData existing = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            data.itemIds = existing.itemIds;
            data.itemQtys = existing.itemQtys;
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(data));
    }

    public void LoadGame() {
        if (!File.Exists(SavePath)) return;

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));

        Pet.control.energy.SetValue(data.energy);
        Pet.control.hunger.SetValue(data.hunger);
        Pet.control.sanity.SetValue(data.sanity);
        Pet.control.hygiene.SetValue(data.hygiene);
        Pet.control.isSick = data.isSick;

        MoneyManager.control.LoadMoney(data.money);
        ItemManager.control.LoadInventory(data.itemIds, data.itemQtys);

        ApplyOfflineDecay(data.lastSavedUtc);

        Pet.control.UpdateUI();
    }

    void ApplyOfflineDecay(string lastSavedUtc) {
        if (string.IsNullOrEmpty(lastSavedUtc)) return;
        if (!DateTime.TryParse(lastSavedUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime lastSaved)) return;

        double elapsedSeconds = (DateTime.UtcNow - lastSaved).TotalSeconds;
        int ticks = Mathf.FloorToInt((float)(elapsedSeconds / TimeManager.control.GetHourLength()));
        ticks = Mathf.Clamp(ticks, 0, maxOfflineTicks);

        for (int i = 0; i < ticks; i++) {
            TimeManager.control.DecreaseStats();
        }
    }

    public void DeleteSave() {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }

    void OnApplicationQuit() {
        SaveGame();
    }

    void OnApplicationPause(bool pause) {
        if (pause) SaveGame();
    }
}
