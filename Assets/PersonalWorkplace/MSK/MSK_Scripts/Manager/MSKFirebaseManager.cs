using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Unity.VisualScripting;
using VContainer.Unity;

public class MSKFirebaseManager : IStartable
{
    private static FirebaseAuth auth;
    public static FirebaseAuth Auth { get { return auth; } }

    private static FirebaseApp app;
    public static FirebaseApp App { get { return app; } }

    private static FirebaseDatabase database;
    public static FirebaseDatabase Database { get { return database; } }

    public void Start() { }

    private void Init()
    {

    }
}
