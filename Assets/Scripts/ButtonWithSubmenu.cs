using UnityEngine;
using UnityEngine.UI;

public class ButtonWithSubmenu : MonoBehaviour
{
    [SerializeField] GameObject submenuGameObject;
    [SerializeField] Button submenuBackground;
    Toggle menuToggle;

    void Start()
    {
        submenuBackground.gameObject.SetActive(false);

        menuToggle = GetComponent<Toggle>();
        menuToggle.onValueChanged.AddListener(toggleSubmenu);
        submenuBackground.onClick.AddListener(backgroundClicked);
    }

    void toggleSubmenu(bool visibility)
    {
        submenuGameObject.SetActive(visibility);

        // if this menu is selected then find other ones and deactivate then
        if (visibility == true)
        {
            ButtonWithSubmenu[] allButtons = FindObjectsByType<ButtonWithSubmenu>(FindObjectsSortMode.None);
            foreach (ButtonWithSubmenu button in allButtons)
            {
                if (button != this) { button.menuToggle.isOn = false; }
            }
        }

        submenuBackground.gameObject.SetActive(visibility);
    }

    void backgroundClicked()
    {
        menuToggle.isOn = false;
        submenuBackground.gameObject.SetActive(false);
    }
}
