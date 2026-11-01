using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>Assembles serialized Unity objects in the real Editor, never a second game simulation.</summary>
public static class ForgeborneSceneBuilder
{
    public const string MainPath = "Assets/Scenes/Main.unity";

    [MenuItem("Forgeborne/Create Main Scene")]
    public static void CreateMain()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play mode before creating Main.");
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainPath) != null)
            throw new InvalidOperationException("Main already exists. Open it; creation never overwrites authored work.");
        if (SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Save the current scene before creating Main.");

        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets", "Materials");
        ConfigureUrp();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var manager = new GameObject("GameManager").AddComponent<GameManager>();
        manager.Seed = 42;
        manager.MeshRenderer = new GameObject("Chunk").AddComponent<VoxelMeshRenderer>();
        new GameObject("World Actors").AddComponent<WorldActorDisplay>().Manager = manager;

        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.GetComponent<Camera>();
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 180f;
        camera.fieldOfView = 75f;
        cameraObject.transform.position = new Vector3(16f, 5.6f, 16f);
        manager.FpsAdapter = cameraObject.AddComponent<FpsInputAdapter>();
        // Leave MeshRenderer null here: GameManager's production null-fill is exercised on Play.
        cameraObject.AddComponent<WeaponViewModel>().CameraAnchor = cameraObject.transform;

        var canvasObject = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        var hud = canvasObject.AddComponent<HudDisplay>();
        var cyan = new Color(0.32f, 0.9f, 1f);
        var orange = new Color(1f, 0.64f, 0.27f);
        hud.HealthText = Line(canvasObject.transform, "Health", new Vector2(0, 1), new Vector2(38, -38), new Vector2(450, 45), 28, cyan);
        hud.ShieldsText = Line(canvasObject.transform, "Shields", new Vector2(0, 1), new Vector2(38, -82), new Vector2(450, 40), 23, cyan);
        hud.ObjectiveText = Line(canvasObject.transform, "Objective", Vector2.one, new Vector2(-38, -38), new Vector2(550, 95), 26, orange, TextAnchor.UpperRight);
        hud.AmmoText = Line(canvasObject.transform, "Ammo", new Vector2(1, 0), new Vector2(-38, 38), new Vector2(500, 100), 30, cyan, TextAnchor.LowerRight);
        hud.WarningText = Line(canvasObject.transform, "Power warning", new Vector2(0.5f, 1), new Vector2(0, -160), new Vector2(1150, 90), 22, orange, TextAnchor.UpperCenter);
        manager.HudDisplay = hud;
        Line(canvasObject.transform, "Crosshair", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24, 24), 22, Color.white, TextAnchor.MiddleCenter).text = "+";
        Line(canvasObject.transform, "Controls", Vector2.zero, new Vector2(38, 38), new Vector2(1000, 70), 20, new Color(0.78f, 0.84f, 0.88f), TextAnchor.LowerLeft).text = "WASD  MOVE    MOUSE  LOOK    SPACE  JUMP\nE  GATHER    Q  WALL    LMB  FIRE    R  RELOAD    O  SQUADS";
        manager.OrbitalDisplay = canvasObject.AddComponent<OrbitalSquadDisplay>();
        manager.OrbitalDisplay.SquadText = Line(canvasObject.transform, "Squads", new Vector2(0, 1), new Vector2(38, -145), new Vector2(700, 270), 19, cyan);
        var markers = new GameObject("Squad Markers");
        manager.MarkerDisplay = markers.AddComponent<SquadMarkerDisplay>();
        manager.MarkerDisplay.MarkerRoot = markers.transform;

        var lighting = new GameObject("Dusk Lighting").AddComponent<DuskLighting>();
        var sky = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DuskSky.mat");
        if (sky == null)
        {
            sky = new Material(Shader.Find("Skybox/Procedural")) { name = "Forgeborne Dusk" };
            sky.SetColor("_SkyTint", new Color(0.20f, 0.26f, 0.34f));
            sky.SetColor("_GroundColor", new Color(0.52f, 0.54f, 0.58f));
            sky.SetFloat("_Exposure", 0.9f);
            AssetDatabase.CreateAsset(sky, "Assets/Materials/DuskSky.mat");
        }
        lighting.SkyboxMaterial = sky;
        RenderSettings.skybox = sky;
        EditorSceneManager.SaveScene(scene, MainPath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MainPath, true) }
            .Concat(EditorBuildSettings.scenes.Where(s => s.path != MainPath)).ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("Forgeborne Main created in Unity: Seed 42, one GameManager, all five adapters linked. Play validation still required.");
    }

    [MenuItem("Forgeborne/Configure URP")]
    public static void ConfigureUrp()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play mode before configuring rendering.");
        const string pipelinePath = "Assets/Rendering/ForgebornePipeline.asset";
        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
        if (GraphicsSettings.defaultRenderPipeline != null && GraphicsSettings.defaultRenderPipeline != pipeline)
            throw new InvalidOperationException("A different pipeline is assigned. Review it before replacing it.");
        EnsureFolder("Assets", "Rendering");
        if (pipeline == null)
        {
            var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer, "Assets/Rendering/ForgeborneRenderer.asset");
            ResourceReloader.ReloadAllNullIn(renderer, UniversalRenderPipelineAsset.packagePath);
            pipeline = UniversalRenderPipelineAsset.Create(renderer);
            pipeline.name = "ForgebornePipeline";
            pipeline.msaaSampleCount = 2;
            pipeline.shadowDistance = 60f;
            AssetDatabase.CreateAsset(pipeline, pipelinePath);
        }
        GraphicsSettings.defaultRenderPipeline = pipeline;
        // Quality tiers inherit the project pipeline; no tier may silently fall back to built-in.
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            int previous = QualitySettings.GetQualityLevel();
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = pipeline;
            QualitySettings.SetQualityLevel(previous, false);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Forgeborne URP assigned. The prior built-in pipeline rendered URP Lit materials pink.");
    }

    private static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name);
    }

    private static Text Line(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size,
        int fontSize, Color color, TextAnchor alignment = TextAnchor.UpperLeft)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }
}
