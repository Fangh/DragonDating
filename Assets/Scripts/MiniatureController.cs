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
    
    private void Awake()
    {
        Instance = this;
    }
    
    public void DestroyCurrentMiniature()
    {
        if (currentMiniature != null)
        {
            Destroy(currentMiniature);
        }
    }

    public async Task<GameObject> SpawnMiniature(DragonModel _model)
    {
        return await SpawnMiniature(_model.miniatureGLBLocalPath);
    }

    /// <summary>
    /// Load the GLB 3D Model as a file from the local storage in the device and spawn it into the scene.
    /// </summary>
    /// <param name="_localPath">The path of the 3DModel in the local storage</param>
    /// <returns>The gameobject of the spawned miniature</returns>
    public async Task<GameObject> SpawnMiniature(string _localPath)
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
        return currentMiniature;
    }
}
