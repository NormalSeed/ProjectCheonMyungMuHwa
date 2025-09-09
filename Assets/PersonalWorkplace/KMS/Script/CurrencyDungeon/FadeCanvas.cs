using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class FadeCanvas : MonoBehaviour
{
    [SerializeField] CurrencyDungeonSceneLoadDataSO data;

    [SerializeField] Image image;

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
        if (data.FadeIn)
        {
            image.color = new Color(0, 0, 0, 1);
            FadeIn();
        }
        else
        {
            image.color = new Color(0, 0, 0, 0);
        }

    }

    public void FadeOutAndLoadMainScene()
    {
        data.FadeIn = true;
        Sequence seq = DOTween.Sequence();
        seq.Append(image.DOFade(1f, 1.5f));
        seq.OnComplete(() =>
        {
            SceneManager.LoadSceneAsync("Demo_GameScene");
        });

    }

}
