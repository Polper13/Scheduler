using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class OverwriteDialogMenu : MonoBehaviour
{
    [SerializeField] GameObject dimmBackground;
    [SerializeField] Button overwriteButton;
    [SerializeField] Button cancelButton;

    public Task<bool> askForOverwrite()
    {
        gameObject.SetActive(true);
        dimmBackground.SetActive(true);
        var tcs = new TaskCompletionSource<bool>();

        void overwriteClicked()
        {
            tcs.TrySetResult(true);
            cleanup();
        }

        void CancelClicked()
        {
            tcs.TrySetResult(false);
            cleanup();
        }

        void cleanup()
        {
            gameObject.SetActive(false);
            dimmBackground.SetActive(false);

            overwriteButton.onClick.RemoveListener(overwriteClicked);
            cancelButton.onClick.RemoveListener(CancelClicked);
        }

        overwriteButton.onClick.AddListener(overwriteClicked);
        cancelButton.onClick.AddListener(CancelClicked);

        return tcs.Task;
    }
}