using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARController : MonoBehaviour
{
    
    public static ARController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private ARSession session;
    [SerializeField] private ARRaycastManager raycastManager;

    InputAction m_PressAction;
    bool m_Pressed = false;
    List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    private void Awake()
    {
        Instance = this;
        m_PressAction = new InputAction("touch", binding: "<Pointer>/press");
        m_PressAction.performed += OnPress;
        m_PressAction.canceled += OnRelease;
    }

    private void OnRelease(InputAction.CallbackContext context)
    {
        m_Pressed = false;
    }

    private void OnPress(InputAction.CallbackContext context)
    {
        m_Pressed = true;
    }

    private void Start()
    {
        StartCoroutine(InitializeAR());

    }

    private void OnEnable()
    {
        m_PressAction.Enable();
    }

    private void OnDisable()
    {
        m_PressAction.Disable();
    }

    private IEnumerator InitializeAR()
    {
        if ((ARSession.state == ARSessionState.None) ||
            (ARSession.state == ARSessionState.CheckingAvailability))
        {
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            Debug.LogError("AR is not supported on this device");
            // Start some fallback experience for unsupported devices
        }
        else
        {
            // Start the AR session
            Debug.Log("Starting AR session");
            session.enabled = true;
        }
    }


    public async void CloseAR()
    {
        MiniatureController.Instance.DestroyCurrentMiniature();
        await AppFlowController.Instance.ToggleARScene(false);
    }

    /// <summary>
    /// If Miniatue is not spawned, then wait for the user to touch the screen then spawn the miniature at the hit position.
    /// </summary>
    private void Update()
    {
        if (MiniatureController.Instance.isMiniatureSpawned)
            return;

        if (Pointer.current == null || m_Pressed == false)
            return;

        var touchPosition = Pointer.current.position.ReadValue();

        if (raycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinBounds))
        {
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            MiniatureController.Instance.SpawnMiniatureAt(s_Hits[0].pose.position);
        }
    }
}
