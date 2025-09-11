using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer.Unity;

public class PartyManager : MonoBehaviour, IStartable
{
    #region SingleTon
    public static PartyManager Instance { get; private set; }
    public PartyManager()
    {
        Instance = this;
    }
    #endregion


    public List<GameObject> partyMembers = new List<GameObject>();
    public List<string> MembersID = new List<string>();

    public List<PlayerController> players = new();
    private readonly int MaxPartySize = 5;                      // 파티 최대 편성 수 


    private bool isHeroSetNow = false;                          // 파티 편성 진행중 여부
    public bool IsHeroSetNow { get { return isHeroSetNow; } }   //파티 편성 진행중 외부 참조

    private int partySixe = 1;                                  // 현재 편성된 파티인원
    public int PartySize { get { return partySixe; } }           //현재 편성인원 외부 참조

    public event Action<Dictionary<string, CardInfo>> partySet;

    public List<SynergyInfo> activeSynergies = new();           // 현재 활성화된 시너지 정보
    public SynergyUI synergyUI;
    public SynergyExplainUI explainUI;

    #region Unity LifeCycle
    private void Awake() { }

    public void Start()
    {
        CurrencyManager.OnInitialized += HandleCurrencyReady;
    }
    private void OnDestroy()
    {
        CurrencyManager.OnInitialized -= HandleCurrencyReady;
    }
    #endregion

    #region Public 
    public void AddMember(GameObject member)
    {
        int activeCount = partyMembers.Count(m => m != null);

        if (activeCount < MaxPartySize && !partyMembers.Contains(member))
        {
            // 현재 추가될 멤버가 List의 몇번째에 있는지 체크해서
            int listOrder = 0;

            int emptyIndex = partyMembers.FindIndex(m => m == null);
            if (emptyIndex >= 0)
            {
                partyMembers[emptyIndex] = member;
                listOrder = emptyIndex;
            }
            else
            {
                partyMembers.Add(member);
                listOrder = partyMembers.Count - 1;
            }
            InGameManager.Instance.playerCount++;

            partySixe++;

            var heroInfo = member.GetComponent<HeroInfoSetting>().chardata;
            

            // players 리스트 안에 동일한 순서에 있는 PlayerController 안의 charID를 HeroID로 변경시킴
            PlayerController controller = players[listOrder];

            if (controller != null)
            {
                controller.gameObject.SetActive(true);
                controller.charID.Value = heroInfo.HeroID;
            }
            // 그 후 해당 PlayerController 안의 partyNum을 변경시킨다.
            if (heroInfo != null)
            {
                controller.partyNum = listOrder;
                Debug.Log($"추가된 멤버 {controller.name}의 partyNum 설정됨: {controller.partyNum}");
            }

            if (controller.model != null && controller.model.modelSO != null)
            {
                CheckSynergy();
            }
        }
    }

    public void RemoveMember(GameObject member)
    {
        if (partyMembers.Contains(member))
        {
            var heroInfo = member.GetComponent<HeroInfoSetting>().chardata;
            // 현재 제거될 멤버가 List의 몇번째에 있는지 체크해서
            int listOrder = partyMembers.IndexOf(member);
            // players 리스트 안에 동일한 순서에 있는 PlayerController 안의 charID를 HeroID로 변경시킴
            PlayerController controller = players[listOrder];
            controller.gameObject.SetActive(false);

            if (controller != null)
            {
                controller.charID.Value = string.Empty;
            }

            partyMembers[listOrder] = null;
            InGameManager.Instance.playerCount--;
            partySixe--;
        }
        CheckSynergy();
    }
    public void PartyInit()
    {
        // 파티 멤버 초기화
        for (int i = 0; i < partyMembers.Count; i++)
        {
            var controller = players[i].GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.partyNum = i;
                Debug.Log($"파티 멤버 {controller.name}의 partyNum 설정됨: {i}");
            }
            else
            {
                Debug.LogWarning($"파티 멤버 {partyMembers[i].name}에 PlayerController가 없습니다.");
            }
        }
    }

    // 파티 편성 진행 여부 트리거
    public void StartPartySetting()
    {
        // 맴버 리스트 초기화
        MembersID = new List<string>();
        isHeroSetNow = true;
    }
    public void EndPartySetting()
    {
        PartyUpload();
        isHeroSetNow = false;
    }


    // 추후 AddMember와 합칠 생각
    public void AddMemberID(string memberID)
    {
        //  현재 맴버수 체크
        if (MaxPartySize <= MembersID.Count)
            return;
        MembersID.Add(memberID);
    }

    public void RemoveMemberID(string memberID)
    {
        MembersID.Remove(memberID);
    }
    /// <summary>
    /// 파티를 자동 편성하는 기능입니다.
    /// </summary>
    public void AutoPartySetting()
    {
        // 시너지 체크 위에 로직을 짜주세요
        CheckSynergy();
    }
    #endregion

    #region Private
    #region Synergy
    public void CheckSynergy()
    {
        ClearSynergy();
        activeSynergies.Clear(); // UI용 리스트 초기화

        Dictionary<HeroFaction, int> factionCounts = new();
        foreach (var member in partyMembers)
        {
            if (member != null)
            {
                var cardInfo = member.GetComponent<HeroInfoSetting>().chardata;
                if (cardInfo == null)
                    continue;

                HeroFaction faction = cardInfo.faction;

                if (!factionCounts.ContainsKey(faction))
                    factionCounts[faction] = 0;

                factionCounts[faction]++;
            }
        }

        foreach (var kvp in factionCounts)
        {
            int stage = 0;

            if (kvp.Value == 5 && kvp.Key != HeroFaction.M)
                stage = 3;
            else if (kvp.Value >= 4 && kvp.Key == HeroFaction.M)
                stage = 2;
            else if (kvp.Value >= 3 && (kvp.Key == HeroFaction.J || kvp.Key == HeroFaction.S))
                stage = 2;
            else if (kvp.Value >= 2 && (kvp.Key == HeroFaction.J || kvp.Key == HeroFaction.S))
                stage = 1;
            else if (kvp.Value >= 1 && kvp.Key == HeroFaction.M)
                stage = 1;

            if (stage > 0)
            {
                ActiveSynergy(kvp.Key, stage);
                activeSynergies.Add(new SynergyInfo(kvp.Key, stage, kvp.Value)); // UI용 데이터 저장
            }
        }
        synergyUI.UpdateSynergyUI(activeSynergies);
        explainUI.UpdateExplainUI(activeSynergies);
    }
    
    /// <summary>
    /// 적용중인 시너지 초기화 메서드
    /// </summary>
    private void ClearSynergy()
    {
        foreach (var member in partyMembers)
        {
            if (member == null)
                continue;

            var cardInfo = member.GetComponent<HeroInfoSetting>().chardata;
            if (cardInfo == null)
                continue;

            string charID = cardInfo.HeroID;
            StatModifierManager.RemoveModifiers(charID, ModifierSource.Synergy);
            var player = players.Find(p => p.charID.Value == charID);
            if (player != null)
                StatModifierManager.ApplyToModel(player.model);
        }
    }

    /// <summary>
    /// 시너지 활성화 메서드
    /// </summary>
    /// <param name="faction">문파</param>
    /// <param name="stage">시너지 단계</param>
    private void ActiveSynergy(HeroFaction faction, int stage)
    {
        foreach (var member in partyMembers)
        {
            if (member == null) continue;

            var cardInfo = member.GetComponent<HeroInfoSetting>().chardata;
            if (cardInfo == null)
                continue;

            string targetCharID = cardInfo.HeroID;

            var player = players.Find(p => p.charID.Value == targetCharID);
            if (player == null || player.model?.modelSO == null)
                continue;

            // 시너지 이름을 originID로 사용
            string synergyID = $"{faction}_Synergy_Stage{stage}";

            switch (faction)
            {
                case HeroFaction.J:
                    if (stage == 1)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.Attack, 0.2, ModifierSource.Synergy, synergyID, true));
                    }
                    else if (stage == 2)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.Attack, 0.4, ModifierSource.Synergy, synergyID, true));
                    }
                    else if (stage == 3)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.Attack, 0.7, ModifierSource.Synergy, synergyID, true));
                    }
                    break;

                case HeroFaction.S:
                    if (stage == 1)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.BDamage, 0.2, ModifierSource.Synergy, synergyID));
                    }
                    else if (stage == 2)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.BDamage, 0.4, ModifierSource.Synergy, synergyID));
                    }
                    else if (stage == 3)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.BDamage, 0.7, ModifierSource.Synergy, synergyID));
                    }
                    break;

                case HeroFaction.M:
                    if (stage == 1)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.SkillDamage, 0.15, ModifierSource.Synergy, synergyID));
                    }
                    else if (stage == 2)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.SkillDamage, 0.5, ModifierSource.Synergy, synergyID));
                    }
                    else if (stage == 3)
                    {
                        Debug.Log($"마교는 3단계 시너지가 없습니다.");
                    }
                    break;
            }

            // Modifier 적용 후 최종 능력치 갱신
            StatModifierManager.ApplyToModel(player.model);
            Debug.Log($"시너지 적용 완료: {faction} {targetCharID} (단계 {stage})");
        }
    }

    private void HandleCurrencyReady()
    {
        PartyLoadData();
        PartyInit();
    }


    private void PartyUpload()
    {
        CurrencyManager.Instance.SavePartyToFirebase(MembersID);
    }

    private void PartyLoadData()
    {
        CurrencyManager.Instance.LoadPartyIdsFromFirebase(MembersID);
    }
    #endregion
    #endregion
}

/*
    TODO : 파티편성 필요 작업 목록
        드래그 드롭으로 순서를 변경하는 기능     
 */