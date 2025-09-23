using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;

public class FadeCanvas : MonoBehaviour
{

    private static bool isInstantiated;

    [SerializeField] Image image;

    void Awake()
    {
        if (isInstantiated)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            isInstantiated = true;
        }
    }

    public void FadeIn(float time)
    {
        image.color = new Color(0, 0, 0, 1);
        image.DOFade(0f, time);
    }
    public void FadeOut(float time)
    {
        image.color = new Color(0, 0, 0, 0);
        image.DOFade(1f, time);
    }

    public void FadeOutAndLoadScene(string scene, float time)
    {
        AudioManager.Instance.StopAllSounds();
        Sequence seq = DOTween.Sequence();
        seq.Append(image.DOFade(1f, time));
        seq.OnComplete(() =>
        {
            LoadSceneAndFadeInAsync(scene, time);
        });
    }
    private async void LoadSceneAndFadeInAsync(string scene, float time)
    {
        GameEvents.ReturnAllPOs();
        await SceneManager.LoadSceneAsync(scene);
        FadeIn(time);
    }
}
