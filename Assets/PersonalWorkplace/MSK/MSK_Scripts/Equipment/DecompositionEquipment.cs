using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class DecompositionEquipment : MonoBehaviour
{
    [Inject] private EquipmentService equipmentService;
    [Inject] private EquipmentManager equipmentManager;

    [Header("Button")]
    [SerializeField] private Button normalButton;       // 노말 일괄
    [SerializeField] private Button rareButton;         // 희귀 일괄
    [SerializeField] private Button epicButton;         // 특급 일괄
    [SerializeField] private Button exitButton;         // 나가기
    [SerializeField] private Button submitButton;       // 분해하기

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI equipCount;    // 장비 개수
    [SerializeField] private TextMeshProUGUI resultGrind;    // 획득 연마석

    public List<DecompositionButton> activeEquipButtons = new();    // 분해 선택 버튼

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
