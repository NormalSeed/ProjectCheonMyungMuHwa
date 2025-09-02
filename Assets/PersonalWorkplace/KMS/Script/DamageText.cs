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
    }

    void OnEnable()
    {
        timer = 0;
    }
}
