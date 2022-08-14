using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameManager : MonoBehaviour
{
    public GameObject modal;
    
    public void OpenModal() {
        modal.SetActive(true);
    }

    public void CloseModal() {
        modal.SetActive(false);
    }


    public void LoadScene(string sceneName) {
       if(sceneName == SceneManager.GetActiveScene().name) {
            return;
        }

       // TODO: Make me async
        SceneManager.LoadScene(sceneName);
    }
}
