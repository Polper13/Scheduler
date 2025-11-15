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

    public SongBlockSettings(bool muted, float volume, float fadeIn, float fadeOut)
    {
        this.muted = muted;
        this.volume = volume;
        this.fadeIn = fadeIn;
        this.fadeOut = fadeOut;
    }
}

public class SongBlockSettingsMenu : MonoBehaviour
{
    [SerializeField] GameObject dimmBackground;
    [SerializeField] Button clickAwayButton;

    [Header("Preview")]
    [SerializeField] GameObject previewGameObject;
    [SerializeField] Toggle playToggle;
    [SerializeField] GameObject playIconOn;
    [SerializeField] GameObject playIconOff;
    [SerializeField] Slider progressBar;
    [SerializeField] TMP_Text previewTimeText;

    [Header("Volume")]
    [SerializeField] TMP_Text volumeTitleText;
    [SerializeField] Toggle muteToggle;
    [SerializeField] Button volumeUpButton;
    [SerializeField] Button volumeDownButton;
    [SerializeField] TMP_InputField volumeInputField;

    [Header("FadeIn")]
    [SerializeField] TMP_Text fadeInTitleText;
    [SerializeField] Button FadeInUpButton;
    [SerializeField] Button FadeInDownButton;
    [SerializeField] TMP_InputField fadeInInputField;

    [Header("FadeOut")]
    [SerializeField] TMP_Text fadeOutTitleText;
    [SerializeField] Button FadeOutUpButton;
    [SerializeField] Button FadeOutDownButton;
    [SerializeField] TMP_InputField fadeOutInputField;

    SongBlock currentSongBlock;
    SongBlockSettings currentSettings;

    void Start()
    {
        playIconOn.SetActive(true);
        playIconOff.SetActive(false);

        clickAwayButton.onClick.AddListener(closeMenu);

        progressBar.onValueChanged.AddListener(onProgressBarChange);
        playToggle.onValueChanged.AddListener(play);

        muteToggle.onValueChanged.AddListener(mute);
        volumeUpButton.onClick.AddListener(volumeUp);
        volumeDownButton.onClick.AddListener(volumeDown);
        volumeInputField.onEndEdit.AddListener(checkVolumeInput);

        FadeInUpButton.onClick.AddListener(fadeInUp);
        FadeInDownButton.onClick.AddListener(fadeInDown);
        fadeInInputField.onEndEdit.AddListener(checkFadeInInput);

        FadeOutUpButton.onClick.AddListener(fadeOutUp);
        FadeOutDownButton.onClick.AddListener(fadeOutDown);
        fadeOutInputField.onEndEdit.AddListener(checkFadeOutInput);
    }

    void Update()
    {
        if (previewGameObject.activeInHierarchy && currentSongBlock != null && currentSongBlock.isPlaying)
        {
            float progress = currentSongBlock.getPreviewProgress();

            if (progress > 1f)
            {
                currentSongBlock.stop();
                playToggle.isOn = false;
            }
            else
            {
                progressBar.value = progress;
                updatePreviewTime();
            }
        }
    }

    static string formatSeconds(float seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        if (timeSpan.Hours > 0)
        {
            return string.Format("{0}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
        else
        {
            return string.Format("{0}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
    }

    void closeMenu()
    {
        // currentSongBlock.stop();
        playToggle.isOn = false;

        this.gameObject.SetActive(false);
        dimmBackground.SetActive(false);
    }

    public void openMenu(SongBlock songBlock, bool thisPagePlaying, bool clipSelected)
    {
        currentSongBlock = songBlock;
        currentSettings = songBlock.settings;

        progressBar.value = 0f;
        this.gameObject.SetActive(true);
        previewGameObject.SetActive(!thisPagePlaying && clipSelected);
        dimmBackground.SetActive(true);

        updatePreviewTime();
        updateDisplayedValues();
    }

    void updateDisplayedValues()
    {
        volumeInputField.text = Mathf.Round(currentSettings.volume * 100f).ToString() + "%";
        fadeInInputField.text = Math.Round(currentSettings.fadeIn, 1).ToString("F1") + "s";
        fadeOutInputField.text = Math.Round(currentSettings.fadeOut, 1).ToString("F1") + "s";

        muteToggle.isOn = currentSettings.muted;
    }

    void updatePreviewTime()
    {
        float progress = progressBar.value;

        string passed = formatSeconds(progress * currentSongBlock.getAudioClipLength());
        string duration = formatSeconds(currentSongBlock.getAudioClipLength());
        previewTimeText.text = passed + " / " + duration;
    }

    void onProgressBarChange(float value)
    {
        if (previewGameObject.activeInHierarchy)
        {
            currentSongBlock.movePreview(value);
            updatePreviewTime();
        }
    }

    void play(bool value)
    {
        playIconOn.SetActive(!value);
        playIconOff.SetActive(value);

        if (value == false) { currentSongBlock.stop(); }
        if (value == true) { currentSongBlock.playPreview(progressBar.value); }
    }

    void mute(bool value)
    {
        if (currentSettings == null) { Debug.LogWarning("No settings selected to edit"); return; }
        currentSettings.muted = value;
    }
    
    void volumeUp()
    {
        if (currentSettings == null) { Debug.LogWarning("No settings selected to edit"); return; }
        float step = 0.05f - (float)(Math.Round(currentSettings.volume * 100) % 5f) / 100f;
        currentSettings.volume = Mathf.Clamp(currentSettings.volume + step, 0f, 1f);
        currentSettings.volume = (float)Math.Round(currentSettings.volume, 2);
        updateDisplayedValues();
    }

    void volumeDown()
    {
        if (currentSettings == null) { Debug.LogWarning("No settings selected to edit"); return; }
        float step = (Math.Round(currentSettings.volume % 0.05f, 2) > 0f) ? currentSettings.volume % 0.05f : 0.05f;
        currentSettings.volume = Mathf.Clamp(currentSettings.volume - step, 0f, 1f);
        currentSettings.volume = (float)Math.Round(currentSettings.volume, 2);
        updateDisplayedValues();
    }

    void checkVolumeInput(string input)
    {
        input = input.Replace("%", "").Trim();
        volumeTitleText.color = new Color(255f / 255f, 69f / 255f, 69f / 255f);
        if (string.IsNullOrEmpty(input)) { return; }

        int intValue;
        if (int.TryParse(input, out intValue) == false) { return; }

        volumeTitleText.color = new Color(221f / 255f, 221f / 255f, 221f / 255f);
        currentSettings.volume = Mathf.Clamp(intValue / 100f, 0f, 1f);
        currentSettings.volume = (float)Math.Round(currentSettings.volume, 2);
        updateDisplayedValues();
    }

    void fadeInUp()
    {
        if (currentSettings == null) { Debug.LogWarning("No settings selected to edit"); return; }
        float step = 0.5f - (float)(Math.Round(currentSettings.fadeIn * 10) % 5f) / 10f;
        currentSettings.fadeIn = Mathf.Clamp(currentSettings.fadeIn + step, 0f, 30f);
        currentSettings.fadeIn = (float)Math.Round(currentSettings.fadeIn, 1);
        updateDisplayedValues();
    }

    void fadeInDown()
    {
        if (currentSettings == null) { Debug.LogWarning("No settings selected to edit"); return; }
        float step = (Math.Round(currentSettings.fadeIn % 0.5f, 1) > 0f) ? currentSettings.fadeIn % 0.5f : 0.5f;
        currentSettings.fadeIn = Mathf.Clamp(currentSettings.fadeIn - step, 0f, 30f);
        currentSettings.fadeIn = (float)Math.Round(currentSettings.fadeIn, 1);
        updateDisplayedValues();
    }

    void checkFadeInInput(string input)
    {
        input = input.Replace("s", "").Replace(",", ".").Trim();
        fadeInTitleText.color = new Color(255f / 255f, 69f / 255f, 69f / 255f);
        if (string.IsNullOrEmpty(input)) { return; }

        float floatValue;
        var numberStyles = System.Globalization.NumberStyles.Float;
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        if (float.TryParse(input, numberStyles, culture, out floatValue) == false) { return; }

        fadeInTitleText.color = new Color(221f / 255f, 221f / 255f, 221f / 255f);
        currentSettings.fadeIn = Mathf.Clamp(floatValue, 0f, 30f);
        currentSettings.fadeIn = (float)Math.Round(currentSettings.fadeIn, 1);
        updateDisplayedValues();
    }

    void fadeOutUp()
    {
        if (currentSettings == null) { Debug.LogWarning("No settings selected to edit"); return; }
        float step = 0.5f - (float)(Math.Round(currentSettings.fadeOut * 10) % 5f) / 10f;
        currentSettings.fadeOut = Mathf.Clamp(currentSettings.fadeOut + step, 0f, 30f);
        currentSettings.fadeOut = (float)Math.Round(currentSettings.fadeOut, 1);
        updateDisplayedValues();
    }

    void fadeOutDown()
    {
        if (currentSettings == null) { Debug.LogWarning("No settings selected to edit"); return; }
        float step = (Math.Round(currentSettings.fadeOut % 0.5f, 1) > 0f) ? currentSettings.fadeOut % 0.5f : 0.5f;
        currentSettings.fadeOut = Mathf.Clamp(currentSettings.fadeOut - step, 0f, 30f);
        currentSettings.fadeOut = (float)Math.Round(currentSettings.fadeOut, 1);
        updateDisplayedValues();
    }

    void checkFadeOutInput(string input)
    {
        input = input.Replace("s", "");
        fadeOutTitleText.color = new Color(255f / 255f, 69f / 255f, 69f / 255f);
        if (string.IsNullOrEmpty(input)) { return; }

        float floatValue;
        var numberStyles = System.Globalization.NumberStyles.Float;
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        if (float.TryParse(input, numberStyles, culture, out floatValue) == false) { return; }

        fadeOutTitleText.color = new Color(221f / 255f, 221f / 255f, 221f / 255f);
        currentSettings.fadeOut = Mathf.Clamp(floatValue, 0f, 30f);
        currentSettings.fadeOut = (float)Math.Round(currentSettings.fadeOut, 1);
        updateDisplayedValues();
    }
}
