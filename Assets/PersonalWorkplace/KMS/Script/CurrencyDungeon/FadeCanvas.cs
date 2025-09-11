using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class FadeCanvas : MonoBehaviour
{
    [SerializeField] CurrencyDungeonSceneLoadDataSO data;

    [SerializeField] Image image;

    [SerializeField] CurrencyDungeonUI currencyDungeonUI;

    private void FadeIn()
    {
        image.DOFade(0f, 1.5f);
    }
    private void FadeOut()
    {
        image.DOFade(1f, 1.5f);
    }


    void Awake()
    {
        if (data.BackToMain)
        {
            image.color = new Color(0, 0, 0, 1);
            FadeIn();
            Show();
            data.BackToMain = false;
        }
        else
        {
            image.color = new Color(0, 0, 0, 0);
        }

    }

    public void FadeOutAndLoadMainScene()
    {
        AudioManager.Instance.StopAllSounds();
        data.BackToMain = true;
        Sequence seq = DOTween.Sequence();
        seq.Append(image.DOFade(1f, 1.5f));
        seq.OnComplete(() =>
        {
            SceneManager.LoadSceneAsync("Demo_GameScene");
        });

    }

    private async void Show()
    {
        await currencyDungeonUI.LoadFromFirebase();
        currencyDungeonUI.OpenLevelPanel(data.type);
    }

}
