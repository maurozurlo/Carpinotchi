using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire_FireParticlePool : MonoBehaviour
{
	public GameObject fireParticlePrefab;
    public GameObject foamParticlePrefab;

	private List<Fire_Fire> fireParticles = new List<Fire_Fire>();

    private List<FoamParticle> foamParticles = new List<FoamParticle>();

    public static Fire_FireParticlePool control;

    public int initialSize = 0;
    

    private void Awake()
    {
        control = this;
        InitializePool(initialSize);
    }

    public void InitializePool(int initialSize)
    {
        for (int i = 0; i < initialSize; i++)
        {
            Fire_Fire fireParticle = new Fire_Fire();
            fireParticle.gameObject = Instantiate(fireParticlePrefab, Vector3.zero, Quaternion.identity);
            fireParticle.gameObject.SetActive(false);
            fireParticle.isActive = false;
            fireParticles.Add(fireParticle);

            FoamParticle foam = new FoamParticle();
            foam.gameObject = Instantiate(foamParticlePrefab, Vector3.zero, Quaternion.identity);
            foam.gameObject.SetActive(false);
            foamParticles.Add(foam);
        }
    }

    public FoamParticle GetPooledFoamParticle()
    {
        foreach (FoamParticle foamParticle in foamParticles)
        {
            if (!foamParticle.isActive)
            {
                foamParticle.gameObject.SetActive(true);
                foamParticle.isActive = true;
                return foamParticle;
            }
        }

        // If no inactive fire particles are available, create and add a new one to the pool
        FoamParticle newFoamParticle = new FoamParticle();
        newFoamParticle.gameObject = Instantiate(foamParticlePrefab, Vector3.zero, Quaternion.identity);
        newFoamParticle.isActive = true;
        foamParticles.Add(newFoamParticle);

        return newFoamParticle;
    }

    public Fire_Fire GetPooledFireParticle()
    {
        foreach (Fire_Fire fireParticle in fireParticles)
        {
            if (!fireParticle.isActive)
            {
                fireParticle.gameObject.SetActive(true);
                fireParticle.isActive = true;
                return fireParticle;
            }
        }

        // If no inactive fire particles are available, create and add a new one to the pool
        Fire_Fire newFireParticle = new Fire_Fire();
        newFireParticle.gameObject = Instantiate(fireParticlePrefab, Vector3.zero, Quaternion.identity);
        newFireParticle.isActive = true;

        fireParticles.Add(newFireParticle);

        return newFireParticle;
    }

    public class FoamParticle
    {
        public GameObject gameObject;
        public bool isActive;
        // Add any other properties or methods specific to the fire particle behavior

        public void ReturnToPool()
        {
			control.ReturnFoamToPool(this);
        }
    }

    public void ReturnFireToPool(Fire_Fire fireParticle)
    {
        fireParticle.transform.gameObject.SetActive(false);

        fireParticle.isActive = false;
        fireParticle.transform.position = Vector3.zero;
    }

    public void ReturnFoamToPool(FoamParticle foamParticle)
    {
        foamParticle.gameObject.SetActive(false);
        foamParticle.isActive = false;
        foamParticle.gameObject.transform.position = Vector3.zero;
    }
}
