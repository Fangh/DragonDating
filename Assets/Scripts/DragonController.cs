using Firebase.Firestore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    public static DragonController Instance;

    /// <summary>
    /// This is the infos about the actual user of the app
    /// </summary>
    public DragonModel currentDragon;
    
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Generate a new Dragon and save it into the Firestore (database)
    /// Also save the picture in the Storage
    /// </summary>
    /// <param name="_name">The name of the dragon</param>
    /// <param name="_bio">The biography of the dragon</param>
    /// <param name="_type">The type of the dragon</param>
    /// <param name="_interestedTypes">The types of dragons this dragon is interested in</param>
    /// <param name="_profilePicture">The profile picture of the dragon</param>
    /// <returns>Returns the Model if the dragon was created successfully, otherwise null</returns>
    public async Task<DragonModel> CreateNewDragon(string _name, string _bio, EDragonType _type, EDragonType[] _interestedTypes, Texture2D _profilePicture)
    {
        string id = AccountController.Instance.GetAccountID();

        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError("Invalid account ID");
            return null;
        }

        Debug.Log($"Creating a new dragon profile for id {id}");
        DragonModel newDragon = new(
            AccountController.Instance.GetAccountID(),
            _name,
            _type,
            _interestedTypes,
            _bio,
            _profilePicture);

        try
        {
            bool uploadPictureResult = await FireStorageController.Instance.UploadPicture(id, _profilePicture);
            if (!uploadPictureResult)
            {
                Debug.LogError("Error creating a new dragon, because the upload of the profile picture has an issue", this);
                return null;
            }

            bool localSaveResult = await newDragon.SaveProfilePicture(_profilePicture, id);
            if (!localSaveResult)
            {
                Debug.LogError("Error creating a new dragon, because the caching of the profile picture has an issue", this);
                return null;
            }


            Debug.Log("Profile Picture has been uploaded & saved locally, now uploading everything to firebase...");
            bool createDocResult = await newDragon.Create();
            if (!createDocResult)
            {
                Debug.LogError("Error creating a new dragon, because the creation of the document has an issue", this);
                return null;
            }

            Debug.Log($"Dragon {_name} with id {newDragon.Id} profile has been successfully created");
            return newDragon;
        }
        catch (Exception ex)
        {
            Debug.LogError("An error occurred while creating a new dragon", this);
            Debug.LogException(ex);
            return null;
        }

    }

    /// <summary>
    /// Get the infos of a dragon from the Firestore.
    /// Will also download the profile picture if it's not present in local storage (cache).
    /// Then return the dragon model when the picture is present and downloaded
    /// </summary>
    /// <param name="_id">the ID of the dragon you want</param>
    /// <returns>The dragon model</returns>
    public async Task<DragonModel> GetDragon(string _id)
    {
        DragonModel _model = new DragonModel(_id);
        Task readTask = _model.Read();
        await readTask;
        if (!readTask.IsCompletedSuccessfully)
        {
            Debug.LogError("Error reading the dragon profile", this);
            Debug.LogException(readTask.Exception);
            return null;
        }
        return _model;
    }

    /// <summary>
    /// Download all Dragons Document from Firestore
    /// </summary>
    /// <returns></returns>
    public async Task<List<DragonModel>> ListAllDragons()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        Task<QuerySnapshot> task = db.Collection("dragons").GetSnapshotAsync();
        await task;
        
        if(!task.IsCompletedSuccessfully)
        {
            Debug.LogError("Error reading dragons collection");
            Debug.LogException(task.Exception);
            return null;
        }

        QuerySnapshot snapshot = task.Result;
        List<DragonModel> dragons = new();
        Debug.Log("Found " + snapshot.Count + " dragons in Firestore");
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            DragonModel model = new DragonModel(document.Id);
            await model.Read();
            dragons.Add(model);
        }
        return dragons;
    }

}


