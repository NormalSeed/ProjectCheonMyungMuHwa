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


    private List<CardInfo> backupMembers = new(); // 기존 멤버 백업용
    public List<CardInfo> MembersID = new();// 실제 배치용
    public List<PlayerController> players = new();

    public List<SynergyInfo> activeSynergies = new();           // 현재 활성화된 시너지 정보
    public SynergyUI synergyUI;
    public SynergyExplainUI explainUI;


    private readonly int MaxPartySize = 5;                      // 파티 최대 편성 수 
    private int partySize = 1;                                  // 현재 편성된 파티인원
    public int PartySize { get { return partySize; } }           //현재 편성인원 외부 참조

    private bool isHeroSetNow = false;                          // 파티 편성 진행중 여부
    public bool IsHeroSetNow { get { return isHeroSetNow; } }   //파티 편성 진행중 외부 참조
    [SerializeField] private HeroUI heroUI;

    #region Unity LifeCycle
    public void Start()
    {
        PartyLoadData();
    }
    #endregion

    #region Public 
    // 멤버를 추가하는 로직
    public void AddMember(CardInfo input)
    {
        if (MembersID.Contains(input) || MembersID.Count >= MaxPartySize)
            return;
        MembersID.Add(input);
        // UI 갱신
        if (heroUI != null)
        {
            heroUI.SetSlot(input, MembersID.Count - 1);
        }
    }

    public void RemoveMember(CardInfo input)
    {
        int listOrder = MembersID.IndexOf(input);
        if (listOrder < 0) return;

        MembersID.RemoveAt(listOrder);

        // 해당 슬롯 비우기z
        if (heroUI != null)
            heroUI.SetSlot(null, listOrder);

        // 이후 슬롯들 재정렬
        for (int i = listOrder; i < MembersID.Count; i++)
        {
            heroUI.SetSlot(MembersID[i], i);
        }

        // 마지막 슬롯 비우기
        if (MembersID.Count < MaxPartySize)
            heroUI.SetSlot(null, MembersID.Count);
    }
    public void PartyLoadUI()
    {
        for (int i = 0; i < MembersID.Count; i++)
        {
            heroUI.SetSlot(MembersID[i], i);
        }
    }


    public void PartyInit()
    {
        //InGameManager.Instance.playerCount = 0;
        // 파티 추가
        for (int i = 0; i < players.Count; i++)
        {
            PlayerController controller = players[i];
            if (controller == null) continue;

            if (!string.IsNullOrEmpty(controller.charID.Value))
            {
                StatModifierManager.ClearAllModifiers(controller.charID.Value);
                StatModifierManager.ApplyToModel(controller.model);
            }

            if (i < MembersID.Count)
            {
                CardInfo card = MembersID[i];
                string heroID = card.HeroID;
                if (InGameManager.Instance != null)
                {
                    Transform alignRoot = InGameManager.Instance.alignPoint.transform;
                    Transform point = alignRoot.Find($"Point{i + 1}");

                    //controller.gameObject.SetActive(false);

                    if (controller.charID.Value == null || controller.charID.Value == string.Empty)
                    {
                        controller.transform.position = point.position;
                    }
                }
                controller.gameObject.SetActive(true);
                controller.charID.Value = heroID;
                controller.partyNum = i;
                //InGameManager.Instance.playerCount++;
                partySize++;

                Debug.Log($"파티 멤버 {controller.name}의 partyNum 설정됨: {controller.partyNum}");
            }
            else
            {
                controller.charID.Value = string.Empty;
                controller.gameObject.SetActive(false);
            }
            controller.synergyUI.UpdateDamageUI();
        }
        InGameManager.Instance.playerCount = MembersID.Count;
        TeamInfoUI.Instance.UpdateTeamUI();

        CheckSynergy();
    }


    // 파티 편성 진행 여부 트리거
    public void StartPartySetting()
    {
        // 맴버 리스트 초기화
        isHeroSetNow = true;
        backupMembers = new List<CardInfo>(MembersID);
    }
    public void StartWithoutPartySetting()
    {
        // 기존 멤버 복원
        MembersID = new List<CardInfo>(backupMembers);
        PartyLoadUI();
        CheckSynergy();
        isHeroSetNow = false;
    }

    public void EndPartySetting()
    {
        PartyInit();
        PartyUpload();
        backupMembers.Clear();
        isHeroSetNow = false;
        TeamInfoUI.Instance.UpdateTeamUI();
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
        foreach (var member in MembersID)
        {
            if (member != null)
            {
                HeroFaction faction = member.faction;

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

        // 시너지가 없을 경우
        if (activeSynergies.Count == 0)
        {
            synergyUI.SetSynergySkillButtonState(null); // 또는 비활성화 처리
            return;
        }

        // 1. 가장 높은 stage 찾기
        int maxStage = activeSynergies.Max(s => s.stage);

        // 2. 해당 stage를 가진 시너지들 필터링
        var topSynergies = activeSynergies
            .Where(s => s.stage == maxStage)
            .ToList();

        // 3. 우선순위에 따라 정렬
        HeroFaction[] priority = { HeroFaction.S, HeroFaction.J, HeroFaction.M };
        topSynergies.Sort((a, b) =>
            Array.IndexOf(priority, a.faction).CompareTo(Array.IndexOf(priority, b.faction))
        );

        // 4. 최종 선택된 시너지
        SynergyInfo selected = topSynergies.First();

        // 5. SynergyUI에서 합격 스킬 활성화
        synergyUI.SetSynergySkillButtonState(selected);
    }

    /// <summary>
    /// 적용중인 시너지 초기화 메서드
    /// </summary>
    private void ClearSynergy()
    {
        foreach (var member in MembersID)
        {
            if (member == null)
                continue;

            string charID = member.HeroID;
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
        foreach (var member in MembersID)
        {
            if (member == null) continue;

            string targetCharID = member.HeroID;

            var player = players.Find(p => p.charID.Value == targetCharID);
            if (player == null || player.model == null || player.model.modelSO == null)
                continue;

            // 시너지 이름을 originID로 사용
            string synergyID = $"{faction}_Synergy_Stage{stage}";

            switch (faction)
            {
                case HeroFaction.J:
                    if (stage == 1)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.Attack, 0.2f, ModifierSource.Synergy, synergyID, true));
                    }
                    else if (stage == 2)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.Attack, 0.4f, ModifierSource.Synergy, synergyID, true));
                    }
                    else if (stage == 3)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.Attack, 0.7f, ModifierSource.Synergy, synergyID, true));
                    }
                    break;

                case HeroFaction.S:
                    if (stage == 1)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.BDamage, 0.2f, ModifierSource.Synergy, synergyID));
                    }
                    else if (stage == 2)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.BDamage, 0.4f, ModifierSource.Synergy, synergyID));
                    }
                    else if (stage == 3)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.BDamage, 0.7f, ModifierSource.Synergy, synergyID));
                    }
                    break;

                case HeroFaction.M:
                    if (stage == 1)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.SkillDamage, 0.15f, ModifierSource.Synergy, synergyID));
                    }
                    else if (stage == 2)
                    {
                        StatModifierManager.ApplyModifier(targetCharID,
                            new StatModifier(StatType.SkillDamage, 0.5f, ModifierSource.Synergy, synergyID));
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

    private void PartyUpload()
    {
        CurrencyManager.Instance.SavePartyToFirebase(MembersID);
    }

    private void PartyLoadData()
    {
        CurrencyManager.Instance.LoadPartyFromFirebase(MembersID);
    }
    #endregion
    #endregion
}
/*
    TODO : 파티편성 필요 작업 목록
        드래그 드롭으로 순서를 변경하는 기능     
 */