using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    public class UIManager : MonoBehaviour
    {
        public Image energyMeter;
        public Image hungerMeter;
        public Image sanityMeter;
        public Image hygieneMeter;

        void Start()
        {
            UpdateStatsUI();
        }

        public void UpdateStatsUI()
        {
            energyMeter.fillAmount = Pet.control.energy.GetPercentage();
            hungerMeter.fillAmount = Pet.control.hunger.GetPercentage();
            sanityMeter.fillAmount = Pet.control.sanity.GetPercentage();
            hygieneMeter.fillAmount = Pet.control.hygiene.GetPercentage();
        }
    }