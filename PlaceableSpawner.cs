using UnityEngine;

public class PlaceableSpawner : MonoBehaviour
{
    [Header("Prefab to Spawn")]
    public GameObject prefabToSpawn;

    [Header("Spawn Settings")]
    public float distanceInFront = 2f;

    [Header("Tracking spawned objects")]
    public Transform spawnedParent;

    public void Spawn()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("No prefab assigned to spawn!");
            return;
        }

        Vector3 spawnPos = transform.position + transform.forward * distanceInFront;
        Quaternion spawnRot = Quaternion.identity;

        GameObject spawnedObj = Instantiate(prefabToSpawn, spawnPos, spawnRot);

        if (spawnedParent != null)
        {
            spawnedObj.transform.SetParent(spawnedParent);
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            Spawn();
        }
    }
}