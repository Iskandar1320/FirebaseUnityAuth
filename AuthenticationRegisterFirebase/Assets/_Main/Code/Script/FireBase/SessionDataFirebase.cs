using Firebase.Auth;

public static class SessionDataFirebase
{
    public static FirebaseUser FirebaseUser;
    public static FirebaseUserProfileData CurrentProfile;

    public static bool IsLoggedIn()
    {
        return FirebaseUser != null;
    }

    public static void Clear()
    {
        FirebaseUser = null;
        CurrentProfile = null;
    }
}