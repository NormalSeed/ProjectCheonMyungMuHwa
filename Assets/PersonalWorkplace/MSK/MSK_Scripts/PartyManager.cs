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
    public List<GameObject> newPartyMembers = new List<GameObject>();

    private Dictionary<string, CardInfo> partyInfo = new Dictionary<string, CardInfo>();


    private readonly int MaxPartySize = 5;              // 파티 최대 편성 수 
    private int partySixe = 1;                          // 현재 편성된 파티인원
    private bool isHeroSetNow = false;                  // 파티 편성 진행중 여부
    public bool IsHeroSetNow { get { return isHeroSetNow; } }   //파티 편성 진행중 외부 참조
    public int PartySize { get{ return partySixe; } }       //현재 편성인원 외부 참조
    public event Action<Dictionary<string, CardInfo>> partySet;


    #region Unity LifeCycle
    private void Awake() { }

    public void Start()
    {
        PartyInit();
    }
    #endregion

    #region Public 
    public void AddMember(GameObject member)
    {
        Debug.Log($"{member}파티 추가시도");
        if (partyMembers.Count < MaxPartySize && !partyMembers.Contains(member))
        {
            partyMembers.Add(member);
            partySixe++;
            Debug.Log($"{member}파티 추가됨");
            var controller = member.GetComponent<PlayerController>();
            if (controller != null)
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
            Debug.Log($"{member}파티 제거됨");
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

    public void StartPartySetting()
    {
        isHeroSetNow = true;
    }
    public void EndPartySetting()
    {
        isHeroSetNow = false;
    }
    #endregion

    #region Private
    private void CheakSynergy()
    {
        foreach (var member in partyMembers)
        {

        }
    }
    #endregion
}