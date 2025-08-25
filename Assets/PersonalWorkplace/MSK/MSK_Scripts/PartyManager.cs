using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public List<GameObject> partyMembers = new List<GameObject>();
    private readonly int MaxPartySize = 5;

    #region Unity LifeCycle
    private void Start()
    {
        PartyInit();
    }
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
        foreach (var partyMembers in partyMembers)
        {
            // 파티 멤버 초기화
        }
    }
    #endregion

    #region Private
    private void CheakSynergy( )
    {
        foreach (var member in partyMembers)
        {

        }
    }
    #endregion
}