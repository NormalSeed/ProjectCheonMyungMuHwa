using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using static UnityEngine.Rendering.DebugUI;


public class PartyManager : IStartable
{
    public List<GameObject> partyMembers = new List<GameObject>();
    private Dictionary<string, CardInfo> partyInfo = new Dictionary<string, CardInfo>();

    private readonly int MaxPartySize = 5;

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
        partySet?.Invoke(partyInfo);
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