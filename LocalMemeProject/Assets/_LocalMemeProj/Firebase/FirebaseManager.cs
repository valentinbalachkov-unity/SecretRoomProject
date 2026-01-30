using Firebase.Auth;
using Firebase.Storage;
using Firebase.Extensions; 
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public FirebaseStorage Storage => _storage;
    
    private FirebaseAuth _auth;
    private FirebaseStorage _storage;


    private void Start()
    {
        // Инициализация
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == Firebase.DependencyStatus.Available) {
                InitializeFirebase();
            } else {
                Debug.LogError($"Firebase Error: {task.Result}");
            }
        });
    }

    private void InitializeFirebase()
    {
        _auth = FirebaseAuth.DefaultInstance;
        _storage = FirebaseStorage.DefaultInstance;
        
        // Анонимный вход (обязательно для доступа к Storage)
        if (_auth.CurrentUser == null) {
            _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(authTask => {
                if (!authTask.IsCanceled && !authTask.IsFaulted) {
                    Debug.Log($"Logged in as {authTask.Result.User.UserId}");
                }
            });
        }
    }
}
