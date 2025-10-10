using UnityEngine;
using System.Collections;

public class MainSceneUIAutoOpener : MonoBehaviour
{
    [SerializeField] CurrencyDungeonSceneLoadDataSO data;

    [SerializeField] MainSceneUIController mainUI;

    void Start()
    {
        StartCoroutine(TestRoutine());
    }
    private IEnumerator TestRoutine()
    {
        yield return null;
        mainUI.ShowUI(data.MainUiToOpen);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            AudioManager.Instance.PlayBGM("BGM_Chapter2");
        }
    }
}