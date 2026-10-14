using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public static class Taller2Builder
{
    private const string ScenesPath = "Assets/Scenes/";
    private const string TilesSheet = "Assets/Sprites/Environment/Floor_Tiles.png";
    private const string BackgroundSprite = "Assets/Sprites/UI/MenuBackground.png";
    private const string ButtonSprite = "Assets/Sprites/UI/Button.png";
    private const string IdleSheet = "Assets/Sprites/Soldier_Idle.png";
    private const string MovementController = "Assets/Animations/Player/PlayerMovement.controller";

    [MenuItem("Taller2/Construir todo")]
    public static void BuildAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        AnimatorController controller = BuildMovementController();
        BuildMainMenu();
        BuildGameScene(controller);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenesPath + "MainMenu.unity", true),
            new EditorBuildSettingsScene(ScenesPath + "GameScene.unity", true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(ScenesPath + "MainMenu.unity");
        EditorUtility.DisplayDialog("Taller 2", "Listo. Escenas MainMenu y GameScene creadas y registradas en Build Settings.", "OK");
    }

    private static Font GetFont()
    {
#if UNITY_2022_2_OR_NEWER
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
    }

    private static GameObject NewUI(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Text CreateText(Transform parent, string name, string content, int size, Color color, TextAnchor anchor, Vector2 pos, Vector2 dim)
    {
        GameObject go = NewUI(name, parent);
        Text t = go.AddComponent<Text>();
        t.font = GetFont();
        t.text = content;
        t.fontSize = size;
        t.color = color;
        t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = dim;
        return t;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 pos)
    {
        GameObject go = NewUI(name, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(460, 110);

        Image img = go.AddComponent<Image>();
        img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSprite);
        img.type = Image.Type.Sliced;

        Button b = go.AddComponent<Button>();
        b.targetGraphic = img;
        ColorBlock colors = b.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.93f, 0.7f, 1f);
        colors.pressedColor = new Color(0.72f, 0.62f, 0.45f, 1f);
        colors.selectedColor = Color.white;
        b.colors = colors;

        Text t = CreateText(go.transform, "Text", label, 52, new Color(0.25f, 0.15f, 0.08f, 1f), TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
        t.fontStyle = FontStyle.Bold;
        Stretch(t.GetComponent<RectTransform>());
        return b;
    }

    private static void BuildMainMenu()
    {
        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        cam.orthographic = true;
        camGo.AddComponent<AudioListener>();
        camGo.transform.position = new Vector3(0f, 0f, -10f);

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        GameObject canvasGo = new GameObject("Canvas", typeof(RectTransform));
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject bgGo = NewUI("BackgroundImage", canvasGo.transform);
        Image bg = bgGo.AddComponent<Image>();
        bg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundSprite);
        bg.preserveAspect = false;
        Stretch(bgGo.GetComponent<RectTransform>());

        Text title = CreateText(canvasGo.transform, "TitleText", "EL CABALLERO Y\nLOS PERGAMINOS", 120, new Color(1f, 0.85f, 0.4f, 1f), TextAnchor.MiddleCenter, new Vector2(0f, 300f), new Vector2(1600f, 320f));
        title.fontStyle = FontStyle.Bold;
        Outline outline = title.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.08f, 0.05f, 1f);
        outline.effectDistance = new Vector2(6f, -6f);

        GameObject menuPanel = NewUI("MenuPanel", canvasGo.transform);
        Stretch(menuPanel.GetComponent<RectTransform>());
        Button play = CreateButton(menuPanel.transform, "Btn_Play", "JUGAR", new Vector2(0f, -40f));
        Button credits = CreateButton(menuPanel.transform, "Btn_Credits", "CRÉDITOS", new Vector2(0f, -170f));
        Button quit = CreateButton(menuPanel.transform, "Btn_Quit", "SALIR", new Vector2(0f, -300f));

        GameObject creditsPanel = NewUI("CreditsPanel", canvasGo.transform);
        Image panelImg = creditsPanel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.85f);
        Stretch(creditsPanel.GetComponent<RectTransform>());

        Text creditsTitle = CreateText(creditsPanel.transform, "Titulo", "INFORMACIÓN Y CRÉDITOS", 70, new Color(1f, 0.85f, 0.4f, 1f), TextAnchor.MiddleCenter, new Vector2(0f, 440f), new Vector2(1500f, 110f));
        creditsTitle.fontStyle = FontStyle.Bold;

        string body =
            "EL CABALLERO Y LOS PERGAMINOS\n" +
            "Versión Prototipo - Semestre II / 2026\n\n" +
            "1. PROPÓSITO EDUCATIVO Y COMPETENCIAS\n" +
            "- Asignatura / Área: Historia Medieval\n" +
            "- Competencia formativa: el jugador explora unas ruinas y recolecta pergaminos con datos históricos de la Edad Media, reforzando su conocimiento de forma lúdica.\n\n" +
            "2. EQUIPO DE DESARROLLO\n" +
            "- Diseño, programación, arte e interfaces: Kevin Santiago Díaz\n\n" +
            "3. FUENTES Y REFERENCIAS ACADÉMICAS\n" +
            "- Guía del Taller Práctico 2 - UTS, Desarrollo de Contenidos Digitales y Multimediales.\n" +
            "- Documentación oficial de Unity (Tilemaps, Animación, Scripting API).\n\n" +
            "4. ATRIBUCIÓN DE RECURSOS DE TERCEROS\n" +
            "- Arte y sprites: elaborados para este prototipo.\n\n" +
            "AVISO LEGAL Y ACADÉMICO: prototipo interactivo desarrollado con fines formativos y académicos. No posee fines comerciales ni lucrativos. Todos los derechos de los recursos externos pertenecen a sus respectivos creadores.";
        Text bodyText = CreateText(creditsPanel.transform, "Credito", body, 28, Color.white, TextAnchor.UpperLeft, new Vector2(0f, -10f), new Vector2(1600f, 740f));

        Button close = CreateButton(creditsPanel.transform, "Btn_CloseCredits", "VOLVER", new Vector2(0f, -440f));

        GameObject managerGo = new GameObject("MenuManager");
        MainMenuManager manager = managerGo.AddComponent<MainMenuManager>();
        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("creditsPanel").objectReferenceValue = creditsPanel;
        so.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddPersistentListener(play.onClick, manager.PlayGame);
        UnityEventTools.AddPersistentListener(credits.onClick, manager.OpenCredits);
        UnityEventTools.AddPersistentListener(quit.onClick, manager.QuitGame);
        UnityEventTools.AddPersistentListener(close.onClick, manager.CloseCredits);

        creditsPanel.SetActive(false);

        EditorSceneManager.SaveScene(scene, ScenesPath + "MainMenu.unity");
    }

    private static AnimatorController BuildMovementController()
    {
        AssetDatabase.DeleteAsset(MovementController);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(MovementController);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Player/Player_Idle.anim");
        AnimationClip walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Player/Player_Walk.anim");

        AnimatorState idle = sm.AddState("Player_Idle");
        idle.motion = idleClip;
        AnimatorState walk = sm.AddState("Player_Walk");
        walk.motion = walkClip;
        sm.defaultState = idle;

        AnimatorStateTransition toWalk = idle.AddTransition(walk);
        toWalk.hasExitTime = false;
        toWalk.duration = 0f;
        toWalk.AddCondition(AnimatorConditionMode.Greater, 0.01f, "Speed");

        AnimatorStateTransition toIdle = walk.AddTransition(idle);
        toIdle.hasExitTime = false;
        toIdle.duration = 0f;
        toIdle.AddCondition(AnimatorConditionMode.Less, 0.01f, "Speed");

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static Tile[] BuildTiles()
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(TilesSheet).OfType<Sprite>().OrderBy(s => s.name).ToArray();
        Tile[] tiles = new Tile[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            string path = "Assets/Tilemaps/" + sprites[i].name + ".asset";
            AssetDatabase.DeleteAsset(path);
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprites[i];
            AssetDatabase.CreateAsset(tile, path);
            tiles[i] = tile;
        }
        return tiles;
    }

    private static void BuildGameScene(AnimatorController controller)
    {
        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject grid = new GameObject("Grid");
        grid.AddComponent<Grid>();

        GameObject tmGo = new GameObject("Tilemap");
        tmGo.transform.SetParent(grid.transform, false);
        Tilemap tilemap = tmGo.AddComponent<Tilemap>();
        tmGo.AddComponent<TilemapRenderer>();
        Rigidbody2D tmBody = tmGo.AddComponent<Rigidbody2D>();
        tmBody.bodyType = RigidbodyType2D.Static;
        TilemapCollider2D tmCollider = tmGo.AddComponent<TilemapCollider2D>();
        tmCollider.usedByComposite = true;
        tmGo.AddComponent<CompositeCollider2D>();

        Tile[] tiles = BuildTiles();
        int minX = -8;
        int maxX = 41;
        for (int x = minX; x <= maxX; x++)
        {
            int col = x == minX ? 0 : (x == maxX ? 2 : 1);
            tilemap.SetTile(new Vector3Int(x, -3, 0), tiles[col]);
            tilemap.SetTile(new Vector3Int(x, -4, 0), tiles[3 + col]);
            tilemap.SetTile(new Vector3Int(x, -5, 0), tiles[3 + col]);
            tilemap.SetTile(new Vector3Int(x, -6, 0), tiles[6 + col]);
        }

        Sprite idleSprite = AssetDatabase.LoadAllAssetsAtPath(IdleSheet).OfType<Sprite>().First(s => s.name == "Soldier_Idle_0");

        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0f, -1.5f, 0f);
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = idleSprite;
        sr.sortingLayerName = "Characters";
        Animator animator = player.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        CapsuleCollider2D capsule = player.AddComponent<CapsuleCollider2D>();
        capsule.offset = new Vector2(0f, 0.45f);
        capsule.size = new Vector2(0.4f, 0.8f);
        capsule.direction = CapsuleDirection2D.Vertical;
        player.AddComponent<PlayerController>();

        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.55f, 0.75f, 0.95f, 1f);
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        camGo.AddComponent<AudioListener>();
        camGo.transform.position = new Vector3(0f, -0.5f, -10f);
        CameraFollow2D follow = camGo.AddComponent<CameraFollow2D>();
        SerializedObject so = new SerializedObject(follow);
        so.FindProperty("target").objectReferenceValue = player.transform;
        so.FindProperty("minX").floatValue = 1f;
        so.FindProperty("maxX").floatValue = 33f;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, ScenesPath + "GameScene.unity");
    }
}
