using UnityEngine;

public class Fire_Fire : MonoBehaviour
{
    // Attack
    public float initialDelay = 2f; // Delay before the fire enemy starts draining health
    public float drainInterval = 1f; // Interval between health drain ticks
    public int initialDrainAmount = 1; // Health drained initially
    public int additionalDrainAmount = 1; // Additional health drained after initial delay
    public int finalDrainAmount = 2; // Health drained after initial delay and additional drain

    private float drainTimer;
    private bool isDraining;
    Fire_Manager manager;

    public int index; // The index of this fire in the mananger;

    // Health
    public int fireHealth = 3; // Health of the fire enemy
    public float drainRate = 0.5f; // Rate at which health drains per second while clicking
    private bool selfHealthDraining;
    private bool isDead;


    private void Start()
    {
        // Start the initial delay before health draining begins
        Invoke("StartDraining", initialDelay);
        manager = Fire_Manager.control;
    }

    private void Update()
    {
        if (isDead) return;
        Attack();
        TakeDamage();
    }

    void Attack()
    {
        if (!isDraining)
            return;

        drainTimer -= Time.deltaTime;

        if (drainTimer > 0f)
            return;

        if (drainTimer <= -initialDelay)
            manager.DrainBuildingHealth(finalDrainAmount);
        else if (drainTimer <= 0f)
            manager.DrainBuildingHealth(additionalDrainAmount);

        drainTimer = drainInterval;
    }

    void TakeDamage()
    {
        if (selfHealthDraining)
        {
            DrainHealth(Time.deltaTime * drainRate);

            if (fireHealth <= 0)
            {
                isDead = true;
                Die();
            }
        }
    }

    private void DrainHealth(float amount)
    {
        fireHealth -= Mathf.CeilToInt(amount);
    }

    private void StartDraining()
    {
        drainTimer = drainInterval;
        isDraining = true;
        manager.DrainBuildingHealth(initialDrainAmount);
    }

    private void Die()
    {
        // Perform death logic here
        manager.FireDestroyed(index);
        GetComponent<Fire_Fader>().enabled = true;
    }

    private void OnMouseDown()
    {
        if (isDead) return;
        selfHealthDraining = true;
    }

    private void OnMouseUp()
    {
        if (isDead) return;
        selfHealthDraining = false;
    }

}