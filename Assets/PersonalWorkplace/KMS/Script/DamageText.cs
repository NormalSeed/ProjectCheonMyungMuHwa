using TMPro;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class DamageText : MonoBehaviour, IPooled<DamageText>
{
    [SerializeField] float lifetime;

    [SerializeField] TMP_Text damageText;

    public Action<IPooled<DamageText>> OnLifeEnded { get; set; }

    private float timer;

    void Update()
    {
        if (timer >= lifetime)
        {
            OnLifeEnded?.Invoke(this);
        }
        timer += Time.deltaTime;
        transform.position += Vector3.up * Time.deltaTime;
    }
    public void SetText(string damage)
    {
        damageText.text = damage;
        damageText.color = new Color(1, 0.294f, 0.294f);
    }
    public void SetText(string damage, Color color)
    {
        damageText.text = damage;
        damageText.color = color;
    }

    void OnEnable()
    {
        timer = 0;
    }
}
