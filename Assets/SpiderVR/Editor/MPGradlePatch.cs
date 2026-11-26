#if UNITY_EDITOR
using System.IO;
using UnityEditor.Android;

namespace SpiderVR.Editor
{
    /// <summary>Adiciona a biblioteca MediaPipe Tasks (baixada do Maven Central durante o build) ao projeto Gradle gerado pelo Unity.</summary>
    public class MPGradlePatch : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 999;
        public void OnPostGenerateGradleAndroidProject(string unityLibraryPath)
        {
            File.AppendAllText(Path.Combine(unityLibraryPath, "build.gradle"),
                "\ndependencies {\n    implementation 'com.google.mediapipe:tasks-vision:0.10.14'\n}\n");
            var root = Directory.GetParent(unityLibraryPath).FullName;
            var rb = Path.Combine(root, "build.gradle");
            if (File.Exists(rb)) File.AppendAllText(rb, "\nallprojects {\n    repositories {\n        google()\n        mavenCentral()\n    }\n}\n");
            File.AppendAllText(Path.Combine(root, "gradle.properties"), "\nandroid.useAndroidX=true\nandroid.enableJetifier=true\n");
        }
    }
}
#endif
