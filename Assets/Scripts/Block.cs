using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    [SerializeField] protected Button closeButton;
    [SerializeField] protected Button upButton;
    [SerializeField] protected Button downButton;
    [SerializeField] protected TMP_Text startTimeText;
    [SerializeField] public TimeSpan startTime = new TimeSpan(0);
    [SerializeField] public TimeSpan duration = new TimeSpan(0);
    protected List<Block> blockList;

    public virtual void printInfo() {}

    public virtual void updateTiming()
    {
        int index = blockList.IndexOf(this);

        // sanity check (shouldnt happen)
        if (index == 0) { return; }

        Block previous = blockList[index - 1];
        startTime = previous.startTime + previous.duration;
        startTimeText.text = "starting " + startTime.ToString(@"hh\:mm\:ss");
    }

    protected void updateWholePageTiming()
    {
        foreach (Block block in blockList)
        {
            if (block is WaitUntilBlock) { block.updateTiming(); }
        }
    }

    protected void moveUp()
    {
        int index = blockList.IndexOf(this);

        // if isnt already in the list
        if (index == -1) { Debug.LogError("Block object not in the blockList"); return; }

        // if is already the first
        if (index == 0) { return; }

        // move stuff (swap two blocks)
        Block blockAbowe = blockList[index - 1];
        blockList[index] = blockAbowe;
        blockList[index - 1] = this;
        

        // update sibling's index'es in blockContainer
        blockAbowe.gameObject.transform.SetSiblingIndex(index);
        this.gameObject.transform.SetSiblingIndex(index - 1);

        // update timing after block order shifted
        updateWholePageTiming();
    }

    protected void moveDown()
    {
        int index = blockList.IndexOf(this);

        // if isnt already in the list
        if (index == -1) { Debug.LogError("Block object not in the blockList"); return; }

        // if is already the last
        if (index == blockList.Count - 1) { return; }

        // move stuff
        Block blockBelow = blockList[index + 1];
        blockList[index] = blockBelow;
        blockList[index + 1] = this;
        

        // update sibling's index'es in blockContainer
        blockBelow.gameObject.transform.SetSiblingIndex(index);
        this.gameObject.transform.SetSiblingIndex(index + 1);

        // update timing after block order shifted
        updateWholePageTiming();
    }
}
