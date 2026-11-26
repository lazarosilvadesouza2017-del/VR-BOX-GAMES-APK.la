#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

namespace SpiderVR.Editor
{
    /// <summary>
    /// MediaPipe (maos/corpo pela camera) e OPCIONAL e vem DESLIGADO: o primeiro APK precisa compilar sem ele.
    /// Ligar: menu SpiderVR > MediaPipe > Ativar (copia o .java para Plugins/Android e adiciona a biblioteca no Gradle).
    /// </summary>
    public class MPGradlePatch : IPostGenerateGradleAndroidProject
    {
        const string Src = "Assets/SpiderVR/MediaPipe~/MPBridge.java", Dst = "Assets/Plugins/Android/MPBridge.java";
        public static bool Enabled => File.Exists(Dst);

        [MenuItem("SpiderVR/MediaPipe/Ativar")]
        public static void Enable() { Directory.CreateDirectory("Assets/Plugins/Android"); File.Copy(Src, Dst, true); AssetDatabase.Refresh(); Debug.Log("MediaPipe ATIVADO."); }

        [MenuItem("SpiderVR/MediaPipe/Desativar")]
        public static void Disable() { if (File.Exists(Dst)) { AssetDatabase.DeleteAsset(Dst); AssetDatabase.Refresh(); } Debug.Log("MediaPipe DESATIVADO."); }

        public int callbackOrder => 999;
        public void OnPostGenerateGradleAndroidProject(string unityLibraryPath)
        {
            if (!Enabled) return;
            File.AppendAllText(Path.Combine(unityLibraryPath, "build.gradle"), "\ndependencies {\n    implementation 'com.google.mediapipe:tasks-vision:0.10.14'\n}\n");
            var root = Directory.GetParent(unityLibraryPath).FullName;
            var rb = Path.Combine(root, "build.gradle");
            if (File.Exists(rb)) File.AppendAllText(rb, "\nallprojects {\n    repositories {\n        google()\n        mavenCentral()\n    }\n}\n");
            File.AppendAllText(Path.Combine(root, "gradle.properties"), "\nandroid.useAndroidX=true\nandroid.enableJetifier=true\n");
        }
    }
}
#endif
