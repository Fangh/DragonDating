using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Storage;

public class FireStorageController : MonoBehaviour
{
    public static FireStorageController Instance { get; private set; }
    private FirebaseStorage storage;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        FirebaseController.Instance.OnFirebaseInitialized += Initialize;
    }

    private void Initialize()
    {
        FirebaseController.Instance.OnFirebaseInitialized -= Initialize;
        storage = FirebaseStorage.DefaultInstance;
    }
    
    /// <summary>
    /// Upload the profile picture 
    /// </summary>
    /// <param name="_dragonID"></param>
    public async Task<bool> UploadPicture(string _dragonID, string _filePath)
    {
        StorageReference fileRef = storage.RootReference.Child($"{_dragonID}/profilePicture.jpg");
        
        Task<StorageMetadata> task = fileRef.PutFileAsync(_filePath);
        await task;
        
        if (task.IsFaulted || task.IsCanceled) 
        {
            // Uh-oh, an error occurred!
            Debug.LogError($"Error uploading. {task.Exception.Message}");
            return false;
        }

        // Metadata contains file metadata such as size, content-type, and download URL.
        Debug.Log("Finished uploading...");
        return true;
    }

    /// <summary>
    ///
    /// https://firebase.google.com/docs/storage/unity/upload-files#upload_from_data_in_memory
    /// </summary>
    /// <param name="_dragonID"></param>
    /// <param name="_texture"></param>
    /// <returns></returns>
    public async Task<bool> UploadPicture(string _dragonID, Texture2D _texture)
    {
        StorageReference fileRef = storage.RootReference.Child($"{_dragonID}/profilePicture.jpg");
        byte[] bytes = _texture.GetRawTextureData();
        Task<StorageMetadata> task = fileRef.PutBytesAsync(bytes);
        await task;
        
        if (task.IsFaulted || task.IsCanceled) 
        {
            // Uh-oh, an error occurred!
            Debug.LogError($"Error uploading. {task.Exception.Message}");
            return false;
        }

        // Metadata contains file metadata such as size, content-type, and download URL.
        Debug.Log("Finished uploading...");
        return true;
    }

    /// <summary>
    /// Download the profilePicture of a dragon and save it in the local storage
    /// </summary>
    /// <param name="_dragonID"></param>
    /// <returns></returns>
    public async Task<string> DownloadPicture(string _dragonID)
    {
        StorageReference fileRef = storage.RootReference.Child($"{_dragonID}/profilePicture.jpg");
        
        // Create local filesystem URL
        string localUrl = $"file:///local/{_dragonID}/profilePicture.jpg";

        // Download to the local filesystem
        Task task = fileRef.GetFileAsync(localUrl);
        await task;

        if (!task.IsFaulted && !task.IsCanceled) 
            return localUrl;
        
        Debug.LogError($"File download error. {task.Exception.Message}");
        return null;
    }

}
