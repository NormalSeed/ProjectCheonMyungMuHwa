using System.Collections.Generic;
using UnityEngine;


//선택한 던전의 종류별로 구분짓는 용도
public class CurrencyDungeonPoint : MonoBehaviour
{
    [SerializeField] CurrencyDungeonType type;
    [SerializeField] AlignPoint alignPoint;
    public CurrencyDungeonType Type => type;
    public Vector2 Pos => transform.position;
    public List<GameObject> points => alignPoint.alignPoints;
}
