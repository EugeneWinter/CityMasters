using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildMenuButton : MonoBehaviour
{
    public BuildingData data;
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    public void Setup(BuildingData d)
    {
        data = d;
        if (iconImage) iconImage.sprite = d.icon;
        if (nameText) nameText.text = d.buildingName;
        if (costText) costText.text = $"{d.cost}$";
    }

    public void OnClick()
    {
        if (data != null && BuildingPlacer.Instance != null)
        {
            BuildingPlacer.Instance.SelectBuilding(data);
        }
    }
}