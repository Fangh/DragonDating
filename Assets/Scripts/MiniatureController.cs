using System;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

public class MiniatureController : MonoBehaviour
{
    public static MiniatureController Instance;
    
    [SerializeField] private Transform miniatureParent;

    private GameObject currentMiniature;
    public bool isMiniatureSpawned = false;

    private void Awake()
    {
        Instance = this;
        isMiniatureSpawned = false;
    }
    
    public void DestroyCurrentMiniature()
    {
        if (currentMiniature != null)
        {
            Destroy(currentMiniature);
        }
        isMiniatureSpawned = false;
    }


    /// <summary>
    /// Set the position of the spawned miniature to the given position and make it visible.
    /// </summary>
    /// <param name="_position"></param>
    [ContextMenu("Force Spawn")]
    public void SpawnMiniatureAt(Vector3 _position)
    {
        miniatureParent.transform.position = _position;
        currentMiniature.gameObject.SetActive(true);
        isMiniatureSpawned = true;
        Debug.Log($"Miniature has been spawned at {_position}");
    }

    /// <summary>
    /// Load the GLB 3D Model as a file from the local storage in the device and load it into the scene disblaed and at the origin of the scene.
    /// </summary>
    /// <param name="_localPath">The path of the 3DModel in the local storage</param>
    /// <returns>The gameobject of the spawned miniature</returns>
    public async Task<GameObject> LoadMiniature(string _localPath)
    {
        Task<byte[]> readFromLocalTask = File.ReadAllBytesAsync(_localPath);
        try
        {
            await readFromLocalTask;
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading the 3D Model from local Path");
            Debug.LogException(e);
            throw;
        }
        GltfImport importer = new();
        try
        {
            await importer.LoadGltfBinary(readFromLocalTask.Result,new Uri(_localPath));
            await importer.InstantiateMainSceneAsync(miniatureParent);
        }
        catch (Exception e)
        {
            Debug.LogError($"GLTF Importer didn't succeed to load the 3D Model");
            Debug.LogException(e);
            throw;
        }
        currentMiniature = miniatureParent.GetChild(0).gameObject;
        currentMiniature.transform.localPosition = Vector3.zero;
        currentMiniature.gameObject.SetActive(false);
        return currentMiniature;
    }
}
