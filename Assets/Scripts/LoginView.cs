using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginView : MonoBehaviour
{
    [Header("References")] 
    
    [SerializeField]
    private Button loginButton;
    
    [SerializeField]
    private Button signUpButton;
    
    [SerializeField]
    private TMP_InputField emailInputField;
    
    [SerializeField]
    private TMP_InputField passwordInputField;
    
    private void OnEnable()
    {
        signUpButton.onClick.AddListener(OnSignUpClicked);
        loginButton.onClick.AddListener(OnLoginClicked);
    }
    
    private void OnDisable()
    {
        signUpButton.onClick.RemoveAllListeners();
        loginButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Handles the sign-up button click event.
    /// Validates the input fields and attempts to create a new account.
    /// If the sign-up is successful, navigates to the subscription page.
    /// </summary>
    private async void OnSignUpClicked()
    {
        bool signUpResult = await SignUp();
        if (!signUpResult)
        {
            Debug.LogError("Sign up failed", this);
            return;
        }
        
        Debug.Log("Sign up successful", this);
        AppFlowController.Instance.ShowSubscriptionPage();
    }

    /// <summary>
    /// Handles the login button click event.
    /// Validates the input fields and attempts to log in.
    /// If the login is successful, retrieves the current user's dragon profile and navigates to the match page.
    /// </summary>
    private async void OnLoginClicked()
    {
        bool loginResult = await Login();
        if (!loginResult)
        {
            Debug.LogError("Login failed", this);
            return;
        }

        DragonModel currentDragon = await DragonController.Instance.GetDragon(AccountController.Instance.GetAccountID());
        DragonController.Instance.currentDragon = currentDragon;

        Debug.Log("Login successful", this);
        AppFlowController.Instance.ShowMatchPage();
    }

    private async Task<bool> SignUp()
    {
        if (string.IsNullOrEmpty(emailInputField.text) || string.IsNullOrEmpty(passwordInputField.text))
        {
            Debug.LogError("Please fill all fields");
            return false;
        }
        Task task = AccountController.Instance.CreateAccount(emailInputField.text, passwordInputField.text);
        await task;
        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogException(task.Exception);
        }
        Debug.Log("Sign up successful");
        return true;
    }

    private async Task<bool> Login()
    {
        if (string.IsNullOrEmpty(emailInputField.text) || string.IsNullOrEmpty(passwordInputField.text))
        {
            Debug.LogError("Please fill all fields");
            return false;
        }
        Task task = AccountController.Instance.Login(emailInputField.text, passwordInputField.text);
        await task;
        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogException(task.Exception);
        }
        Debug.Log("Login successful");
        return true;
    }

}
