using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    [SerializeField] TMP_Text pageNameText;
    [SerializeField] Button selectButton;
    [SerializeField] Button closeButton;
    public SongBlock playingBlock = null;
    bool selected = false;
    public bool playingTurnedOn = false;
    public TimeSpan playingStartTime;
    public string pageName { get; private set; }

    public static GameObject getActiveBlockContainer()
    {
        Page page = getActivePage();
        if (page == null) { return null; }

        GameObject scrollArea = Utils.getChildWithComponent<ScrollRect>(page.gameObject);
        GameObject blockContainer = scrollArea.transform.GetChild(0).gameObject;

        return blockContainer;
    }

    public static Page getActivePage()
    {
        if (pageList.Count <= 0) { return null; }
         
        foreach (Page page in pageList)
        {
            if (page.selected) { return page; }
        }

        return null;
    }

    public static Page findByName(string name)
    {
        foreach (Page page in pageList)
        {
            if (page.name == name) { return page; }
        }

        return null;
    }


    public void setName(string name)
    {
        pageName = name;

        GameObject textureGameObject = pageButtonGameObject.transform.GetChild(0).gameObject;
        GameObject buttonLabel = Utils.getChildWithComponent<TMP_Text>(textureGameObject);
        TMP_Text buttonLabelText = buttonLabel.GetComponent<TMP_Text>();

        if (buttonLabelText == null) { return; }
        buttonLabelText.text = name;
    }

    public static Page create(GameObject pagePrefab, GameObject pageButtonPrefab)
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

        // create references
        page.pageGameObject = pageGameObject;
        page.pageButtonGameObject = Instantiate(pageButtonPrefab, pageButtonContainer.transform);

        // generate name
        page.generateName();

        // initialize connections
        page.initialize();

        return page;
    }

    public static Page create(GameObject pagePrefab, GameObject pageButtonPrefab, string name)
    {
        Page page = Page.create(pagePrefab, pageButtonPrefab);
        page.setName(name);

        return page;
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
        closeButton = Utils.getChildWithComponent<Button>(temp).GetComponent<Button>();
        pageNameText = Utils.getChildWithComponent<TMP_Text>(temp).GetComponent<TMP_Text>();

        selectButton.onClick.AddListener(select);
        closeButton.onClick.AddListener(destroy);
        playToggle.onValueChanged.AddListener(play);
    }

    void generateName()
    {
        var takenIndexes = new List<int>();
        foreach (Page pageObj in pageList)
        {
            if (pageObj.pageName == null) { continue; }
            
            if (pageObj.pageName.StartsWith("page"))
            {
                string numberPart = pageObj.pageName.Substring("page".Length);
                if (int.TryParse(numberPart, out int number) && number >= 0)
                {
                    takenIndexes.Add(number);
                }
                else if (numberPart == string.Empty) { takenIndexes.Add(0); }
            }
        }

        takenIndexes.Sort();
        if (takenIndexes.Count == 0) { this.setName("page"); }
        else
        {
            int newIndex = takenIndexes[takenIndexes.Count - 1] + 1;
            this.setName($"page{newIndex}");
        }
    }

    void FixedUpdate()
    {
        if (playingTurnedOn == false) { return; }

        if (playingBlock != null)
        {
            if (playingBlock.shouldPlayNow() == false)
            {
                playingBlock.stop();
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
            if (block.shouldPlayNow() && block.isPlaying == false && playingBlock == null)
            {
                block.play(block.settings.normalize);
                block.updateAudioSourceSettings(audioSource);
            }
        }
    }

    void play(bool value)
    {
        if (value == false && playingBlock != null)
        {
            playingBlock.stop();
        }
        if (value == true)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            playingStartTime = now;
        }

        // update icon on play button
        playIconOn.SetActive(!value);
        playIconOff.SetActive(value);

        // update page name color
        Color playColor = new Color(105 / 255.0f, 176 / 255.0f, 87 / 255.0f, 1.0f);
        Color defaultColor = new Color(221 / 255.0f, 221 / 255.0f, 221 / 255.0f, 1.0f);
        pageNameText.color = value ? playColor : defaultColor;

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
            if (page != this) { page.deactivate(); }
        }
        this.activate();
    }

    public void destroy()
    {
        if (pageList != null && pageList.Contains(this))
        {
            pageList.Remove(this);
        }

        // remove all blocks first
        if (blockList != null && blockList.Count > 0)
        {
            foreach (Block block in new List<Block>(blockList)) { block.destroy(); }
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
        if (selected == false)
        {
            // change pageButton look -> connect to page to show that it is selected
            VerticalLayoutGroup layout = pageButtonGameObject.GetComponent<VerticalLayoutGroup>();
            layout.padding.bottom = 20;

            // update layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
        }

        selected = true;
        if (playingTurnedOn == false) { this.gameObject.SetActive(true); } 
        this.gameObject.transform.SetAsLastSibling(); // move to the Top
    }
    void deactivate()
    {
        if (selected)
        {
            // change pageButton look -> disconnect from page to show that it is not selected
            VerticalLayoutGroup layout = pageButtonGameObject.GetComponent<VerticalLayoutGroup>();
            layout.padding.bottom = 4;

            // update layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
        }

        selected = false;
        if (playingTurnedOn == false) { this.gameObject.SetActive(false); }

        
    }
}
