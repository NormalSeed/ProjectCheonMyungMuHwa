using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestPlayerSetter : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private PlayerController player;
    [SerializeField] private GachaManager gachaManager;
    [SerializeField] private List<CardInfo> cards;

    private void Start()
    {
        inputField.onEndEdit.AddListener(OnInputSubmitted);
    }

    private void OnInputSubmitted(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            Debug.Log("입력값이 비어있음");
            return;
        }

        if (player != null)
        {
            StartCoroutine(gachaManager.ProcessResultsCoroutine(cards));
            player.gameObject.SetActive(false);
            player.gameObject.SetActive(true);
            player.charID.Value = input;
            Debug.Log($"CharID 설정 완료됨 : {player.charID.Value}");
        }
        else
        {
            Debug.LogError("PlayerController 설정되지 않음");
        }
    }

}
