using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using SFB;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.IO;

public static class GeneralSettings
    {
        public static float uiScaling  = 1f;

        static CanvasScaler canvasScaler;

        public static void loadSettings()
        {
            // TODO - load settings from json
        }
        public static void updateSettings()
        {
            if (canvasScaler == null)
            {
                canvasScaler = GameObject.FindAnyObjectByType<CanvasScaler>();
                if (canvasScaler == null) { Debug.LogError("Couldnt find canvas scaler"); return; }
            }
            
            canvasScaler.scaleFactor = uiScaling;
        }
    }

public class AppManager : MonoBehaviour
{
    [SerializeField] GameObject pagePrefab;
    [SerializeField] GameObject pageButtonPrefab;
    [SerializeField] GameObject songBlockPrefab;
    [SerializeField] GameObject waitUntilBlockPrefab;
    [SerializeField] GameObject waitBlockPrefab;

    void Awake()
    {
        GeneralSettings.loadSettings();
    }

    void Start()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = 60;
    }

    void FixedUpdate()
    {
        GeneralSettings.updateSettings();
    }

    void Update()
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null && selected.GetComponent<TMP_InputField>() != null)
        {
            return; // return and do not detect key presses because input field is seleted now
        }

        if (Input.GetKeyDown(KeyCode.P)) { addPage(); }
        if (Input.GetKeyDown(KeyCode.S)) { addSongBlock(); }
        if (Input.GetKeyDown(KeyCode.U)) { addWaitUntilBlock(); }
        if (Input.GetKeyDown(KeyCode.W)) { addWaitBlock(); }

        if (Input.GetKeyDown(KeyCode.Space)) { export(); }
        if (Input.GetKeyDown(KeyCode.L)) { import(); }
    }

    public async void import()
    {
        ButtonWithSubmenu.hideAllMenues();

        string[] filePath = StandaloneFileBrowser.OpenFilePanel("Import project", "", "zip", false);
        if (filePath.Length <= 0 || string.IsNullOrEmpty(filePath[0]))
        {
            Debug.LogWarning("No file selected to import");
            return;
        }

        LoadResult loadResult = await SaveData.load(filePath[0]);
        if (loadResult == null) { return; }

        // create a new Page
        Page page = Page.create(pagePrefab, pageButtonPrefab, Path.GetFileNameWithoutExtension(filePath[0]));
        GameObject container = Page.getActiveBlockContainer();
        if (container == null)
        {
            Debug.LogWarning("Couldnt find active block container");
            return;
        }

        // load blocks to the page
        string extractPath = loadResult.extractPath;
        foreach (BlockData block in loadResult.blockDataListWrapper.blocks)
        {
            if (block is WaitBlockData waitBlock)
            {
                WaitBlock.create(container, waitBlockPrefab, waitBlock.value);
            }
            else if (block is WaitUntilBlockData waitUntilBlock)
            {
                WaitUntilBlock.create(container, waitUntilBlockPrefab, waitUntilBlock.value);
            }
            else if (block is SongBlockData songBlock)
            {
                SongBlock.create(container, songBlockPrefab, $"{extractPath}/{songBlock.fileName}", songBlock.settings);
            }
            else
            {
                Debug.LogWarning("Couldnt mach a type when creating blocks from json");
            }
        }
    }

    public void export()
    {
        ButtonWithSubmenu.hideAllMenues();

        Page activePage = Page.getActivePage();
        if (activePage == null)
        {
            Debug.Log("Could not contnue with exporting: \"no page\"");
            return;
        }

        string path = StandaloneFileBrowser.SaveFilePanel("Export project", "", "project", "zip");
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning($"Null export path: {path}");
            return;
        }
        
        SaveData.save(activePage, path);
    }

    public void addPage()
    {
        Page.create(pagePrefab, pageButtonPrefab);
    }

    public void addSongBlock()
    {
        GameObject container = Page.getActiveBlockContainer();
        if (container == null) {Debug.LogWarning("Couldnt find active block container"); return; }

        SongBlock.create(container, songBlockPrefab);
    }

    public void addWaitUntilBlock()
    {
        GameObject container = Page.getActiveBlockContainer();
        if (container == null) {Debug.LogWarning("Couldnt find active block container"); return; }

        WaitUntilBlock.create(container, waitUntilBlockPrefab);
    }

    public void addWaitBlock()
    {
        GameObject container = Page.getActiveBlockContainer();
        if (container == null) {Debug.LogWarning("Couldnt find active block container"); return; }

        WaitBlock.create(container, waitBlockPrefab);
    }

    public static GameObject getChildWithComponent<T>(GameObject parent) where T : Component
    {
        foreach (Transform child in parent.transform)
        {
            T component = child.GetComponent<T>();
            if (component != null) { return child.gameObject; }
        }

        Debug.LogError("Couldnt find a child with component on parent: " + parent);
        return null;
    }
}
