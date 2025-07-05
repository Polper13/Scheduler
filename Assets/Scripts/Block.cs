using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public abstract class Block : MonoBehaviour
{
    public abstract string type { get; }
    [SerializeField] protected Button closeButton;
    [SerializeField] protected Button upButton;
    [SerializeField] protected Button downButton;
    [SerializeField] protected TMP_Text startTimeText;
    [SerializeField] public TimeSpan startTime = new TimeSpan(0);
    [SerializeField] public TimeSpan duration = new TimeSpan(0);
    protected bool relativeTiming = false;
    protected List<Block> blockList;

    public virtual void printInfo() {}

    public abstract BlockData toBlockData();
    public abstract void destroy();

    public virtual void updateTiming()
    {
        int index = blockList.IndexOf(this);

        if (index == 0)
        {
            relativeTiming = true;
            startTime = new TimeSpan(0);
        }

        if (index != 0)
        {
            Block previous = blockList[index - 1];
            startTime = previous.startTime + previous.duration;
            relativeTiming = previous.relativeTiming;
        }
        startTimeText.text = (relativeTiming ? "+" : "") + startTime.ToString(@"hh\:mm\:ss") + " / "
                           + (relativeTiming ? "+" : "") + (startTime + duration).ToString(@"hh\:mm\:ss");
    }

    protected void updateWholePageTiming()
    {
        foreach (Block block in blockList)
        {
            block.updateTiming();
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
        Block blockAbove = blockList[index - 1];
        blockList[index] = blockAbove;
        blockList[index - 1] = this;
        

        // update sibling's index'es in blockContainer
        blockAbove.gameObject.transform.SetSiblingIndex(index);
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
