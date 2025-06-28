using UnityEngine;
using UnityEngine.UI;

public class TaskBar : MonoBehaviour
{
    [Header("File")]
    [SerializeField] Button pageButton;
    [SerializeField] Button exportButton;
    [SerializeField] Button importButton;

    [Header("Block")]
    [SerializeField] Button songBlockButton;
    [SerializeField] Button waitUntilBlockButton;
    [SerializeField] Button waitBlockButton;

    void Start()
    {
        AppManager appManager = FindAnyObjectByType<AppManager>();

        pageButton.onClick.AddListener(appManager.addPage);
        exportButton.onClick.AddListener(appManager.export);
        importButton.onClick.AddListener(appManager.import);

        songBlockButton.onClick.AddListener(appManager.addSongBlock);
        waitUntilBlockButton.onClick.AddListener(appManager.addWaitUntilBlock);
        waitBlockButton.onClick.AddListener(appManager.addWaitBlock);
    }


}


