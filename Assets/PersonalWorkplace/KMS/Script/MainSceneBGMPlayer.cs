using UnityEngine;

public class MainSceneBGMPlayer
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{
    //    InGameManager.Instance.OnStageStart += SetBGM;
    //    InGameManager.Instance.OnStageInit += SetInitialBGM;
    //}

    private int currentstage;

    public void SetInitialBGM(int stage)
    {
        Debug.Log($"<color=yellow>{stage}");
        int door = stage % 100;

        if (door == 0)
        {
            AudioManager.Instance.PlayBGM("BGM_Chapter4");

        }
        else if (door <= 25)
        {
            AudioManager.Instance.PlayBGM("BGM_Chapter1");
        }
        else if (door <= 50)
        {
            Debug.Log($"<color=yellow>{door}");
            AudioManager.Instance.PlayBGM("BGM_Chapter2");
        }
        else if (door <= 75)
        {
            AudioManager.Instance.PlayBGM("BGM_Chapter3");
        }
        else
        {
            AudioManager.Instance.PlayBGM("BGM_Chapter4");
        }
        currentstage = stage;
    }

    public void SetBGM(int stage)
    {
        int door = stage % 100;
        switch (door)
        {
            case 1: AudioManager.Instance.PlayBGM("BGM_Chapter1"); break;
            case 26: AudioManager.Instance.PlayBGM("BGM_Chapter2"); break;
            case 51: AudioManager.Instance.PlayBGM("BGM_Chapter3"); break;
            case 76: AudioManager.Instance.PlayBGM("BGM_Chapter4"); break;
        }
    }
    //void OnDestroy()
    //{
    //    InGameManager.Instance.OnStageStart -= SetBGM;
    //    InGameManager.Instance.OnStageInit -= SetInitialBGM;
    //}
}
