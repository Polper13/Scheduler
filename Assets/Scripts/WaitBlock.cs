using UnityEngine;
using TMPro;
using System;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class WaitBlock : Block
{
    public override string type => "WaitBlock";
    [SerializeField] TMP_InputField timeInputField;
    [SerializeField] TMP_Text titleText;
    [SerializeField] Slider progressBar;
    Page page;

    public override void printInfo()
    {
        Debug.Log("WaitBlock: " + startTime.ToString(@"hh\:mm\:ss"));
    }

    public override BlockData toBlockData()
    {
        return new WaitBlockData
        {
            type = this.type,
            value = (int)this.duration.TotalSeconds
        };
    }

    public static WaitBlock create(GameObject container, GameObject waitBlockPrefab)
    {
        GameObject waitBlockGameObject = Instantiate(waitBlockPrefab, container.transform);

        GameObject page = container.transform.parent.parent.gameObject;
        WaitBlock waitBlock = waitBlockGameObject.GetComponent<WaitBlock>();
        waitBlock.blockList = page.GetComponent<Page>().blockList;
        waitBlock.blockList.Add(waitBlock);
        waitBlock.page = page.GetComponent<Page>();

        // set sibling index (order of displayingl in block container) according to index in the list
        waitBlockGameObject.transform.SetSiblingIndex(waitBlock.blockList.Count - 1);

        // setup
        waitBlock.updateTiming();
        waitBlock.initialize();

        return waitBlock;
    }

    public static WaitBlock create(GameObject container, GameObject waitBlockPrefab, int value)
    {
        WaitBlock waitBlock = WaitBlock.create(container, waitBlockPrefab);
        waitBlock.checkInput(value.ToString());

        return waitBlock;
    }

    private void initialize()
    {
        upButton.onClick.AddListener(moveUp);
        downButton.onClick.AddListener(moveDown);
        closeButton.onClick.AddListener(destroy);
        timeInputField.onEndEdit.AddListener(checkInput);
    }

    void Update()
    {
        TimeSpan now = DateTime.Now.TimeOfDay;
        TimeSpan calculatedStartTime = relativeTiming ? page.playingStartTime + startTime : startTime;
        bool isPlaying = now > calculatedStartTime && now < calculatedStartTime + duration;
        progressBar.gameObject.SetActive(isPlaying && page.playingTurnedOn);

        if (isPlaying && page.playingTurnedOn)
        {
            double offset;
            if (relativeTiming == false) { offset = (now - startTime).TotalSeconds; }
            else { offset = (now - page.playingStartTime - startTime).TotalSeconds; }

            progressBar.value = (float)(offset / duration.TotalSeconds);
        }
    }

    private void checkInput(string input)
    {
        EventSystem.current.SetSelectedGameObject(null); // unselect the input field
        titleText.color = new Color(255f / 255f, 69f / 255f, 69f / 255f);
        
        bool success = TimeSpanUtils.tryParseDuration(input, out duration);

        if (success)
        {
            // generate a string to display the duration value
            timeInputField.text = TimeSpanUtils.ToDetailedString(duration);
            titleText.color = new Color(221f / 255f, 221f / 255f, 221f / 255f);
            updateWholePageTiming();
        }
    }

    public override void destroy()
    {
        if (blockList != null && blockList.Contains(this))
        {
            blockList.Remove(this);
        }

        if (upButton != null) { upButton.onClick.RemoveAllListeners(); }
        if (downButton != null) { downButton.onClick.RemoveAllListeners(); }
        if (closeButton != null) { closeButton.onClick.RemoveAllListeners(); }

        updateWholePageTiming();
        Destroy(this.gameObject);
    }
}
