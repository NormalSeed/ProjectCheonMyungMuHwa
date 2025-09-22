using UnityEngine;

[CreateAssetMenu(fileName = "HeroTemplate", menuName = "Game/HeroTemplate")]
public class HeroTemplateSO : ScriptableObject
{
    public string heroId;
    public string heroName;
    public HeroRarity rarity;
    public PlayerModelSO modelSO;
    public CardInfo cardInfo;
}
