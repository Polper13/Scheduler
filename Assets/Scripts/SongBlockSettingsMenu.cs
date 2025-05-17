using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SongBlockSettings
{
    public bool muted;
    public float volume;
    public float fadeIn;
    public float fadeOut;

    public SongBlockSettings(bool muted, float volume)
    {
        this.muted = muted;
        this.volume = volume;
        fadeIn = 0;
        fadeOut = 0;
    }
}

public class SongBlockSettingsMenu : MonoBehaviour
{
    [SerializeField] GameObject DimmBackground;
    [SerializeField] Button clickAwayButton;

    [Header("Volume")]
    [SerializeField] Toggle muteToggle;
    [SerializeField] Button volumeUpButton;
    [SerializeField] Button volumeDownButton;
    [SerializeField] TMP_Text volumeText;

    [Header("Fade")]
    [SerializeField] Button FadeInUpButton;
    [SerializeField] Button FadeInDownButton;
    [SerializeField] TMP_Text fadeInText;
    [SerializeField] Button FadeOutUpButton;
    [SerializeField] Button FadeOutDownButton;
    [SerializeField] TMP_Text fadeOutText;

    SongBlockSettings currentSettings;

    void Start()
    {
        clickAwayButton.onClick.AddListener(closeMenu);

        muteToggle.onValueChanged.AddListener(mute);
        volumeUpButton.onClick.AddListener(volumeUp);
        volumeDownButton.onClick.AddListener(volumeDown);

        FadeInUpButton.onClick.AddListener(fadeInUp);
        FadeInDownButton.onClick.AddListener(fadeInDown);
        FadeOutUpButton.onClick.AddListener(fadeOutUp);
        FadeOutDownButton.onClick.AddListener(fadeOutDown);
    }

    void closeMenu()
    {
        this.gameObject.SetActive(false);
        DimmBackground.SetActive(false);
    }

    public void openMenu(SongBlockSettings settings)
    {
        currentSettings = settings;
        this.gameObject.SetActive(true);
        DimmBackground.SetActive(true);
        updateDisplayedValues();
    }

    void updateDisplayedValues()
    {
        volumeText.text = Mathf.Round(currentSettings.volume * 100f).ToString() + "%";
        fadeInText.text = Math.Round(currentSettings.fadeIn, 1).ToString("F1") + "s";
        fadeOutText.text = Math.Round(currentSettings.fadeOut, 1).ToString("F1") + "s";
    }

    void mute(bool value)
    {
        if (currentSettings == null) { Debug.LogWarning("No settings selected to edit"); return; }
        currentSettings.muted = value;
    }
    
    void volumeUp()
    {
        if (currentSettings == null) { Debug.LogWarning("No settings selected to edit"); return; }
        currentSettings.volume = Mathf.Clamp(currentSettings.volume + 0.05f, 0f, 1f);
        currentSettings.volume = (float)Math.Round(currentSettings.volume, 2);
        updateDisplayedValues();
    }

    void volumeDown()
    {
        if (currentSettings == null) { Debug.LogWarning("No settings selected to edit"); return; }
        currentSettings.volume = Mathf.Clamp(currentSettings.volume - 0.05f, 0f, 1f);
        currentSettings.volume = (float)Math.Round(currentSettings.volume, 2);
        updateDisplayedValues();
    }

    void fadeInUp()
    {
        if (currentSettings == null) { Debug.LogWarning("No settings selected to edit"); return; }
        currentSettings.fadeIn = Mathf.Clamp(currentSettings.fadeIn + 0.5f, 0f, 5f);
        currentSettings.fadeIn = (float)Math.Round(currentSettings.fadeIn, 1);
        updateDisplayedValues();
        Debug.Log(currentSettings.fadeIn);
    }

    void fadeInDown()
    {
        if (currentSettings == null) { Debug.LogWarning("No settings selected to edit"); return; }
        currentSettings.fadeIn = Mathf.Clamp(currentSettings.fadeIn - 0.5f, 0f, 5f);
        currentSettings.fadeIn = (float)Math.Round(currentSettings.fadeIn, 1);
        updateDisplayedValues();
        Debug.Log(currentSettings.fadeIn);
    }

    void fadeOutUp()
    {
        if (currentSettings == null) { Debug.LogWarning("No settings selected to edit"); return; }
        currentSettings.fadeOut = Mathf.Clamp(currentSettings.fadeOut + 0.5f, 0f, 5f);
        currentSettings.fadeOut = (float)Math.Round(currentSettings.fadeOut, 1);
        updateDisplayedValues();
    }

    void fadeOutDown()
    {
        if (currentSettings == null) { Debug.LogWarning("No settings selected to edit"); return; }
        currentSettings.fadeOut = Mathf.Clamp(currentSettings.fadeOut - 0.5f, 0f, 5f);
        currentSettings.fadeOut = (float)Math.Round(currentSettings.fadeOut, 1);
        updateDisplayedValues();
    }
}
