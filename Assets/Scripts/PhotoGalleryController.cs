using UnityEngine;

public class PhotoGalleryController : MonoBehaviour
{
    public static PhotoGalleryController Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
    }
    
    public async void RequestPermissionAsynchronously( NativeGallery.PermissionType permissionType, NativeGallery.MediaType mediaTypes )
    {
        NativeGallery.Permission permission = await NativeGallery.RequestPermissionAsync( permissionType, mediaTypes );
        Debug.Log( "Permission result: " + permission );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
