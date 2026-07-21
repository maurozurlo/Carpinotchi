using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {
    public Button continueButton;

    void Start() {
        if (continueButton != null) {
            continueButton.interactable = File.Exists(Application.persistentDataPath + "/save.json");
        }
    }

    public void StartNewGame() {
        string savePath = Application.persistentDataPath + "/save.json";
        if (File.Exists(savePath)) {
            File.Delete(savePath);
        }
        SceneManager.LoadScene("House_00");
    }

    public void ContinueGame() {
        SceneManager.LoadScene("House_00");
    }

    public void QuitGame() {
        Application.Quit();
    }
}
