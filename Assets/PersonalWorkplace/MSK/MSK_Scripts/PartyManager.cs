using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VContainer.Unity;
using static UnityEngine.Rendering.DebugUI;

public class PartyManager : MonoBehaviour, IStartable
{
    public static PartyManager Instance { get; private set; }

    public List<GameObject> partyMembers = new List<GameObject>();

    public List<PlayerController> players = new();

    private Dictionary<string, CardInfo> partyInfo = new Dictionary<string, CardInfo>();

    private readonly int MaxPartySize = 5;
    public int PartySixe = 1;

    public event Action<Dictionary<string, CardInfo>> partySet;

    public PartyManager( )
    {
        Instance = this;
    }

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
            PartySixe++;
            Debug.Log($"{member}파티 추가됨");
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
            Debug.Log($"{member}파티 제거됨");
            partyMembers.Remove(member);
            PartySixe--;
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