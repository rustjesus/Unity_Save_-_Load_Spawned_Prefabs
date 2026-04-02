using UnityEngine;

public class PlaceableSpawner : MonoBehaviour
{
    [Header("Prefab to Spawn")]
    public GameObject bouyPrefabToSpawn;
    [SerializeField] private Material ghostMaterial;
    [SerializeField] private Material blockingMaterial;

    [Header("Spawn Settings")]
    public float distanceInFront = 2f;
    public bool showGhost = true;

    [Header("Tracking")]
    public Transform spawnedParent;

    private GameObject _ghostObject;
    private SpawnedObjectsSaveLoad spawnedObjectsSaveLoad;

    private void Awake()
    {
        spawnedObjectsSaveLoad = FindAnyObjectByType<SpawnedObjectsSaveLoad>();

        // Ensure ghostMaterial is red
        if (ghostMaterial != null)
            ghostMaterial.color = Color.red;
    }
    private void Update()
    {
        // Only create ghost if placing buoy
        if (GameManager.placingBuoy && _ghostObject == null)
        {
            CreateGhost();
        }

        if (_ghostObject != null)
        {
            // Toggle visibility
            if (_ghostObject.activeSelf != showGhost)
                _ghostObject.SetActive(showGhost);

            if (showGhost)
            {
                Vector3 spawnPos = transform.position + transform.forward * distanceInFront;

                // Check for blocking objects
                bool blocked = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, distanceInFront);
                if (blocked && hit.collider.gameObject.layer == LayerMask.NameToLayer("Placeable"))
                {
                    // Use blocking material when placement is blocked
                    SetGhostMaterial(_ghostObject.transform, blockingMaterial);
                    // Optional: move ghost slightly back to indicate blocked placement
                    _ghostObject.transform.position = transform.position + transform.forward * (distanceInFront * 0.5f);
                }
                else
                {
                    // Use normal ghost material
                    SetGhostMaterial(_ghostObject.transform, ghostMaterial);
                    _ghostObject.transform.position = spawnPos;
                }

                _ghostObject.transform.rotation = transform.rotation;
            }
        }

        // Spawn buoy on key press
        if ((KeyBindingManager.GetKey(KeyAction.attack) || KeyBindingManager.GetKey(KeyAction.attackAlt))
            && GameManager.placingBuoy)
        {
            bool blocked = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, distanceInFront);
            if (!(blocked && hit.collider.gameObject.layer == LayerMask.NameToLayer("Placeable")))
            {
                SpawnBuoy();
                spawnedObjectsSaveLoad.SaveSpawnedObjects();
                GameManager.placingBuoy = false;
                Destroy(_ghostObject);
                _ghostObject = null;
            }
            else
            {
                Debug.Log("Cannot place buoy: something is in the way!");
            }
        }
    }

    // Helper to recursively set material for ghost
    private void SetGhostMaterial(Transform target, Material mat)
    {
        if (target.TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
        {
            Material[] mats = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            renderer.materials = mats;
        }

        foreach (Transform child in target)
            SetGhostMaterial(child, mat);
    }
    private void CreateGhost()
    {
        if (bouyPrefabToSpawn == null)
        {
            Debug.LogWarning("No buoy prefab assigned!");
            return;
        }

        _ghostObject = Instantiate(bouyPrefabToSpawn);
        _ghostObject.name = "GHOST_PREVIEW";

        // Strip scripts
        foreach (var script in _ghostObject.GetComponentsInChildren<MonoBehaviour>())
            Destroy(script);

        // Strip colliders
        foreach (var col in _ghostObject.GetComponentsInChildren<Collider>())
            Destroy(col);

        // Strip audio
        foreach (var audio in _ghostObject.GetComponentsInChildren<AudioSource>())
            Destroy(audio);

        ApplyGhostMaterial(_ghostObject.transform);

        Debug.Log("Ghost successfully created!");
    }

    private void ApplyGhostMaterial(Transform target)
    {
        if (target.TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
        {
            Material[] mats = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = ghostMaterial;
            renderer.materials = mats;
        }

        foreach (Transform child in target)
            ApplyGhostMaterial(child);
    }

    private void SpawnBuoy()
    {
        Vector3 spawnPos = transform.position + transform.forward * distanceInFront;
        Instantiate(bouyPrefabToSpawn, spawnPos, transform.rotation, spawnedParent);
        Debug.Log("Buoy spawned!");
    }
}