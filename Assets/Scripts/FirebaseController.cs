using System;
using Firebase;
using UnityEngine;
using Firebase.Extensions;

public class FirebaseController : MonoBehaviour
{
    public static FirebaseController Instance;
    public event Action OnFirebaseInitialized;
    
    private FirebaseApp app;


    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
          var dependencyStatus = task.Result;
          if (dependencyStatus == DependencyStatus.Available) 
          {
            // Create and hold a reference to your FirebaseApp,
            // where app is a Firebase.FirebaseApp property of your application class.
           app = FirebaseApp.DefaultInstance;
        
            // Set a flag here to indicate whether Firebase is ready to use by your app.
            OnFirebaseInitialized?.Invoke();
          } 
          else 
          {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            // Firebase Unity SDK is not safe to use here.
          }
        });

    }

}
