using UnityEngine;


[CreateAssetMenu(fileName = "BackgroundData", menuName = "Game/BackgroundData")]
public class BackgroundData : ScriptableObject
{
    public string Id;
    public string DisplayName;
    public UnlockCondition[] Conditions;
    public string SpriteKey;   // 어드레서블 키
}
