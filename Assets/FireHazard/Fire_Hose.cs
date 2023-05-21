using System.Collections;
using UnityEngine;

public class Fire_Hose : MonoBehaviour
{
    public GameObject waterPrefab;
    public float waterSpeed = 10f;
    public float waterLifetime = 1f;
    public float trailDelay = 2f;
    public float trailDuration = 0.5f;

    private bool isShooting;
    private GameObject currentWater;
    private TrailRenderer currentTrailRenderer;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                StartShooting(hit.point);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopShooting();
        }

        if (isShooting)
        {
            if (currentWater != null)
            {
                currentWater.transform.position = Vector3.MoveTowards(currentWater.transform.position, currentWater.transform.position + transform.forward, waterSpeed * Time.deltaTime);
            }
        }
    }

    private void StartShooting(Vector3 hitPoint)
    {
        currentWater = Instantiate(waterPrefab, Camera.main.transform.position, Quaternion.identity, transform);
        currentWater.transform.LookAt(hitPoint);

        currentTrailRenderer = currentWater.GetComponentInChildren<TrailRenderer>();
        currentTrailRenderer.time = trailDuration;

        StartCoroutine(MoveTrail(hitPoint));

        isShooting = true;
    }

    private IEnumerator MoveTrail(Vector3 hitPoint)
    {
        float startTime = Time.time;

        while (Time.time - startTime < trailDuration)
        {
            if (!currentWater) break;
            float fraction = (Time.time - startTime) / trailDuration;
            currentWater.transform.position = Vector3.Lerp(Camera.main.transform.position, hitPoint, fraction);
            yield return null;
        }
    }

    private void StopShooting()
    {
        if (currentWater != null)
        {
            Destroy(currentWater, waterLifetime);
            currentWater = null;
            currentTrailRenderer = null;
        }

        isShooting = false;
    }
}
