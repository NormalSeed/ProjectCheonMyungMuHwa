using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.AnimatedValues;
using UnityEngine;

public class MonsterModel : MonoBehaviour
{
  public ObservableProperty<double> CurHealth;

  [SerializeField] MonsterTextures tex;
  [SerializeField] SpriteRenderer[] renderers;

  public MonsterModelBaseSO BaseModel { get; set; }
  public void InitSprite(int stage)
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
  private void SetSprite(Dictionary<string, Sprite> dict)
  {
    renderers[0].sprite = dict["Body"];
    renderers[1].sprite = dict["Head"];
    renderers[2].sprite = dict["Arm_L"];
    renderers[3].sprite = dict["Arm_R"];
    renderers[4].sprite = dict["Foot_R"];
    renderers[5].sprite = dict["Foot_L"];
  }

  public void SetSpriteColor(Color color)
  {
    foreach (SpriteRenderer sr in renderers)
    {
      sr.color = color;
    }
  }
}
