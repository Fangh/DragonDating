using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DragonView : MonoBehaviour
{
    
    [Header("References")] 
    [SerializeField]
    private TextMeshProUGUI nameLabel;
    [SerializeField]
    private TextMeshProUGUI bioLabel;
    [SerializeField]
    private TextMeshProUGUI typeLabel;
    [SerializeField]
    private Image profilePicture;
    [SerializeField]
    private Button profilePictureButton;

    public string id;

    private string miniaturePath;
    
    private void OnEnable()
    {
        profilePictureButton.onClick.AddListener(OnProfilePictureClicked);
    }
    
    private void OnDisable()
    {
        profilePictureButton.onClick.RemoveAllListeners();
    }
    
    private void OnProfilePictureClicked()
    {
        DragonController.Instance.SetMiniatureToSpawn(miniaturePath);
    }

    /// <summary>
    /// Update the view (name, bio, type and picture) with the data of the model
    /// </summary>
    /// <param name="_model"></param>
    public void UpdateView(DragonModel _model)
    {
        id = _model.Id;
        nameLabel.text = _model.Name;
        bioLabel.text = _model.Bio;
        typeLabel.text = _model.Type.ToString();
        profilePicture.sprite = Sprite.Create(_model.profilPicture, new Rect(0, 0, _model.profilPicture.width, _model.profilPicture.height), Vector2.zero);
        miniaturePath = _model.miniatureGLBLocalPath;
    }
}
