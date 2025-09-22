using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamInfoUI : MonoBehaviour
{
    [Header("이미지 리스트")]
    [SerializeField] List<Sprite> rarities = new();
    [SerializeField] List<Sprite> characterSprites = new();
    [SerializeField] List<Sprite> factionSprites = new();

    [Header("보유 필드")]
    [SerializeField] List<Image> backGrounds = new();
    [SerializeField] List<Image> characters = new();
    [SerializeField] List<Image> factions = new();
}
