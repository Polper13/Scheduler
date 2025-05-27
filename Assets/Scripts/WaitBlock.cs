using UnityEngine;
using TMPro;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WaitBlock : Block
{
    [SerializeField] TMP_InputField timeInputField;
    [SerializeField] TMP_Text titleText;
    [SerializeField] Slider progressBar;
    Page page;

    public override void printInfo()
    {
        Debug.Log("WaitBlock: " + startTime.ToString(@"hh\:mm\:ss"));
    }

    public static void create(GameObject container, GameObject waitBlockPrefab)
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
        if (string.IsNullOrEmpty(input)) { return; }

        if (input[input.Length - 1] == 's')
        {
            input = input.Substring(0, input.Length - 1);
        }

        int seconds;
        bool success = int.TryParse(input, out seconds);

        if (success)
        {
            duration = TimeSpan.FromSeconds(seconds);
            timeInputField.text = seconds.ToString() + "s";
            titleText.color = new Color(221f / 255f, 221f / 255f, 221f / 255f);
            updateWholePageTiming();
        }
    }

    private void destroy()
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
