using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubscriptionView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField bioInputField;
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private TMP_Dropdown interestDropdown;
    [SerializeField] private Button pictureButton;
    [SerializeField] private Button confirmButton;

    public Texture2D userPicture = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        PhotoGalleryController.Instance.RequestPermissionAsynchronously(NativeGallery.PermissionType.Read, NativeGallery.MediaType.Image);
        confirmButton.onClick.AddListener(OnConfirm);
        pictureButton.onClick.AddListener(OnPictureButtonClicked);
    }

    void OnDisable()
    {
        confirmButton.onClick.RemoveAllListeners();
        pictureButton.onClick.RemoveAllListeners();
    }

    private async void OnConfirm()
    {
        if (!FieldsAreCorrect())
        {
            Debug.LogWarning("User hasn't fill all the values in the subscription fields.");
            return;
        }

        DragonModel userDragon = await DragonController.Instance.CreateNewDragon(
            nameInputField.text
            , bioInputField.text
            , GetSelectedDragonTypes(typeDropdown.value + 1).FirstOrDefault() //+1 because the first value is "None"
            , GetSelectedDragonTypes(interestDropdown.value).ToArray()
            , userPicture);

        if (userDragon == null)
        {
            Debug.LogError("Error when trying to create a new profile for the current dragon.");
            return;
        }


        Debug.Log("Subscription finished successfully.");
        DragonController.Instance.currentDragon = userDragon;
        AppFlowController.Instance.ShowMatchPage();
    }

    private List<EDragonType> GetSelectedDragonTypes(int dropdownValue)
    {
        List<EDragonType> selectedTypes = new List<EDragonType>();
    
        // Parcours chaque valeur d'énumération
        foreach (EDragonType dragonType in Enum.GetValues(typeof(EDragonType)))
        {
            // Vérifie si le bit correspondant à ce type est activé dans la valeur du dropdown
            if ((dropdownValue & (1 << ((int)dragonType - 1))) != 0)
            {
                selectedTypes.Add(dragonType);
            }
        }
    
        return selectedTypes;
    }

    private void OnPictureButtonClicked()
    {
        PickImage(_texture =>
        {
            userPicture = _texture;
            pictureButton.GetComponent<Image>().sprite = Sprite.Create(userPicture, new Rect(0, 0, userPicture.width, userPicture.height), Vector2.zero);
            //Debug.Log($"User has changed their pictures for {userPicture.name}");
        });
    }

    private void PickImage(Action<Texture2D> _callback)
    {
        NativeGallery.GetImageFromGallery(async (path) =>
        {
            Debug.Log("Image path: " + path);
            if (path == null)
            {
                Debug.LogError("Couldn't load texture from " + path);
                return;
            }

            // Create Texture from selected image
            Texture2D texture = await NativeGallery.LoadImageAtPathAsync(path, -1, false);
            if (texture == null)
            {
                Debug.Log("Couldn't load texture from " + path);
                return;
            }
            else
            {
                Debug.Log($"texture width = {texture.width}");
            }

            _callback?.Invoke(texture);
        });
    }

    /// <summary>
    /// Check all the fields in the subscription page before sending the form to firebase
    /// </summary>
    /// <returns>false if at least one field is not correct</returns>
    private bool FieldsAreCorrect()
    {
            // Validate name input field
            if (string.IsNullOrEmpty(nameInputField.text))
            {
                Debug.LogError("Name is required.");
                return false;
            }
        
            // Validate bio input field
            if (string.IsNullOrEmpty(bioInputField.text))
            {
                Debug.LogError("Bio is required.");
                return false;
            }
        
            // Validate type dropdown
            string selectedType = typeDropdown.value.ToString();
            if (string.IsNullOrEmpty(selectedType))
            {
                Debug.LogError("Type is required.");
                return false;
            }
        
            // Validate interest dropdown
            
            List<EDragonType> types = GetSelectedDragonTypes(interestDropdown.value);
            if (types.Count == 0)
            {
                Debug.LogError("At least one interest is required.");
                return false;
            }

            if (userPicture == null)
            {
                Debug.LogError("User picture is required.");
                return false;
            }

            return true;
    }

}
