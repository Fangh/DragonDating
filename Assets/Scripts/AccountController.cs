using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

public class AccountController : MonoBehaviour
{
    public static AccountController Instance;
    
    private FirebaseAuth auth;
    
    private FirebaseUser currentUser;

    private void Awake()
    {
        Instance = this;
        auth = FirebaseAuth.DefaultInstance;
    }

    public string GetAccountID()
    {
        return currentUser.UserId;
    }

    /// <summary>
    /// https://firebase.google.com/docs/auth/unity/start#sign_up_new_users
    /// </summary>
    public async Task<AuthResult> CreateAccount(string _email, string _password)
    {
        Task<AuthResult> task = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
        await task;
            
        if (!task.IsCompletedSuccessfully) 
        {
            Debug.LogError("CreateUserWithEmailAndPasswordAsync didn't work.");
            Debug.LogException(task.Exception);
            return null;
        }

        // Firebase user has been created.
        Debug.Log($"Firebase user created successfully: {task.Result.User.DisplayName} ({task.Result.User.UserId})");
        currentUser = task.Result.User;
        return task.Result;
    }

    /// <summary>
    /// https://firebase.google.com/docs/auth/unity/start#sign_in_existing_users
    /// </summary>
    public async Task<AuthResult> Login(string _email, string _password)
    {
        Task<AuthResult> task = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        await task; 
        if (!task.IsCompletedSuccessfully) 
        {
            Debug.LogError("SignInWithEmailAndPasswordAsync didn't work.");
            Debug.LogException(task.Exception);
            return null;
        }

        Debug.Log($"User signed in successfully: {task.Result.User.DisplayName} ({task.Result.User.UserId})");
        currentUser = task.Result.User;
        return task.Result;
    }
}
