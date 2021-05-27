using UnityEngine.UI;
using UnityEngine;

public class DetailsManager : MonoBehaviour
{
    #region Singleton

    public static DetailsManager instance;
    void Awake() { instance = this; }

    #endregion

    public Button returnButton;
    public Button useButton;

    [Space]

    public Text titleText;
    public Text descText;
    public Image itemImage;

    [HideInInspector]
    public ItemSettings currentItem;

    void Start()
    {
        useButton.enabled = false;
        itemImage.sprite = null;

        titleText.text = "";
        descText.text = "";
    }

    void Update()
    {
        //if (useButton.onClicked && currentItem != null) { currentItem.Use(); GameManager.ToggleUISection(0); }
        //if (returnButton.onClicked) { GameManager.ToggleUISection(2); }
    }

    public void SetItem(ItemSettings itemSettings)
    {
        GameManager.ToggleUISection(5);
        currentItem = itemSettings;

        if (itemSettings.isUsableItem)
        {
            useButton.enabled = true;
            useButton.gameObject.SetActive(true);
        } else 
        {
            useButton.enabled = false;
            useButton.gameObject.SetActive(false);
        }

        itemImage.sprite = itemSettings.icon;

        titleText.text = itemSettings.name;
    }
}
