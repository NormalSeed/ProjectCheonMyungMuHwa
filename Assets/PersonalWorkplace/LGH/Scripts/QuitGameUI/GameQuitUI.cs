using UnityEngine;
using UnityEngine.UI;

public class GameQuitUI : MonoBehaviour
{
    [SerializeField] Button yesButton;
    [SerializeField] Button noButton;

    [SerializeField] private InGameManager manager;

    private void Start()
    {
        yesButton.onClick.AddListener(QuitGame);
        noButton.onClick.AddListener(ReturnToGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToGame();
        }
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    private void ReturnToGame()
    {
        manager.isQuitUIActive = false;
        this.gameObject.SetActive(false);
    }
}
