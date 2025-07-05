using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using SFB;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.IO;
using System.IO.Compression;

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

    void OnApplicationQuit()
    {
        SaveData.deleteCache();
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

        // check if contains config.json (means that its a scheduler file)
        if (Utils.ZipContainsFile(filePath[0], "config.json") == false)
        {
            Debug.LogWarning($"Not a Scheduler project file: {filePath[0]}");
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
            switch (block)
            {
                case WaitBlockData waitBlock:
                    WaitBlock.create(container, waitBlockPrefab, waitBlock.value);
                    break;
                case WaitUntilBlockData waitUntilBlock:
                    WaitUntilBlock.create(container, waitUntilBlockPrefab, waitUntilBlock.value);
                    break;
                case SongBlockData songBlock:
                    SongBlock.create(container, songBlockPrefab, $"{extractPath}/{songBlock.fileName}", songBlock.settings);
                    break;
                default:
                    Debug.LogWarning("Couldnt mach a type when creating blocks from json");
                    break;
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
}
