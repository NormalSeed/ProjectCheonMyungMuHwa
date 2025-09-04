using System.Collections.Generic;
using UnityEngine;

public class MonsterModel : MonoBehaviour
{
  public ObservableProperty<double> CurHealth;

  [SerializeField] protected MonsterTextures tex;
  [SerializeField] protected SpriteRenderer[] renderers;

  [SerializeField] public float AttackDistance; //공격하기 위해 멈추는 사정거리
  public float AttackDistanceWithClearance => AttackDistance + 0.5f; //공격 중에 벗어날 경우 다시 추적하기 위한 사정거리 (Attackdistance보다 약간 높게)
  [SerializeField] public float AttackDelay; //공격 쿨타임
  [SerializeField] public float MoveSpeed; //이동속도

  public MonsterModelBaseSO BaseModel { get; set; }
  public virtual void InitSprite(int stage)
  {

    if (stage < 100)
    {
      SetSprite(tex.Orc_1);
    }
    else if (stage < 200)
    {
      SetSprite(tex.Orc_2);
    }
    else if (stage < 300)
    {
      SetSprite(tex.Orc_3);
    }
    else if (stage < 400)
    {
      SetSprite(tex.Orc_4);
    }
  }
  protected void SetSprite(Dictionary<string, Sprite> dict)
  {
    renderers[0].sprite = dict["Body"];
    renderers[1].sprite = dict["Head"];
    renderers[2].sprite = dict["Arm_L"];
    renderers[3].sprite = dict["Arm_R"];
    renderers[4].sprite = dict["Foot_R"];
    renderers[5].sprite = dict["Foot_L"];
  }

  public virtual void SetSpriteColor(Color color)
  {
    foreach (SpriteRenderer sr in renderers)
    {
      sr.color = color;
    }
  }
}
