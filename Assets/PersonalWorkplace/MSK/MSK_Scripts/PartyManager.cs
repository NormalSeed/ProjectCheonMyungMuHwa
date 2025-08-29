using System;
using System.Collections.Generic;
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
        if (partyMembers.Count < MaxPartySize && !partyMembers.Contains(member))
        {
            partyMembers.Add(member);

            partySixe++;

            var heroInfo = member.GetComponent<HeroInfoSetting>().chardata;
            // 현재 추가될 멤버가 List의 몇번째에 있는지 체크해서
            int listOrder = partyMembers.Count - 1;
            // players 리스트 안에 동일한 순서에 있는 PlayerController 안의 charID를 HeroID로 변경시킴
            PlayerController controller = players[listOrder];
            controller.gameObject.SetActive(true);

            if (controller != null)
            {
                controller.charID.Value = heroInfo.HeroID;
            }
            // 그 후 해당 PlayerController 안의 partyNum을 변경시킨다.
            if (heroInfo != null)
            {
                controller.partyNum = partyMembers.Count - 1;
                Debug.Log($"추가된 멤버 {controller.name}의 partyNum 설정됨: {controller.partyNum}");
            }
        }
    }

    public void RemoveMember(GameObject member)
    {
        if (partyMembers.Contains(member))
        {
            partyMembers.Remove(member);
            partySixe--;
        }
    }
    public void PartyInit()
    {
        // 파티 멤버 초기화
        for (int i = 0; i < partyMembers.Count; i++)
        {
            var controller = partyMembers[i].GetComponent<PlayerController>();
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

    }
    #endregion

    #region Private
    private void CheakSynergy()
    {
        foreach (var member in partyMembers)
        {

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
}

/*
    TODO : 파티편성 필요 작업 목록
        드래그 드롭으로 순서를 변경하는 기능
        
 */