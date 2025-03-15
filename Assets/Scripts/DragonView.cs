using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DragonView : MonoBehaviour
{
    [Header("References")] 
    [SerializeField]
    private TextMeshProUGUI _nameLabel;
    [SerializeField]
    private TextMeshProUGUI _bioLabel;
    [SerializeField]
    private TextMeshProUGUI _typeLabel;
    [SerializeField]
    private Image _profilePicture;

    
    public void UpdateView(DragonModel _model)
    {
        _nameLabel.text = _model.Name;
        _bioLabel.text = _model.Bio;
        _typeLabel.text = _model.Type.ToString();
        _profilePicture.sprite = Sprite.Create(_model.profilPicture, new Rect(0, 0, _model.profilPicture.width, _model.profilPicture.height), Vector2.zero);
    }
}
