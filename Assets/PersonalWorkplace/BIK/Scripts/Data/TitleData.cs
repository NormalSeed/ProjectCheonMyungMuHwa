using UnityEngine;

[CreateAssetMenu(fileName = "TitleData", menuName = "Game/TitleData")]
public class TitleData : ScriptableObject
{
    public string Id;          // 고유 ID
    public string DisplayName; // UI 표시 이름
    public UnlockCondition[] Conditions; // 해금 조건들
    public string SpriteKey;   // 어드레서블 키 (선택적, 없으면 Text만)
}
