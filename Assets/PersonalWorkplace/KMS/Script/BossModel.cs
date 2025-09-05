using Unity.VisualScripting;
using UnityEngine;
public class BossModel : MonsterModel
{
  [SerializeField] SpriteRenderer[] clothes;
  private Color[] colors;

  public override void InitSprite(int stage)
  {

    if (stage < 100)
    {
      SetSprite(tex.Orc_1);
      clothes[0].color = Color.white;
      clothes[1].color = Color.green;
      clothes[2].color = Color.green;
      clothes[3].color = Color.white;
      clothes[4].color = Color.white;
    }
    else if (stage < 200)
    {
      SetSprite(tex.Orc_2);
      clothes[0].color = Color.red;
      clothes[1].color = Color.red;
      clothes[2].color = Color.red;
      clothes[3].color = Color.red;
      clothes[4].color = Color.red;
    }
    else if (stage < 300)
    {
      SetSprite(tex.Orc_3);
      clothes[0].color = Color.blue;
      clothes[1].color = Color.blue;
      clothes[2].color = Color.blue;
      clothes[3].color = Color.blue;
      clothes[4].color = Color.blue;
    }
    else if (stage < 400)
    {
      SetSprite(tex.Orc_4);
      clothes[0].color = Color.magenta;
      clothes[1].color = Color.magenta;
      clothes[2].color = Color.magenta;
      clothes[3].color = Color.magenta;
      clothes[4].color = Color.magenta;
    }
    ColorInit();
  }
}
