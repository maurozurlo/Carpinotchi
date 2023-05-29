using System.Collections;
using UnityEngine;

public class Fire_Fire : MonoBehaviour
{
    // Attack
    public float initialDelay = 2f;
    public float drainInterval = 1f;
    public int initialDrainAmount = 1;
    public int additionalDrainAmount = 1;
    public int finalDrainAmount = 2;

    private float attackTimer;
    private bool isAttacking;
    private Fire_Manager manager;
    public int index;

    // Health
    public int enemyHealth = 3;
    public float drainRate = 0.5f;
    private bool isHealthDraining;
    private bool isDead;

    // Foam
    public GameObject foamParticlePrefab;
    private GameObject foamParticleInstance;
    private float foamParticleTimeToLive = 2f;

    // Particles
    public float fadeDuration = 1f;
    private ParticleSystem[] particleSystems;

    private void Start()
    {
        Invoke("StartAttacking", initialDelay);
        manager = Fire_Manager.control;
        particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    private void Update()
    {
        if (isDead) return;
        Attack();
        TakeDamage();
    }

    private void Attack()
    {
        if (!isAttacking) return;

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f) return;

        if (attackTimer <= -initialDelay)
        {
            manager.DrainBuildingHealth(finalDrainAmount);
        }
        else if (attackTimer <= 0f)
        {
            manager.DrainBuildingHealth(additionalDrainAmount);
        }

        attackTimer = drainInterval;
    }

    private void TakeDamage()
    {
        if (isHealthDraining)
        {
            DrainHealth(Time.deltaTime * drainRate);
            Debug.Log(Time.deltaTime);

            if (enemyHealth <= 0)
            {
                isDead = true;
                Die();
            }
        }
    }

    private void DrainHealth(float amount)
    {
        enemyHealth -= Mathf.CeilToInt(amount);
    }

    private void StartAttacking()
    {
        attackTimer = drainInterval;
        isAttacking = true;
        manager.DrainBuildingHealth(initialDrainAmount);
    }

    private void Die()
    {
        manager.FireDestroyed(index);
        StartCoroutine(PutOutFire());
    }

    private void OnMouseDown()
    {
        if (isDead)
        {
            DestroyFoamParticle();
            return;
        }

        if(foamParticleInstance)
        foamParticleInstance.GetComponentInChildren<ParticleSystem>().Stop();

        isHealthDraining = true;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 clickPosition = hit.point;
            Vector3 spawnDirection = Camera.main.transform.position - clickPosition;
            Quaternion spawnRotation = Quaternion.LookRotation(spawnDirection, Vector3.up);
            foamParticleInstance = Instantiate(foamParticlePrefab, clickPosition, spawnRotation);
            foamParticleInstance.transform.SetParent(gameObject.transform);
        }


        StartCoroutine(CountdownFoamParticle());
    }

    private IEnumerator CountdownFoamParticle()
    {
        yield return new WaitForSeconds(foamParticleTimeToLive);
        DestroyFoamParticle();
    }

    private void DestroyFoamParticle()
    {
        if (foamParticleInstance != null)
        {
            Destroy(foamParticleInstance);
            foamParticleInstance = null;
        }
    }

    private void OnMouseUp()
    {
        if (isDead)
        {
            DestroyFoamParticle();
            return;
        }

        isHealthDraining = false;
    }

    private IEnumerator PutOutFire()
    {
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Stop();
        }

        yield return new WaitForSeconds(fadeDuration * 2);
        Destroy(gameObject);
    }
}
