using UnityEngine;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.IO;
using System;

public static class GeneralSettings
{
    public static float uiScaling { get; private set; }

    private static CanvasScaler canvasScaler;
    private static string settingsJsonPath = Application.persistentDataPath + "/settings.json";

    private static GeneralSettingsData toGeneralSettingsData()
    {
        GeneralSettingsData data = new GeneralSettingsData
        {
            uiScaling = uiScaling
        };

        return data;
    }

    public static void loadSettings()
    {
        GeneralSettingsData loaded;

        if (File.Exists(settingsJsonPath)) // load from file
        {
            try
            {
                string json = File.ReadAllText(settingsJsonPath);
                loaded = JsonConvert.DeserializeObject<GeneralSettingsData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Couldnt load general settings: {ex.Message}");
                loaded = new GeneralSettingsData();
            }
        }
        else // load defaults
        {
            loaded = new GeneralSettingsData();
        }

        if (loaded != null) // load into static class
        {
            setUiScaling(loaded.uiScaling);
        }

    }

    public static void saveSettings()
    {
        GeneralSettingsData data = GeneralSettings.toGeneralSettingsData();
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(settingsJsonPath, json);
    }

    // uiScale 1f = 100%
    public static void setUiScaling(float value)
    {
        if (canvasScaler == null)
        {
            canvasScaler = GameObject.FindAnyObjectByType<CanvasScaler>();
            if (canvasScaler == null) { Debug.LogError("Couldnt find canvas scaler"); return; }
        }

        uiScaling = value;
        canvasScaler.scaleFactor = uiScaling;
        saveSettings();
    }

    private class GeneralSettingsData
    {
        public float uiScaling = 1f;
    }
}

public class GeneralSettingsMenu : MonoBehaviour
{
    [SerializeField] TMP_Dropdown uiScaleDropdown;

    void Start()
    {
        // default the values
        updateUIScaleDropDown();

        uiScaleDropdown.onValueChanged.AddListener(updateUIScale);
    }

    void updateUIScale(int index)
    {
        string value = uiScaleDropdown.options[index].text;
        value = value.Replace("%", "");
        
        int scale;
        if (int.TryParse(value, out scale) == false)
        {
            Debug.LogWarning("Invalid UIscale value: " + value);
            return;
        }
        
        GeneralSettings.setUiScaling(scale / 100f);
    }

    void updateUIScaleDropDown()
    {
        int index = 0;
        string value = (GeneralSettings.uiScaling * 100f).ToString("0") + "%";
        for (int i = 0; i < uiScaleDropdown.options.Count; i++)
        {
            if (uiScaleDropdown.options[i].text == value)
            {
                index = i;
                break;
            }
        }

        uiScaleDropdown.value = index;
        uiScaleDropdown.RefreshShownValue();
    }
}