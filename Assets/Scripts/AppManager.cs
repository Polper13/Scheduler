using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class AppManager : MonoBehaviour
{
    [SerializeField] GameObject pagePrefab;
    [SerializeField] GameObject pageButtonPrefab;
    [SerializeField] GameObject songBlockPrefab;
    [SerializeField] GameObject waitUntilBlockPrefab;
    [SerializeField] GameObject waitBlockPrefab;

    void Start()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = 30;
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
