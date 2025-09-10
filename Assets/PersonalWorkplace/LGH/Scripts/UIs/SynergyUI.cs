using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SynergyUI : MonoBehaviour
{
    public List<SynergySlot> slots; // Inspector에서 3개 연결
    public GameObject synergyExplainUI;
    public Button explainButton;

    private void Start()
    {
        foreach (SynergySlot slot in slots) 
        {
            slot.gameObject.SetActive(false);
        }

        explainButton.onClick.AddListener(ToggleExplainUI);
    }

    public void UpdateSynergyUI(List<SynergyInfo> synergyInfos)
    {
        foreach (var slot in slots)
            slot.gameObject.SetActive(false);

        for (int i = 0; i < synergyInfos.Count; i++)
        {
            var info = synergyInfos[i];
            slots[i].SetData(info.faction.ToString(), info.stage, info.count);
            slots[i].gameObject.SetActive(true);
        }
    }

    public void ToggleExplainUI()
    {
        synergyExplainUI.SetActive(!synergyExplainUI.activeSelf);
    }
}
