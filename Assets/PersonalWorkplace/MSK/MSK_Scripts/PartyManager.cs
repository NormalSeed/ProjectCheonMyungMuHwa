using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public List<GameObject> partyMembers = new List<GameObject>();
    private readonly int MaxPartySize = 5;


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
    #endregion

    #region Private
    private void CheakSynergy()
    {

    }
    #endregion
}