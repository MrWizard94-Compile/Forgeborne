#nullable enable
using System;
using System.IO;
using Forgeborne.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Player-facing entry, pause, terminal recovery and local save/load; game rules remain in Core.</summary>
[DefaultExecutionOrder(-100)]
public sealed class PlayerFlow : MonoBehaviour
{
    public GameManager? Manager;
    public FpsInputAdapter? InputAdapter;
    private GameObject? _menu;
    private Text? _heading;
    private Text? _status;
    private Text? _resources;
    private Button? _resume;
    private Button? _save;
    private Button? _restart;
    private bool _begun;
    private bool _quitWithoutSaveArmed;
    private bool _restartArmed;
    public bool IsMenuOpen => _menu != null && _menu.activeSelf;
    public string SavePath => Path.Combine(Application.persistentDataPath, "saves", "slot-1.json");
    public string Status => _status != null ? _status.text : "";

    void Start()
    {
        BuildUi();
        OpenMenu();
    }

    void Update()
    {
        var simulation = Manager != null ? Manager.Simulation : null;
        if (simulation == null) return;
        bool terminal = simulation.IsPlayerDead || simulation.IsObjectiveComplete;
        if (terminal && !IsMenuOpen) OpenMenu();
        if (_heading != null) _heading.text = simulation.IsPlayerDead ? "SUIT BREACH" :
            simulation.IsObjectiveComplete ? "RELAY DISABLED" : _begun ? "PAUSED" : "FORGEBORNE";
        if (_resume != null)
        {
            _resume.gameObject.SetActive(!terminal);
            _resume.GetComponentInChildren<Text>().text = _begun ? "RESUME" : "ENTER THE COURTYARD";
        }
        if (_save != null)
        {
            _save.interactable = !terminal && _begun;
            _save.GetComponentInChildren<Text>().color = _save.interactable ? Color.white : new Color(0.4f,0.48f,0.52f);
        }
        if (_resources != null)
        {
            var inventory = simulation.World.Player.Inventory;
            _resources.text = $"STONE {inventory.Get(BlockType.Stone)}   EMBER {inventory.Get(BlockType.EmberOre)}   WOOD {inventory.Get(BlockType.Wood)}\nQ  WALL · 2 STONE     B  WORKSTATION · 10 STONE + 5 EMBER";
        }
        if (!terminal && Input.GetKeyDown(KeyCode.Escape) && (InputAdapter == null || !InputAdapter.HasPendingCommand))
        {
            if (IsMenuOpen) Resume();
            else OpenMenu();
        }
    }

    public void OpenMenu()
    {
        if (_menu == null) return;
        _menu.SetActive(true);
        Time.timeScale = 0;
        if (InputAdapter != null)
        {
            InputAdapter.InputSuppressed = true;
            InputAdapter.ReleasePointer();
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        var simulation = Manager != null ? Manager.Simulation : null;
        var selected = simulation != null && (simulation.IsPlayerDead || simulation.IsObjectiveComplete) ? _restart : _resume;
        if (selected != null && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(selected.gameObject);
    }

    public void Resume()
    {
        var simulation = Manager != null ? Manager.Simulation : null;
        if (simulation == null || simulation.IsPlayerDead || simulation.IsObjectiveComplete) return;
        _begun = true;
        _quitWithoutSaveArmed = false;
        _restartArmed = false;
        if (_menu != null) _menu.SetActive(false);
        Time.timeScale = 1;
        if (InputAdapter != null)
        {
            InputAdapter.InputSuppressed = false;
            InputAdapter.CapturePointer();
        }
    }

    public void Restart()
    {
        var simulation = Manager != null ? Manager.Simulation : null;
        if (_begun && simulation != null && !simulation.IsPlayerDead && !simulation.IsObjectiveComplete && !_restartArmed)
        {
            _restartArmed = true;
            SetStatus("Restart discards unsaved changes. Press RESTART ENCOUNTER again to confirm.");
            return;
        }
        if (Manager == null || !Manager.RestartEncounter()) { SetStatus("The encounter could not restart; current state was kept."); return; }
        SetStatus("");
        Resume();
    }

    public void SaveSlot()
    {
        TrySaveSlot();
    }

    bool TrySaveSlot()
    {
        var simulation = Manager != null ? Manager.Simulation : null;
        if (simulation == null || simulation.IsPlayerDead || simulation.IsObjectiveComplete) return false;
        try
        {
            bool replacing = File.Exists(SavePath);
            LocalSaveStore.Write(SavePath, Manager!.SaveToJson());
            SetStatus(replacing ? "Saved. The previous slot is retained as a backup." : "Saved to the local slot.");
            _quitWithoutSaveArmed = false;
            _restartArmed = false;
            return true;
        }
        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
        { SetStatus("Save failed. Your previous slot was kept. " + FileError(error)); return false; }
    }

    public void LoadSlot()
    {
        if (Manager == null) return;
        try
        {
            string json = LocalSaveStore.Read(SavePath);
            if (!Manager.TryLoadFromJson(json)) { SetStatus("This save is invalid. The current encounter was kept."); return; }
            _begun = true;
            _quitWithoutSaveArmed = false;
            _restartArmed = false;
            SetStatus("Save loaded. Resume when ready.");
            OpenMenu();
        }
        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
        { SetStatus("Load failed. The current encounter was kept. " + FileError(error)); }
    }

    public void Quit()
    {
        var simulation = Manager != null ? Manager.Simulation : null;
        if (_begun && simulation != null && !simulation.IsPlayerDead && !simulation.IsObjectiveComplete
            && !_quitWithoutSaveArmed && !TrySaveSlot())
        {
            _quitWithoutSaveArmed = true;
            SetStatus("Saving failed. Press QUIT again to leave without saving this encounter.");
            return;
        }
        Time.timeScale = 1;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void SetStatus(string message) { if (_status != null) _status.text = message; }

    static string FileError(Exception error)
    {
        if (error is FileNotFoundException || error is DirectoryNotFoundException) return "No local slot has been saved yet.";
        if (error is UnauthorizedAccessException) return "The save folder cannot be accessed.";
        if (error is ArgumentException) return "The file is not supported UTF-8 JSON or exceeds the save limit.";
        return "Check available disk space and access to the save folder.";
    }

    void OnDestroy()
    {
        Time.timeScale = 1;
        if (InputAdapter != null) InputAdapter.InputSuppressed = false;
    }

    static RectTransform Box(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    static Text Label(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, int fontSize, Color colour, TextAnchor alignment)
    {
        var text = Box(parent, name, anchor, position, size).gameObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = colour;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    static Button ActionButton(Transform parent, string label, float top, UnityEngine.Events.UnityAction action)
    {
        var rect = Box(parent, label, new Vector2(0.5f, 1), new Vector2(0, -top), new Vector2(650, 52));
        var image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.08f, 0.18f, 0.23f, 1);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colours = button.colors;
        colours.highlightedColor = new Color(0.55f, 0.95f, 1f);
        colours.pressedColor = new Color(0.3f, 0.7f, 0.85f);
        colours.selectedColor = new Color(0.55f, 0.95f, 1f);
        colours.disabledColor = new Color(0.35f, 0.4f, 0.45f);
        button.colors = colours;
        button.onClick.AddListener(action);
        var text = Label(rect, "Label", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(620, 44), 22, Color.white, TextAnchor.MiddleCenter);
        text.text = label;
        return button;
    }

    void BuildUi()
    {
        var canvasObject = new GameObject("Player interface", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).transform.SetParent(transform, false);

        _resources = Label(canvasObject.transform, "Resources", Vector2.zero, new Vector2(38, 130), new Vector2(1000, 70), 21,
            new Color(0.78f,0.87f,0.91f), TextAnchor.LowerLeft);
        var menu = Box(canvasObject.transform, "Encounter menu", new Vector2(0.5f,0.5f), Vector2.zero, Vector2.zero);
        menu.anchorMin = Vector2.zero;
        menu.anchorMax = Vector2.one;
        menu.offsetMin = menu.offsetMax = Vector2.zero;
        menu.gameObject.AddComponent<Image>().color = new Color(0.015f,0.028f,0.045f,0.94f);
        _menu = menu.gameObject;
        var card = Box(menu, "Briefing", new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(840, 820));
        card.gameObject.AddComponent<Image>().color = new Color(0.035f,0.065f,0.09f,1);
        var stripe = Box(card, "Energy rule", new Vector2(0.5f,1), Vector2.zero, new Vector2(840, 4));
        stripe.gameObject.AddComponent<Image>().color = new Color(0.15f,0.85f,1f);
        _heading = Label(card, "Heading", new Vector2(0.5f,1), new Vector2(0,-45), new Vector2(730,70), 44, Color.white, TextAnchor.UpperCenter);
        Label(card, "Location", new Vector2(0.5f,1), new Vector2(0,-115), new Vector2(730,40), 20,
            new Color(0.25f,0.85f,1), TextAnchor.UpperCenter).text = "RELAY YARD  /  GATHER · BUILD · SURVIVE";
        Label(card, "Brief", new Vector2(0.5f,1), new Vector2(0,-170), new Vector2(700,110), 22,
            new Color(0.8f,0.85f,0.9f), TextAnchor.UpperCenter).text = "Salvage stone and ember ore. Build useful cover and a workstation.\nOutmaneuver the wardens, then destroy the relay.\nWASD · Move   Mouse · Look   E · Gather   Q · Wall   B · Workstation";
        _resume = ActionButton(card, "ENTER THE COURTYARD", 310, Resume);
        _restart = ActionButton(card, "RESTART ENCOUNTER", 374, Restart);
        _save = ActionButton(card, "SAVE SLOT", 438, SaveSlot);
        ActionButton(card, "LOAD SLOT", 502, LoadSlot);
        ActionButton(card, "QUIT", 566, Quit);
        _status = Label(card, "Save status", new Vector2(0.5f,1), new Vector2(0,-650), new Vector2(700,100), 20,
            new Color(1f,0.7f,0.35f), TextAnchor.UpperCenter);
        Label(card, "Footer", new Vector2(0.5f,0), new Vector2(0,30), new Vector2(700,35), 18,
            new Color(0.6f,0.7f,0.76f), TextAnchor.LowerCenter).text = "WIZARD PRODUCTIONS AI STUDIO  ·  SINGLE PLAYER";
    }
}
