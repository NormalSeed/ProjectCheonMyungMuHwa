using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using VContainer;

public class TeamInfoUI : MonoBehaviour
{
    public static TeamInfoUI Instance { get; private set; }

    [SerializeField] List<PlayerController> players = new();

    [Header("이미지 리스트")]
    [SerializeField] List<Sprite> rarities = new();
    [SerializeField] List<Sprite> factionSprites = new();

    [Header("보유 필드")]
    [SerializeField] List<Image> backGrounds = new();
    [SerializeField] List<Image> characters = new();
    [SerializeField] List<Image> factions = new();
    [SerializeField] List<Slider> healths = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void UpdateTeamUI()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i].gameObject.activeSelf)
            {
                backGrounds[i].sprite = rarities[0];
                characters[i].color = new Color(1f, 1f, 1f, 0f);
                factions[i].color = new Color(1f, 1f, 1f, 0f);
                healths[i].value = 0f;
                continue;
            }

            characters[i].color = new Color(1f, 1f, 1f, 1f);
            factions[i].color = new Color(1f, 1f, 1f, 1f);

            var modelSO = players[i].model.modelSO;

            // 희귀도 배경 설정
            int rarityIndex = modelSO.Rarity - 1;
            if (rarityIndex >= 0 && rarityIndex < rarities.Count)
                backGrounds[i].sprite = rarities[rarityIndex];

            // 캐릭터 스프라이트 설정
            string charID = modelSO.CharID;
            characters[i].sprite = HeroSprites.Instance.GetCharacterSprite(charID);

            // 문파 스프라이트 설정
            int factionIndex = 0;
            if (int.TryParse(modelSO.Faction, out int index))
            {
                factionIndex = index - 1;
            }
            if (factionIndex >= 0 && factionIndex < factions.Count)
                factions[i].sprite = factionSprites[factionIndex];

            // 캐릭터 체력 UI 설정
            healths[i].value = 1f;
        }
    }
}
