using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using static UnityEngine.Rendering.DebugUI;

public class PartyManager : MonoBehaviour, IStartable
{
    public List<GameObject> partyMembers = new List<GameObject>();

    private Dictionary<string, CardInfo> partyInfo = new Dictionary<string, CardInfo>();

    private readonly int MaxPartySize = 5;

    public int PartySixe = 1;

    public event Action<Dictionary<string, CardInfo>> partySet;


    #region Unity LifeCycle
    private void Awake() { }

    public void Start() { }
    #endregion

    #region Public 
    public void AddMember(GameObject member)
    {
        if (partyMembers.Count < MaxPartySize && !partyMembers.Contains(member))
        {
            partyMembers.Add(member);
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
            partyMembers.Remove(member);
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