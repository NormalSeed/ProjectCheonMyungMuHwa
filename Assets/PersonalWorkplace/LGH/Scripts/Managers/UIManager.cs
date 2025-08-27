using UnityEngine;

public interface IUIManager
{
    public void SetActiveUI(GameObject uiObj);
}

public class UIManager : IUIManager
{
    public void SetActiveUI(GameObject uiObj)
    {
        uiObj.SetActive(true);
    }
}
