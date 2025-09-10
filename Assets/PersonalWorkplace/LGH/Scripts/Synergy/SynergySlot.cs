using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SynergySlot : MonoBehaviour
{
    [SerializeField] public List<Sprite> jIcons;
    [SerializeField] public List<Sprite> sIcons;
    [SerializeField] public List<Sprite> mIcons;
    public Dictionary<string, List<Sprite>> synergyIconMap = new();

    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
        synergyIconMap.Clear();

        Debug.Log("Awake에서 아이콘 맵 초기화 시작");

        if (jIcons != null && jIcons.Count > 0)
        {
            synergyIconMap["J"] = jIcons;
            Debug.Log("J 아이콘 등록 완료");
        }

        if (sIcons != null && sIcons.Count > 0)
        {
            synergyIconMap["S"] = sIcons;
            Debug.Log("S 아이콘 등록 완료");
        }

        if (mIcons != null && mIcons.Count > 0)
        {
            synergyIconMap["M"] = mIcons;
            Debug.Log("M 아이콘 등록 완료");
        }

        Debug.Log($"최종 등록된 키: {string.Join(", ", synergyIconMap.Keys)}, 슬롯이름 : {this.name}");
    }

    public void SetData(string synergyKey, int stage)
    {
        List<Sprite> iconList = synergyIconMap.TryGetValue(synergyKey, out var list) ? list : null;

        if (iconList == null || iconList.Count == 0)
        {
            Debug.LogWarning($"시너지 아이콘 null임: {synergyKey}");
            return;
        }
        else if (iconList.Count == 0)
        {
            Debug.LogWarning($"시너지 아이콘에 든 게 없음: {synergyKey}");
        }

        int index = Mathf.Clamp(stage - 1, 0, iconList.Count - 1);
        img.sprite = iconList[index] as Sprite;
    }
}
