using UnityEngine;

public class Pet : MonoBehaviour {
    public static Pet control;
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
            min = 1;
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
            if (value < 0) {
                //Take
                if ((currentValue + value) > min) {
                    currentValue += value;
                }
            }

            if (value > 0) {
                //Add
                if ((currentValue + value) < maxValue) {
                    currentValue += value;
                }
            }

            Pet.control.UpdateUI();
        }
    }

    void UpdateUI() {
        SendMessage("UpdateStatsUI");
    }

    [SerializeField]
    public Stat energy = new Stat(60, 100);
    [SerializeField]
    public Stat hygiene = new Stat(50, 100);
    [SerializeField]
    public Stat sanity = new Stat(90, 100);
    [SerializeField]
    public Stat hunger = new Stat(80, 100);

    private void Awake() {
        if (control == null)
            control = this;
        else
            DestroyImmediate(this);
    }
}