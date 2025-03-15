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
    
    public void ShowMatchPage()
    {
        Destroy(currentPage);
        currentPage = Instantiate(matchPage, canvasRoot);
    }
}
