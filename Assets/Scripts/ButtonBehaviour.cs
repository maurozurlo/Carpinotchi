using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBehaviour : MonoBehaviour
{
    public void QuitGame()
    {
        if (SaveManager.control != null) {
            SaveManager.control.SaveGame();
        }
        Application.Quit();
        Debug.Log("Quit...");
    }
}
