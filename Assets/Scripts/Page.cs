using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Page : MonoBehaviour
{
    public static List<Page> pageList = new List<Page>();
    public List<Block> blockList = new List<Block>();

    [SerializeField] public AudioSource audioSource;
    [SerializeField] Toggle playToggle;
    [SerializeField] GameObject playIconOn;
    [SerializeField] GameObject playIconOff;
    [SerializeField] GameObject pageGameObject;
    [SerializeField] GameObject pageButtonGameObject;
    [SerializeField] Button selectButton;
    [SerializeField] Button closeButton;
    public SongBlock playingBlock = null;
    bool selected = false;
    bool playingTurnedOn = false;

    public static GameObject getActiveBlockContainer()
    {
        if (pageList.Count <= 0) { return null; }
         
        foreach (Page page in pageList)
        {
            if (page.selected)
            {
                GameObject scrollArea = AppManager.getChildWithComponent<ScrollRect>(page.gameObject);
                GameObject blockContainer = scrollArea.transform.GetChild(0).gameObject;

                return blockContainer;
            }
        }

        return null;
    }

    public static void create(GameObject pagePrefab, GameObject pageButtonPrefab)
    {
        // get references to containers
        GameObject pageButtonContainer = GameObject.FindGameObjectWithTag("pageButtonContainer");
        if (pageButtonContainer == null) { Debug.LogError("Couldnt find \"pageButtonContainer\""); }

        GameObject pageContainer = GameObject.FindGameObjectWithTag("pageContainer");
        if (pageContainer == null) { Debug.LogError("Couldnt find \"pageContainer\""); }

        // create Page object
        GameObject pageGameObject = Instantiate(pagePrefab, pageContainer.transform);
        Page page = pageGameObject.GetComponent<Page>();
        pageList.Add(page);

        // initialize references
        page.pageGameObject = pageGameObject;
        page.pageButtonGameObject = Instantiate(pageButtonPrefab, pageButtonContainer.transform);

        // initialize connections
        page.initialize();
    }

    void initialize()
    {
        // set focus to this new page
        this.select();

        playIconOn.SetActive(true);
        playIconOff.SetActive(false);

        // get component references
        selectButton = pageButtonGameObject.GetComponent<Button>();

        GameObject temp = pageButtonGameObject.transform.GetChild(0).gameObject;
        closeButton = AppManager.getChildWithComponent<Button>(temp).GetComponent<Button>();

        selectButton.onClick.AddListener(select);
        closeButton.onClick.AddListener(destroy);
        playToggle.onValueChanged.AddListener(play);
    }

    void FixedUpdate()
    {
        if (playingTurnedOn == false) { return; }

        TimeSpan currentTime = DateTime.Now.TimeOfDay;
        if (playingBlock != null)
        {
            if (playingBlock.shouldPlayNow() == false)
            {
                playingBlock.stop(this);
            }
            else
            {
                playingBlock.updateAudioSourceSettings(audioSource);
            }
        }

        var SongBlocks = blockList.OfType<SongBlock>();
        foreach (SongBlock block in SongBlocks)
        {
            // if currentTime is in range of this blocks duration
            if (block.shouldPlayNow() && block.isPlaying == false)
            {
                if (playingBlock != null) { playingBlock.stop(this); }

                block.play(this);
                block.updateAudioSourceSettings(audioSource);
            }
        }
    }

    void play(bool value)
    {
        if (value == false && playingBlock != null)
        {
            playingBlock.stop(this);
        }

        playIconOn.SetActive(!value);
        playIconOff.SetActive(value);

        playingTurnedOn = value;
    }

    void select()
    {
        if (pageList == null)
        {
            Debug.LogError("pageList disappeared");
            return;
        }

        foreach (Page page in pageList)
        {
            if (page != this.pageGameObject) { page.deactivate(); }
        }
        this.activate();
    }

    void destroy()
    {
        if (pageList != null && pageList.Contains(this))
        {
            pageList.Remove(this);
        }

        // bring focus to first page if this one was in focus
        if (selected && pageList.Count > 0)
        {
            pageList[0].activate();
        }

        if (selectButton != null) { selectButton.onClick.RemoveAllListeners(); }
        if (closeButton != null) { closeButton.onClick.RemoveAllListeners(); }

        Destroy(pageButtonGameObject);
        Destroy(pageGameObject);
    }

    void activate()
    {
        // move to the Top
        this.gameObject.transform.SetAsLastSibling();
        selected = true;
    }
    void deactivate()
    {
        selected = false;
    }
}
