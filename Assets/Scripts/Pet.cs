using UnityEngine;

public class Pet : MonoBehaviour {
    public static Pet control;
    [System.Serializable]
    public struct Stat {
        [SerializeField]
        private int currentValue;
        [SerializeField]
        private int maxValue;
        private readonly int min;
        private bool isLocked;

        public Stat(int currentValue, int maxValue) {
            this.currentValue = currentValue;
            this.maxValue = maxValue;
            this.isLocked = false;
            min = 0;
        }

        public int GetCurrentValue() {
            return currentValue;
        }

        public int GetMaxValue() {
            return maxValue;
        }

        public float GetPercentage() {
            float perc = (float)currentValue / (float)maxValue;
            return perc;
        }

        public void ChangeLockState(bool newState) {
            this.isLocked = newState;
        }

        public void ChangeValue(int value) {
            if (isLocked) return;
            currentValue = Mathf.Clamp(currentValue + value, min, maxValue);
            Pet.control.UpdateUI();
        }

        public void SetValue(int newValue) {
            currentValue = Mathf.Clamp(newValue, min, maxValue);
        }
    }

    public void UpdateUI() {
        SendMessage("UpdateStatsUI", SendMessageOptions.DontRequireReceiver);
    }

    [SerializeField]
    public Stat energy = new Stat(60, 100);
    [SerializeField]
    public Stat hygiene = new Stat(50, 100);
    [SerializeField]
    public Stat sanity = new Stat(90, 100);
    [SerializeField]
    public Stat hunger = new Stat(80, 100);

    public bool isSick;
    private int neglectTicks;
    private int recoveryTicks;

    private void Awake() {
        if (control == null) {
            control = this;
            DontDestroyOnLoad(gameObject);
        } else {
            DestroyImmediate(this);
        }
    }

    public void EvaluateSickness() {
        bool inNeglect = sanity.GetCurrentValue() == 0 || hunger.GetCurrentValue() == 0 || hygiene.GetCurrentValue() == 0;
        if (inNeglect) {
            neglectTicks++;
            recoveryTicks = 0;
            if (neglectTicks >= 3) isSick = true;
            return;
        }

        neglectTicks = 0;
        if (!isSick) return;

        bool allHealthy = energy.GetPercentage() >= 0.5f && hunger.GetPercentage() >= 0.5f
            && sanity.GetPercentage() >= 0.5f && hygiene.GetPercentage() >= 0.5f;
        if (allHealthy) {
            recoveryTicks++;
            if (recoveryTicks >= 2) isSick = false;
        } else {
            recoveryTicks = 0;
        }
    }
}