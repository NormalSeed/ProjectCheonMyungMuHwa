using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SynergyExplainUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI jExplainStage1;
    [SerializeField] private TextMeshProUGUI jExplainStage2;
    [SerializeField] private TextMeshProUGUI jExplainStage3;

    [SerializeField] private TextMeshProUGUI sExplainStage1;
    [SerializeField] private TextMeshProUGUI sExplainStage2;
    [SerializeField] private TextMeshProUGUI sExplainStage3;

    [SerializeField] private TextMeshProUGUI mExplainStage1;
    [SerializeField] private TextMeshProUGUI mExplainStage2;

    private void OnEnable()
    {
        UpdateExplainUI(PartyManager.Instance.activeSynergies);
    }

    public void UpdateExplainUI(List<SynergyInfo> info)
    {
        // 기본 색상 설정
        Color activeColor = new Color32(255, 255, 255, 255); // 밝은 색
        Color inactiveColor = new Color32(120, 120, 120, 255); // 어두운 색

        // 모든 텍스트를 기본적으로 inactiveColor로 초기화
        jExplainStage1.color = inactiveColor;
        jExplainStage2.color = inactiveColor;
        jExplainStage3.color = inactiveColor;

        sExplainStage1.color = inactiveColor;
        sExplainStage2.color = inactiveColor;
        sExplainStage3.color = inactiveColor;

        mExplainStage1.color = inactiveColor;
        mExplainStage2.color = inactiveColor;

        // 리스트가 비어있지 않다면 처리
        if (info != null && info.Count > 0)
        {
            foreach (var synergy in info)
            {
                switch (synergy.faction)
                {
                    case HeroFaction.J:
                        if (synergy.stage == 1) jExplainStage1.color = activeColor;
                        else if (synergy.stage == 2) jExplainStage2.color = activeColor;
                        else if (synergy.stage == 3) jExplainStage3.color = activeColor;
                        break;

                    case HeroFaction.S:
                        if (synergy.stage == 1) sExplainStage1.color = activeColor;
                        else if (synergy.stage == 2) sExplainStage2.color = activeColor;
                        else if (synergy.stage == 3) sExplainStage3.color = activeColor;
                        break;

                    case HeroFaction.M:
                        if (synergy.stage == 1) mExplainStage1.color = activeColor;
                        else if (synergy.stage == 2) mExplainStage2.color = activeColor;
                        break;
                }
            }
        }
    }
}
