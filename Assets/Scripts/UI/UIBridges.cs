using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ActionButtonBridge : MonoBehaviour
{
    public bool isRoad;

    void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveListener(OnClick);
            btn.onClick.AddListener(OnClick);
        }
    }

    public void OnClick()
    {
        if (BuildingPlacer.Instance == null) return;
        if (isRoad)
            BuildingPlacer.Instance.SelectRoad();
        else
            BuildingPlacer.Instance.SelectDemolish();
    }
}