using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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

        if (Input.GetKeyDown(KeyCode.Space)) { save(); }
        if (Input.GetKeyDown(KeyCode.L)) { load(); }
    }

    public void save()
    {
        Page activePage = Page.getActivePage();
        if (activePage == null)
        {
            Debug.Log("no page");
            return;
        }

        // string path = Application.persistentDataPath + "/save.zip";
        string path = "C:/Users/48602/OneDrive/Pulpit/save.zip";
        SaveData.save(activePage, path);
    }

    public void load()
    {
        string extractPath;
        BlockDataListWrapper list = SaveData.load("C:/Users/48602/OneDrive/Pulpit/save.zip", out extractPath);
        if (list == null) { return; }

        // Debug.Log("loaded");

        // create a new Page
        Page page = Page.create(pagePrefab, pageButtonPrefab);
        GameObject container = Page.getActiveBlockContainer();
        if (container == null) {Debug.LogWarning("Couldnt find active block container"); return; }

        // load blocks to the page
        foreach (BlockData block in list.blocks)
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
