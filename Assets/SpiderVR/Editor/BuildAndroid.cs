#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SpiderVR.Editor
{
    public static class BuildAndroid
    {
        [MenuItem("SpiderVR/Configurar Android (leve)")]
        public static void Configure()
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.spidervr.game");
            PlayerSettings.productName = "SpiderVR";
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);
            PlayerSettings.gpuSkinning = true;
            PlayerSettings.Android.androidIsGame = true;
            // shaders leves precisam estar no build (senao Shader.Find retorna null)
            var gs = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/GraphicsSettings.asset"));
            var arr = gs.FindProperty("m_AlwaysIncludedShaders");
            foreach (var n in new[] { "Mobile/Diffuse", "Sprites/Default" })
            {
                var sh = Shader.Find(n); if (sh == null) continue; bool has = false;
                for (int i = 0; i < arr.arraySize; i++) if (arr.GetArrayElementAtIndex(i).objectReferenceValue == sh) has = true;
                if (!has) { arr.InsertArrayElementAtIndex(arr.arraySize); arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = sh; }
            }
            gs.ApplyModifiedProperties();
            // corpo/maos pela camera ligado por padrao
            Debug.Log("SpiderVR: Android configurado.");
        }

        // usado pelo GitHub Actions: -executeMethod SpiderVR.Editor.BuildAndroid.Build
        public static void Build()
        {
            Configure();
            NetSetup.Run();
            SceneBuilder.Build();
            var opts = new BuildPlayerOptions { scenes = new[] { "Assets/Scenes/Menu.unity", "Assets/Scenes/SpiderVR_Game.unity" }, locationPathName = "build/Android/SpiderVR.apk", target = BuildTarget.Android, options = BuildOptions.None };
            var r = BuildPipeline.BuildPlayer(opts);
            if (r.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) EditorApplication.Exit(1);
        }

        [MenuItem("SpiderVR/Escalar cidade x2 (editor)")]
        public static void ScaleSel() { foreach (var t in Selection.transforms) { Undo.RecordObject(t, "scale"); t.localScale *= 2f; } }
    }
}
#endif
