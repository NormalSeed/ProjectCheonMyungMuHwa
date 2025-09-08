using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class TestEquipmentUI : MonoBehaviour
{
    [SerializeField] private List<Button> equipmentButtons;

    [Inject]
    private EquipmentManager manager;

    private void OnEnable()
    {
        if (manager != null)
            StartCoroutine(WaitForManager());
        else
            Debug.LogWarning("EquipmentManager가 아직 주입되지 않았습니다.");
    }

    private IEnumerator WaitForManager()
    {
        while (!manager.IsInitialized)
            yield return null;

        InitializeAllEquipments(manager.allEquipments);
    }


    public void AssignInstanceToButton(EquipmentInstance instance)
    {
        int index = equipmentButtons.FindIndex(b => !b.gameObject.activeSelf);
        if (index == -1)
        {
            Debug.LogWarning("사용 가능한 버튼이 없습니다.");
            return;
        }

        var button = equipmentButtons[index];
        button.gameObject.SetActive(true);

        var itemUI = button.GetComponent<TestEquipmentInstanceUI>();
        if (itemUI == null)
        {
            Debug.LogWarning("EquipmentItemUI 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        itemUI.Bind(instance);
    }

    public void InitializeAllEquipments(List<EquipmentInstance> allEquipments)
    {
        foreach (var instance in allEquipments)
        {
            AssignInstanceToButton(instance);
        }
    }
}
