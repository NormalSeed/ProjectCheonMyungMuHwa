using UnityEngine;

public class CurrencyDungeonPlayerSet : MonoBehaviour
{
    [SerializeField] CurrencyDungeonPoint[] allignPoints;
    [SerializeField] CurrencyDungeonSceneLoadDataSO sceneData;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ActiveAllightPoint();
        }
    }
    private void ActiveAllightPoint()
    {
        CurrencyDungeonType type = sceneData.type;
        foreach (CurrencyDungeonPoint point in allignPoints)
        {
            if (type == point.Type)
            {
                point.gameObject.SetActive(true);
            }
        }
    }
}
