using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SpawnedObjectData
{
    public string prefabName;    // Name of the prefab
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class SpawnedObjectsSave
{
    public List<SpawnedObjectData> objects = new List<SpawnedObjectData>();
}

public class SpawnedObjectsSaveLoad : MonoBehaviour
{
    [Header("Parent for spawned objects")]
    public Transform spawnedObjectsParent;

    [Header("Prefabs to spawn (assign in Inspector)")]
    public List<GameObject> prefabList = new List<GameObject>();

    [Header("Directory Settings")]
    private string folderName = "Saves";
    private string subFolderName = "Save1";
    private string fileName = "SpawnedObjects.json";

    private string saveDirectory;
    private string saveFilePath;

    private Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();

    private void Awake()
    {
        SetupPath();
        SetupPrefabMap();
        LoadSpawnedObjects();
    }

    private void SetupPath()
    {
#if UNITY_EDITOR
        string basePath = Application.dataPath;
#else
        string basePath = Application.persistentDataPath;
#endif

        saveDirectory = Path.Combine(basePath, folderName, subFolderName);

        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
            Debug.Log("Created directory: " + saveDirectory);
        }

        saveFilePath = Path.Combine(saveDirectory, fileName);
    }

    private void SetupPrefabMap()
    {
        prefabMap.Clear();
        foreach (var prefab in prefabList)
        {
            if (prefab != null && !prefabMap.ContainsKey(prefab.name))
            {
                prefabMap.Add(prefab.name, prefab);
            }
        }
    }

    private void OnApplicationQuit()
    {
        SaveSpawnedObjects();
    }

    // SAVE all children of parent
    public void SaveSpawnedObjects()
    {
        if (spawnedObjectsParent == null)
        {
            Debug.LogWarning("No parent assigned for spawned objects!");
            return;
        }

        SpawnedObjectsSave saveData = new SpawnedObjectsSave();

        foreach (Transform t in spawnedObjectsParent)
        {
            saveData.objects.Add(new SpawnedObjectData
            {
                prefabName = t.name.Replace("(Clone)", ""), // remove clone suffix
                position = t.position,
                rotation = t.rotation
            });
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log($"Saved {saveData.objects.Count} spawned objects to {saveFilePath}");
    }

    // LOAD objects
    public void LoadSpawnedObjects()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("No spawned objects save file found.");
            return;
        }

        string json = File.ReadAllText(saveFilePath);
        SpawnedObjectsSave saveData = JsonUtility.FromJson<SpawnedObjectsSave>(json);

        if (saveData == null || saveData.objects.Count == 0)
        {
            Debug.Log("No objects to load.");
            return;
        }

        // Clear existing objects
        foreach (Transform child in spawnedObjectsParent)
        {
            Destroy(child.gameObject);
        }

        // Instantiate objects from saved data
        foreach (var objData in saveData.objects)
        {
            // Try to load prefab from Resources folder
            GameObject prefab = Resources.Load<GameObject>(objData.prefabName);

            if (prefab != null)
            {
                GameObject spawned = Instantiate(prefab, objData.position, objData.rotation, spawnedObjectsParent);
                spawned.name = objData.prefabName; // remove clone suffix
            }
            else
            {
                Debug.LogWarning($"Prefab '{objData.prefabName}' not found in Resources folder. Skipping.");
            }
        }

        Debug.Log($"Loaded {saveData.objects.Count} spawned objects.");
    }
    // DELETE saved spawned objects
    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Spawned objects save deleted.");
        }
    }

    public string GetSavePath()
    {
        return saveFilePath;
    }
}