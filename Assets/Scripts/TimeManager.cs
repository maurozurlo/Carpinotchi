using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour {

    [SerializeField]
    private float hourLength = 1;
    // Start is called before the first frame update
    void Start() {
        StartCoroutine("DecreaseAllStats");
    }


    IEnumerator DecreaseAllStats() {
        DecreaseStats();
        yield return new WaitForSeconds(hourLength);
        StartCoroutine("DecreaseAllStats");
    }

    void DecreaseStats() {
        Pet.control.energy.ChangeValue(-1);
        Pet.control.hygiene.ChangeValue(-1);
        Pet.control.hunger.ChangeValue(-1);
        Pet.control.sanity.ChangeValue(-1);
    }
}