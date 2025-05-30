using UnityEngine;
using TMPro;

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
        
        GeneralSettings.uiScaling = scale / 100f;
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