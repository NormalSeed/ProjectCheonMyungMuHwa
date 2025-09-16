using UnityEngine.UI;
using UnityEngine;
using System;
using UnityEngine.Events;

public class DungeonSelectPanel : MonoBehaviour
{

    [SerializeField] DungeonSelectButton[] selectButtons;

    

    public void RegisteButtons(UnityAction<CurrencyDungeonType> act)
    {
        foreach (DungeonSelectButton btn in selectButtons)
        {
            btn.Register(act);
        }
    }
}
