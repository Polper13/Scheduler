using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WaitUntilBlock : Block
{
    public override string type => "WaitUntilBlock";
    [SerializeField] TMP_InputField timeInputField;
    [SerializeField] TMP_Text titleText;

    public override void printInfo()
    {
        Debug.Log("WaitUntilBlock: " + startTime.ToString(@"hh\:mm\:ss"));
    }

    public override BlockData toBlockData()
    {
        return new WaitUntilBlockData
        {
            type = this.type,
            value = this.startTime.ToString(@"hh\:mm\:ss")
        };
    }

    public override void updateTiming()
    {
        startTimeText.text = "starting " + startTime.ToString(@"hh\:mm\:ss");

        int index = blockList.IndexOf(this);

        // if its the last element
        if (index == blockList.Count - 1) { return; }

        for (int i = index + 1; i < blockList.Count; i++)
        {
            Block block = blockList[i];
            if (block is WaitUntilBlock) { break; }

            block.updateTiming();
        }
    }

    public static WaitUntilBlock create(GameObject container, GameObject waitUntilBlockPrefab)
    {
        GameObject waitUntilBlockGameObject = Instantiate(waitUntilBlockPrefab, container.transform);

        GameObject page = container.transform.parent.parent.gameObject;
        WaitUntilBlock waitUntilBlock = waitUntilBlockGameObject.GetComponent<WaitUntilBlock>();
        waitUntilBlock.blockList = page.GetComponent<Page>().blockList;
        waitUntilBlock.blockList.Add(waitUntilBlock);

        // set sibling index (order of displayingl in block container) according to index in the list
        waitUntilBlockGameObject.transform.SetSiblingIndex(waitUntilBlock.blockList.Count - 1);

        // setup
        waitUntilBlock.initialize();

        return waitUntilBlock;
    }

    public static WaitUntilBlock create(GameObject container, GameObject waitUntilBlockPrefab, string value)
    {
        WaitUntilBlock waitUntilBlock = WaitUntilBlock.create(container, waitUntilBlockPrefab);

        if (TimeSpan.TryParse(value, out waitUntilBlock.startTime) == false)
        {
            Debug.LogWarning($"Couldnt load WaitUntilBlock's startTime: {value}");
        }

        return waitUntilBlock;
    }


    private void initialize()
    {
        upButton.onClick.AddListener(moveUp);
        downButton.onClick.AddListener(moveDown);
        closeButton.onClick.AddListener(destroy);
        timeInputField.onEndEdit.AddListener(checkInput);
    }

    private void checkInput(string input)
    {
        EventSystem.current.SetSelectedGameObject(null); // unselect the input field
        titleText.color = new Color(255f / 255f, 69f / 255f, 69f / 255f);
        if (string.IsNullOrEmpty(input)) { return; }

        // change separators for one consistant ":"
        string separators = @"[.,;-]";
        input = Regex.Replace(input, separators, ":");

        bool success = false;
        switch (input.Length)
        {
        case 8:
            success = TimeSpan.TryParseExact(input, @"hh\:mm\:ss", null, out startTime);
            break;
        case 7:
            success = TimeSpan.TryParseExact(input, @"h\:mm\:ss", null, out startTime);
            if (success) { timeInputField.text = "0" + timeInputField.text; }
            break;
        case 5:
            success = TimeSpan.TryParseExact(input, @"hh\:mm", null, out startTime);
            if (success) { timeInputField.text = timeInputField.text + ":00"; }
            break;
        case 4:
            success = TimeSpan.TryParseExact(input, @"h\:mm", null, out startTime);
            if (success) { timeInputField.text = "0" + timeInputField.text + ":00"; }
            break;
        case 2:
            success = TimeSpan.TryParseExact(input, @"hh", null, out startTime);
            if (success) { timeInputField.text = timeInputField.text + ":00:00"; }
            break;
        case 1:
            success = TimeSpan.TryParseExact("0" + input, @"hh", null, out startTime);
            if (success) { timeInputField.text = "0" + timeInputField.text + ":00:00"; }
            break;
        }

        if (success)
        {
            timeInputField.text = Regex.Replace(timeInputField.text, separators, ":");
            titleText.color = new Color(221f / 255f, 221f / 255f, 221f / 255f);
            updateTiming();
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
