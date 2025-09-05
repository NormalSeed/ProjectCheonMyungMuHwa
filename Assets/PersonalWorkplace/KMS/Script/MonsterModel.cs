using System.Collections.Generic;
using UnityEngine;

public class MonsterModel : MonoBehaviour
{
  public ObservableProperty<double> CurHealth;

  [SerializeField] protected MonsterTextures tex;
  [SerializeField] protected SpriteRenderer[] renderers;

  private SpriteRenderer[] allRenderers;

  private List<Color> allColors;

  [SerializeField] public float AttackDistance; //공격하기 위해 멈추는 사정거리
  public float AttackDistanceWithClearance => AttackDistance + 0.5f; //공격 중에 벗어날 경우 다시 추적하기 위한 사정거리 (Attackdistance보다 약간 높게)
  [SerializeField] public float AttackDelay; //공격 쿨타임
  [SerializeField] public float MoveSpeed; //이동속도

  public MonsterModelBaseSO BaseModel { get; set; }

  void Awake()
  {
    allColors = new();
    allRenderers = GetComponentsInChildren<SpriteRenderer>();
    ColorInit();
  }
  protected void ColorInit()
  {
    allColors.Clear();
    foreach (SpriteRenderer sr in allRenderers)
    {
      allColors.Add(sr.color);
    }
  }
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
    if (color == Color.white) //원래 색으로 복원
    {
      for (int i = 0; i < allRenderers.Length; i++)
      {
        allRenderers[i].color = allColors[i];
      }
    }
    else
    {
      foreach (SpriteRenderer sr in allRenderers)
      {
        sr.color = color;
      }
    }

  }
}
