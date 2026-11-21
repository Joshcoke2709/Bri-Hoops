using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class BriHoopsPauseUiBuilder
{
    private const string AutoCreateSessionKey = "BriHoopsPauseUiBuilder_AutoCreateTried";

    static BriHoopsPauseUiBuilder()
    {
        EditorApplication.delayCall += AutoCreateIfNeeded;
    }

    [MenuItem("Tools/Bri Hoops/Create Pause UI")]
    public static void CreatePauseUiFromMenu()
    {
        CreatePauseUi(true);
    }

    private static void AutoCreateIfNeeded()
    {
        if (SessionState.GetBool(AutoCreateSessionKey, false))
        {
            return;
        }

        SessionState.SetBool(AutoCreateSessionKey, true);

        GamePauseManager pauseManager = FindInOpenScene<GamePauseManager>();
        if (pauseManager == null)
        {
            return;
        }

        bool needsUi = pauseManager.pauseMenu == null
            || pauseManager.pauseMenu == pauseManager.gameObject
            || pauseManager.pauseButton == null
            || pauseManager.resumeButton == null
            || pauseManager.restartButton == null
            || pauseManager.difficultyDropdown == null;

        if (needsUi)
        {
            CreatePauseUi(false);
        }
    }

    private static void CreatePauseUi(bool showDialog)
    {
        Canvas canvas = FindOrCreateCanvas();
        GamePauseManager pauseManager = FindOrCreatePauseManager();
        ScoreTimerLevelManager scoreTimer = FindInOpenScene<ScoreTimerLevelManager>();
        BasketballShooter shooter = FindInOpenScene<BasketballShooter>();
        SimpleMovingBackboard movingHoop = FindInOpenScene<SimpleMovingBackboard>();

        EnsureCanvasCanReceiveClicks(canvas);
        EnsureEventSystemExists();

        Button pauseButton = FindOrCreateButton(canvas.transform, "PauseButton", "PAUSE");
        ConfigureRect(pauseButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-68f, -42f), new Vector2(120f, 48f));

        GameObject pauseMenu = FindDirectChild(canvas.transform, "PauseMenu");
        if (pauseMenu == null)
        {
            pauseMenu = ObjectFactory.CreateGameObject("PauseMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Undo.RegisterCreatedObjectUndo(pauseMenu, "Create Pause Menu");
            pauseMenu.transform.SetParent(canvas.transform, false);
        }

        pauseMenu.layer = canvas.gameObject.layer;
        Image panelImage = pauseMenu.GetComponent<Image>();
        if (panelImage == null)
        {
            panelImage = pauseMenu.AddComponent<Image>();
        }

        panelImage.color = new Color(0f, 0f, 0f, 0.78f);
        ConfigureRect(pauseMenu.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        TextMeshProUGUI titleText = FindOrCreateText(pauseMenu.transform, "PauseTitleText", "PAUSED");
        titleText.fontSize = 48f;
        titleText.alignment = TextAlignmentOptions.Center;
        ConfigureRect(titleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 140f), new Vector2(420f, 80f));

        Button resumeButton = FindOrCreateButton(pauseMenu.transform, "ResumeButton", "RESUME");
        ConfigureRect(resumeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 55f), new Vector2(260f, 56f));

        Button restartButton = FindOrCreateButton(pauseMenu.transform, "RestartButton", "RESTART");
        ConfigureRect(restartButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -15f), new Vector2(260f, 56f));

        TMP_Dropdown difficultyDropdown = FindOrCreateDropdown(pauseMenu.transform);
        ConfigureRect(difficultyDropdown.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -92f), new Vector2(260f, 46f));

        pauseManager.pauseMenu = pauseMenu;
        pauseManager.pauseButton = pauseButton;
        pauseManager.resumeButton = resumeButton;
        pauseManager.restartButton = restartButton;
        pauseManager.difficultyDropdown = difficultyDropdown;

        if (shooter != null)
        {
            pauseManager.shooterScript = shooter;
        }

        if (movingHoop != null)
        {
            pauseManager.movingHoop = movingHoop;
        }

        if (scoreTimer != null)
        {
            pauseManager.scoreTimer = scoreTimer;
        }

        CreateOrUpdateMobileInput(shooter, scoreTimer, pauseManager);

        ApplyArcadeFontIfAssigned(pauseManager, pauseMenu);
        pauseMenu.SetActive(false);

        EditorUtility.SetDirty(pauseManager);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        if (showDialog)
        {
            EditorUtility.DisplayDialog("Bri Hoops", "Pause UI created and assigned.", "OK");
        }
    }

    private static Button FindOrCreateButton(Transform parent, string name, string label)
    {
        GameObject existing = FindDirectChild(parent, name);
        GameObject buttonObject = existing != null ? existing : TMP_DefaultControls.CreateButton(GetStandardResources());

        if (existing == null)
        {
            Undo.RegisterCreatedObjectUndo(buttonObject, "Create " + name);
            buttonObject.name = name;
            buttonObject.transform.SetParent(parent, false);
        }

        Button button = buttonObject.GetComponent<Button>();
        TMP_Text text = buttonObject.GetComponentInChildren<TMP_Text>(true);
        Image image = buttonObject.GetComponent<Image>();

        if (image != null)
        {
            image.color = new Color(1f, 0.08f, 0.08f, 0.92f);
        }

        if (text != null)
        {
            text.text = label;
            text.fontSize = 24f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
        }

        return button;
    }

    private static TMP_Dropdown FindOrCreateDropdown(Transform parent)
    {
        GameObject existing = FindDirectChild(parent, "DifficultyDropdown");
        GameObject dropdownObject = existing != null ? existing : TMP_DefaultControls.CreateDropdown(GetStandardResources());

        if (existing == null)
        {
            Undo.RegisterCreatedObjectUndo(dropdownObject, "Create Difficulty Dropdown");
            dropdownObject.name = "DifficultyDropdown";
            dropdownObject.transform.SetParent(parent, false);
        }

        TMP_Dropdown dropdown = dropdownObject.GetComponent<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string> { "Easy", "Normal", "Hard" });
        dropdown.value = 1;
        dropdown.RefreshShownValue();

        Image image = dropdownObject.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.02f, 0.02f, 0.02f, 0.94f);
        }

        TMP_Text[] texts = dropdownObject.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].fontSize = 20f;
            texts[i].color = Color.white;
        }

        return dropdown;
    }

    private static TextMeshProUGUI FindOrCreateText(Transform parent, string name, string text)
    {
        GameObject existing = FindDirectChild(parent, name);
        TextMeshProUGUI textComponent;

        if (existing == null)
        {
            GameObject textObject = ObjectFactory.CreateGameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            Undo.RegisterCreatedObjectUndo(textObject, "Create " + name);
            textObject.transform.SetParent(parent, false);
            textComponent = textObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            textComponent = existing.GetComponent<TextMeshProUGUI>();
            if (textComponent == null)
            {
                textComponent = existing.AddComponent<TextMeshProUGUI>();
            }
        }

        textComponent.text = text;
        textComponent.color = Color.white;
        return textComponent;
    }

    private static void CreateOrUpdateMobileInput(BasketballShooter shooter, ScoreTimerLevelManager scoreTimer, GamePauseManager pauseManager)
    {
        MobileInputController mobileInput = FindInOpenScene<MobileInputController>();
        if (mobileInput == null)
        {
            GameObject mobileInputObject = ObjectFactory.CreateGameObject("MobileInputController", typeof(MobileInputController));
            Undo.RegisterCreatedObjectUndo(mobileInputObject, "Create Mobile Input Controller");
            mobileInput = mobileInputObject.GetComponent<MobileInputController>();
        }

        mobileInput.shooter = shooter;
        mobileInput.gameManager = scoreTimer;
        mobileInput.pauseManager = pauseManager;
        EditorUtility.SetDirty(mobileInput);
    }

    private static Canvas FindOrCreateCanvas()
    {
        Canvas canvas = FindInOpenScene<Canvas>();
        if (canvas != null)
        {
            return canvas;
        }

        GameObject canvasObject = ObjectFactory.CreateGameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas");
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        return canvas;
    }

    private static GamePauseManager FindOrCreatePauseManager()
    {
        GamePauseManager pauseManager = FindInOpenScene<GamePauseManager>();
        if (pauseManager != null)
        {
            return pauseManager;
        }

        GameObject pauseManagerObject = ObjectFactory.CreateGameObject("PauseManager", typeof(GamePauseManager));
        Undo.RegisterCreatedObjectUndo(pauseManagerObject, "Create Pause Manager");
        return pauseManagerObject.GetComponent<GamePauseManager>();
    }

    private static void EnsureCanvasCanReceiveClicks(Canvas canvas)
    {
        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            Undo.AddComponent<GraphicRaycaster>(canvas.gameObject);
        }

        if (canvas.GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = Undo.AddComponent<CanvasScaler>(canvas.gameObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private static void EnsureEventSystemExists()
    {
        if (FindInOpenScene<EventSystem>() != null)
        {
            return;
        }

        Type inputSystemModule = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        GameObject eventSystemObject;

        if (inputSystemModule != null)
        {
            eventSystemObject = ObjectFactory.CreateGameObject("EventSystem", typeof(EventSystem), inputSystemModule);
        }
        else
        {
            eventSystemObject = ObjectFactory.CreateGameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
    }

    private static void ConfigureRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static GameObject FindDirectChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        return child != null ? child.gameObject : null;
    }

    private static T FindInOpenScene<T>() where T : UnityEngine.Object
    {
        T[] allObjects = Resources.FindObjectsOfTypeAll<T>();
        Scene activeScene = SceneManager.GetActiveScene();

        for (int i = 0; i < allObjects.Length; i++)
        {
            UnityEngine.Object current = allObjects[i];
            GameObject gameObject = GetGameObject(current);

            if (gameObject != null
                && gameObject.scene == activeScene
                && !EditorUtility.IsPersistent(current))
            {
                return allObjects[i];
            }
        }

        return null;
    }

    private static GameObject GetGameObject(UnityEngine.Object current)
    {
        if (current is GameObject gameObject)
        {
            return gameObject;
        }

        if (current is Component component)
        {
            return component.gameObject;
        }

        return null;
    }

    private static TMP_DefaultControls.Resources GetStandardResources()
    {
        return new TMP_DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
            checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
            dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
            mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd")
        };
    }

    private static void ApplyArcadeFontIfAssigned(GamePauseManager pauseManager, GameObject pauseMenu)
    {
        if (pauseManager.arcadeFont == null)
        {
            return;
        }

        TMP_Text[] texts = pauseMenu.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].font = pauseManager.arcadeFont;
            EditorUtility.SetDirty(texts[i]);
        }
    }
}
