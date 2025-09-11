using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeCanvas : MonoBehaviour
{
    [SerializeField] CurrencyDungeonSceneLoadDataSO data;

    [SerializeField] Image image;

    [SerializeField] MainSceneUIController mainSceneUI;
    CurrencyDungeonClearData clearData;

    void Start()
    {
        if (data.BackToMain)
        {
            FadeIn();
            StartCoroutine(TestRoutine());
        }
    }
    private IEnumerator TestRoutine()
    {
        yield return null;
        mainSceneUI.ShowUI(UIType.Dungeon);
    }

    public void FadeIn()
    {
        image.color = new Color(0, 0, 0, 1);
        image.DOFade(0f, 1.5f);
    }
    public void FadeOut()
    {
        image.color = new Color(0, 0, 0, 0);
        image.DOFade(1f, 1.5f);
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

}
