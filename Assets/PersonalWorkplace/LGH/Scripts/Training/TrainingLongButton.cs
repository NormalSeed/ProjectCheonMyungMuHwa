using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingLongButton : MonoBehaviour
{
    [SerializeField] private TrainingUI trainingUI;

    [SerializeField] private Button oneButton;
    [SerializeField] private Button fiveButton;
    [SerializeField] private Button twentyButton;

    [SerializeField] Image longButtonImage;
    [SerializeField] private List<Sprite> buttonSprites;

    private void OnEnable()
    {
        oneButton.onClick.AddListener (OnOneButtonClick);
        fiveButton.onClick.AddListener (OnFiveButtonClick);
        twentyButton.onClick.AddListener (OnTwentyButtonClick);
    }

    private void OnOneButtonClick()
    {
        trainingUI.trainingCount = 1;
        longButtonImage.sprite = buttonSprites[0];
        trainingUI.UpdateUI();
    }

    private void OnFiveButtonClick()
    {
        trainingUI.trainingCount = 5;
        longButtonImage.sprite = buttonSprites[1];
        trainingUI.UpdateUI();
    }

    private void OnTwentyButtonClick()
    {
        trainingUI.trainingCount = 20;
        longButtonImage.sprite = buttonSprites[2];
        trainingUI.UpdateUI();
    }
}
