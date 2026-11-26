using UnityEditor;

[InitializeOnLoad]
public static class FixSdk
{
    static FixSdk()
    {
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
    }
}
