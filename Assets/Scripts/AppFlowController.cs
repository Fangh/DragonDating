using System;
using UnityEngine;

public class AppFlowController : MonoBehaviour
{
    public static AppFlowController Instance;
    
    
    [Header("References")] 
    [SerializeField] private Transform canvasRoot;
    
    [SerializeField] private GameObject loginPage;
    
    [SerializeField] private GameObject subscriptionPage;

    [SerializeField] private GameObject matchPage;

    private GameObject currentPage;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPage = Instantiate(loginPage, canvasRoot);
    }

    public void ShowSubscriptionPage()
    {
        Destroy(currentPage);
        currentPage = Instantiate(subscriptionPage, canvasRoot);
    }
    
    public async void ShowMatchPage()
    {
        Destroy(currentPage);
        bool initializeSuccess = await MatchController.Instance.Initialize();
        if(!initializeSuccess)
        {
            Debug.LogError("Failed to initialize match controller.");
            return;
        }
        currentPage = Instantiate(matchPage, canvasRoot);
    }
}
