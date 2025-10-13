using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private GameObject tapToStartText;

    public string Text {get => progressText.text; set => progressText.text = value; }

    private CanvasGroup tapToStartCanvasGroup;
    private bool isBlinking = false;

    private void Awake()
    {
        if (tapToStartText != null)
        {
            tapToStartCanvasGroup = tapToStartText.GetComponent<CanvasGroup>();
            if (tapToStartCanvasGroup == null)
                tapToStartCanvasGroup = tapToStartText.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        tapToStartText.SetActive(false); // 처음엔 안 보이게
    }

    public void UpdateProgress(float progress)
    {
        progressBar.value = progress;
        progressText.text = Mathf.RoundToInt(progress * 100) + "%";
    }

    public void ShowTapToStart()
    {
        progressBar.gameObject.SetActive(false);
        progressText.gameObject.SetActive(false);
        tapToStartText.SetActive(true);

        if (!isBlinking && tapToStartCanvasGroup != null)
            StartCoroutine(BlinkTapToStart());
    }

    private System.Collections.IEnumerator BlinkTapToStart()
    {
        isBlinking = true;
        while (true)
        {
            // 알파값을 0~1로 반복
            float t = Mathf.PingPong(Time.time, 1f);
            tapToStartCanvasGroup.alpha = Mathf.Lerp(0.3f, 1f, t);
            yield return null;
        }
    }
}