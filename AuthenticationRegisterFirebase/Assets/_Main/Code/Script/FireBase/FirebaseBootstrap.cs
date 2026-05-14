using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseBootstrap : MonoBehaviour
{
    public static FirebaseAuth Auth { get; private set; }
    public static FirebaseDatabase Database { get; private set; }
    public static DatabaseReference RootReference { get; private set; }
    public static bool IsReady { get; private set; }

    [SerializeField] private UIManagerFireBase uiManager;

    private const string DatabaseUrl = "https://fir-auth-93a31-default-rtdb.firebaseio.com/";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                Auth = FirebaseAuth.DefaultInstance;

                Database = FirebaseDatabase.GetInstance(DatabaseUrl);
                RootReference = Database.RootReference;

                IsReady = true;

                Debug.Log("Firebase inicializado correctamente con Realtime Database.");

                if (uiManager != null)
                {
                    uiManager.ShowStatus("Firebase listo.", MessageTypeFireBase.Success);
                }
            }
            else
            {
                IsReady = false;

                Debug.LogError("No se pudieron resolver las dependencias de Firebase: " + dependencyStatus);

                if (uiManager != null)
                {
                    uiManager.ShowStatus("Error inicializando Firebase.", MessageTypeFireBase.Error);
                }
            }
        });
    }
}