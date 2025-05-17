using UnityEngine;
using UnityEngine.UI;

public class TaskBar : MonoBehaviour
{
    [SerializeField] Button pageButton;
    [SerializeField] Button songBlockButton;
    [SerializeField] Button waitUntilBlockButton;
    [SerializeField] Button waitBlockButton;

    void Start()
    {
        AppManager appManager = FindAnyObjectByType<AppManager>();

        pageButton.onClick.AddListener(appManager.addPage);
        songBlockButton.onClick.AddListener(appManager.addSongBlock);
        waitUntilBlockButton.onClick.AddListener(appManager.addWaitUntilBlock);
        waitBlockButton.onClick.AddListener(appManager.addWaitBlock);
    }


}


