using UnityEngine;

public enum CurrencyDungeonType { Gold, Honbaeg, Spirit }

public class CurrencyDungeonUI : UIBase
{
    [SerializeField] DungeonSelectPanel dungeonPanel;
    [SerializeField] LevelSelectPanel levelPanel;


    public override void SetShow()
    {
        gameObject.SetActive(true);
        OpenDungeonPanel();
    }

    public void OpenDungeonPanel()
    {
        levelPanel.gameObject.SetActive(false);
        dungeonPanel.gameObject.SetActive(true);
        dungeonPanel.RegisteButtons(OpenLevelPanel);

    }
    private void OpenLevelPanel(CurrencyDungeonType type)
    {
        levelPanel.gameObject.SetActive(true);
        dungeonPanel.gameObject.SetActive(false);
        levelPanel.Setting(type);
    }

}
