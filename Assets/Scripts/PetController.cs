using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetController : MonoBehaviour
{
    public enum Mood {
        happy,
        sad,
        crazy,
        thirsty,
        hungry
    }

    public Mood currentMood;

    Animator PetAnimator;
    // Start is called before the first frame update
    void Start()
    {
        PetAnimator = GetComponentInChildren<Animator>();
    }

    public void DisplayMood(Mood mood) {
        PetAnimator.SetTrigger(mood.ToString());
        currentMood = mood;
        Debug.Log($"Now {mood}");
    }
}
