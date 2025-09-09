using TMPro;
using UnityEngine;

public class TestPlayerSetter : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private PlayerController player;

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
            player.charID.Value = input;
            Debug.Log($"CharID 설정 완료됨 : {player.charID.Value}");
        }
        else
        {
            Debug.LogError("PlayerController 설정되지 않음");
        }
    }

}
