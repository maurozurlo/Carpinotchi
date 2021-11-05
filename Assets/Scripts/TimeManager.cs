using UnityEngine;

    public class TimeManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            InvokeRepeating("DecreaseStats", 0, 2);
        }

        void DecreaseStats()
        {
            Pet.control.energy.ChangeValue(-1);
            Pet.control.hygiene.ChangeValue(-1);
            Pet.control.hunger.ChangeValue(-1);
        }
    }