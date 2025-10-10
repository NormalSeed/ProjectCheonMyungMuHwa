using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopUISelectButton : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Button btn;
    private Color ButtonColor { get => image.color; set => image.color = value; }
    public UnityEvent OnClick => btn.onClick;

    [SerializeField] Color activeColor;
    private Color inactiveColor;

    void Awake()
    {
        inactiveColor = ButtonColor;
    }

    public void ActiveButton()
    {
        ButtonColor = activeColor;
    }

    public void InactiveButton()
    {
        ButtonColor = inactiveColor;
    }
}
