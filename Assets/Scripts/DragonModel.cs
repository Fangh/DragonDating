using System.IO;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class DragonModel
{
    [FirestoreDocumentId]
    public string Id { get; set; }

    [FirestoreProperty]
    public string Name { get; set; }

    [FirestoreProperty]
    public EDragonType Type { get; set; }

    [FirestoreProperty]
    public EDragonType[] InterestedTypes { get; set; }

    [FirestoreProperty]
    public string Bio { get; set; }
    
    [FirestoreProperty]
    public EStatus Status { get; set; }

    public Texture2D profilPicture;
    
    public string miniatureGLBLocalPath;

    public DragonModel()
    {
        Id = null;
        Name = null;
        Type = EDragonType.None;
        InterestedTypes = new []{EDragonType.None};
        Bio = null;
    }

    /// <summary>
    /// Use this constructor to create a model if you already have all the data of a Dragon (for example at subscription)
    /// </summary>
    /// <param name="_id"></param>
    /// <param name="_name"></param>
    /// <param name="_type"></param>
    /// <param name="_interestedTypes"></param>
    /// <param name="_bio"></param>
    /// <param name="_profilePicture"></param>
    public DragonModel(string _id, string _name, EDragonType _type, EDragonType[] _interestedTypes, string _bio, Texture2D _profilePicture)
    {
        Id = _id;
        Name = _name;
        Type = _type;
        InterestedTypes = _interestedTypes;
        Bio = _bio;
        profilPicture = _profilePicture;
    }

    /// <summary>
    /// Use this contructor to create an empty model. You may use Read() after to fill the model.
    /// </summary>
    /// <param name="_id"></param>
    public DragonModel(string _id)
    {
        Id = _id;
    }

    /// <summary>
    /// CRUD operation to Create this DragonModel into the Firestore (database)
    /// https://firebase.google.com/docs/firestore/manage-data/add-data?hl=en#custom_objects
    /// </summary>
    public async Task<bool> Create()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("dragons").Document(AccountController.Instance.GetAccountID());

        Task task = docRef.SetAsync(this);
        await task;
        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Cannot create a dragon document in the firestore");
            Debug.LogException(task.Exception);
            return false;
        }
        Debug.Log($"Dragon {Name} created successfully in the firestore");
        return true;
    }

    /// <summary>
    /// CRUD operation to Read this DragonModel from the Firestore (database).
    /// </summary>
    public async Task<bool> Read()
    {
        if(string.IsNullOrEmpty(this.Id))
        {
            Debug.LogError("Cannot read a Dragon without an ID");
            return false;
        }

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("dragons").Document(this.Id);
        Task<DocumentSnapshot> task = docRef.GetSnapshotAsync();
        await task;
        if (!task.IsCompletedSuccessfully)
        {
            Debug.LogError($"Cannot read Dragon {this.Id} in the database");
            Debug.LogException(task.Exception);
            return false;
        }
        DocumentSnapshot snapshot = task.Result;
        DragonModel model = snapshot.ConvertTo<DragonModel>();
        this.Name = model.Name;
        this.Bio = model.Bio;
        this.Type = model.Type;
        this.InterestedTypes = model.InterestedTypes;
        this.Status = model.Status;
        
        if(Status >= EStatus.AccountStarted)
            this.profilPicture = await ReadProfilePicture(this.Id);
        
        if(Status >= EStatus.AccountCompleted)
            this.miniatureGLBLocalPath = await Read3DMiniature(this.Id);
        
        //Debug.Log($"Dragon {this.Id} has been downloaded from Firebase and is ready to be used in the app");
        return true;
    }


    /// <summary>
    /// CRUD operation to Update this DragonModel into the Firestore (database)
    /// </summary>
    public void Update()
    {
        
    }

    /// <summary>
    /// CRUD operation to Destroy this DragonModel into the Firestore (database)
    /// </summary>
    public void Destroy()
    {
        
    }


    /// <summary>
    /// Check first if the profile picture is already in the local storage, if not, download it from the Firestore.
    /// </summary>
    /// <param name="_dragonID"></param>
    /// <returns></returns>
    private async Task<Texture2D> ReadProfilePicture(string _dragonID)
    {
        string localPath = Path.Combine(Application.persistentDataPath, _dragonID, "profilePicture.png");

        //if file doesn't exist, download it from the firestorage and save it locally
        if (!File.Exists(localPath))
        {
            Debug.Log($"Profile picture of {_dragonID} not found in local storage, downloading it first from the firestorage");
            await FireStorageController.Instance.DownloadPicture(_dragonID);
        }

        //load the image from the local storage
        //Debug.Log($"Loading the profile picture of {_dragonID} from the local storage");
        return await LoadProfilePicture(_dragonID);
    }
    
    /// <summary>
    /// Check first if the GLB 3D Model is already in the local storage, if not, download it from the Firestore.
    /// </summary>
    /// <param name="_dragonID"></param>
    /// <returns>The path in the local storage where the GLB is stored</returns>
    private async Task<string> Read3DMiniature(string _dragonID)
    {
        string localPath = Path.Combine(Application.persistentDataPath, _dragonID, "model.glb");

        //if file doesn't exist, download it from the firestorage and save it locally
        if (!File.Exists(localPath))
        {
            Debug.Log($"model.glb of {_dragonID} not found in local storage, downloading it first from the firestorage");
            await FireStorageController.Instance.Download3DModel(_dragonID);
        }

        //load the image from the local storage
        //Debug.Log($"Loading the profile picture of {_dragonID} from the local storage");
        return localPath;
    }

    public async Task<bool> SaveProfilePicture(Texture2D _texture, string _dragonID)
    {
        Task<bool> task = SaveProfilePicture(_texture.EncodeToPNG(), _dragonID);
        await task;
        return task.Result;
    }

    /// <summary>
    /// This is an asynchronous method that saves a profile picture to local storage on the device.
    /// </summary>
    /// <param name="_textureAsBytes">The byte array containing the image data.</param>
    /// <param name="_dragonID">A unique identifier for the dragon associated with the profile picture.</param>
    /// <returns></returns>
    public async Task<bool> SaveProfilePicture(byte[] _textureAsBytes, string _dragonID)
    {
        string localPath = Path.Combine(Application.persistentDataPath, _dragonID, "profilePicture.png");

        if(!Directory.Exists(Path.GetDirectoryName(localPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(localPath));
        }

        Task task = File.WriteAllBytesAsync(localPath, _textureAsBytes);
        await task;
        if (!task.IsCompletedSuccessfully)
        {
            Debug.LogError("Error saving the profile picture");
            Debug.LogException(task.Exception);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Load the profile picture as a file from the local storage in the device into a Texture3D in the memory.
    /// </summary>
    /// <param name="_dragonID"></param>
    /// <returns></returns>
    private async Task<Texture2D> LoadProfilePicture(string _dragonID)
    {
        string localPath = Path.Combine(Application.persistentDataPath, _dragonID, "profilePicture.png");
        Task<byte[]> task = File.ReadAllBytesAsync(localPath);
        await task;
        if (!task.IsCompletedSuccessfully)
        {
            Debug.LogError("Error loading the profile picture");
            Debug.LogException(task.Exception);
            return null;
        }

        Texture2D tex = new(1024, 1024);
        tex.LoadImage(task.Result);
        return tex;
    }
    
    
}

public enum EDragonType
{
    None = 0,
    Fire = 1,
    Ice = 2,
    Earth = 3,
    Wind = 4,
    Aquatic = 5,
    Electric = 6,
    Poison = 7,
    Light = 8,
    Darkness = 9,
    Storm = 10
}

public enum EStatus
{
    AccountNotCompleted = 0, //A profile have this status only when the user have a Firebase Account but no profile in the Firestore
    AccountStarted = 1, //A profile have this status only when the user have completed all the fields of the profile and the Firestore document is ready
    AccountCompleted = 100, //A profile have this status only when the Meshy API have created their 3D model and stored in the Firestorage
}
