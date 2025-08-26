using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using static UnityEngine.Rendering.DebugUI;


public class PartyManager : MonoBehaviour, IStartable
{
    public List<CardInfo> partyMembers = new List<CardInfo>();

    private Dictionary<string, CardInfo> partyInfo = new Dictionary<string, CardInfo>();

    private readonly int MaxPartySize = 5;

    public int PartySixe = 1;

    public event Action<Dictionary<string, CardInfo>> partySet;


    #region Unity LifeCycle
    private void Awake() { }

    public void Start() { }
    #endregion

    #region Public 
    public void AddMember(CardInfo member)
    {
        if (partyMembers.Count < MaxPartySize && !partyMembers.Contains(member))
        {
            partyMembers.Add(member);
            partyInfo.Add(member.HeroID, member);
        }
    }

    public void RemoveMember(CardInfo member)
    {
        if (partyMembers.Contains(member))
        {
            partyMembers.Remove(member);
            partyInfo.Remove(member.HeroID);
        }
    }
    public void PartyInit()
    {
       // partySet?.Invoke(partyInfo);
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