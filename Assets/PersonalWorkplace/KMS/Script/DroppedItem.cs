using System;
using UnityEngine;
using DG.Tweening;

public class DroppedItem : MonoBehaviour, IPooled<DroppedItem>
{

    private Vector2 p0;
    private Vector2 p1;
    private Vector2 p2;
    private float lerp_t;
    private float timer;
    private SpriteRenderer sprite;

    private DroppedItemType type;
    private int quantity;

    private bool throwing;


    [SerializeField] DroppedItemDataSO datas;


    public Action<IPooled<DroppedItem>> OnLifeEnded { get; set; }

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    public void Init(DroppedItemType type, int quantity)
    {
        this.type = type;
        this.quantity = quantity;
        sprite.sprite = datas.sprites[type];
        PoolManager.Instance.AddItemToList(this);
    }
    public void Shot()
    {
        p0 = transform.position;
        p2 = p0 + GetRandomPos();
        p1 = Vector2.Lerp(p0, p2, 0.5f) + 1.5f * Vector2.up;
        lerp_t = 0;
        throwing = true;
    }
    public void Release(Vector2 pos)
    {
        throwing = false;
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(pos, 1));
        seq.OnComplete(() =>
        {
            AddCurrency();
            OnLifeEnded?.Invoke(this);
        });
    }

    void Update()
    {
        if (!throwing) return;
        if (lerp_t >= 1) return;
        transform.localPosition = CalPos();
        lerp_t += Time.deltaTime * 0.7f;
    }

    private Vector2 GetRandomPos()
    {
        float x = UnityEngine.Random.Range(-2.5f, 2.5f);
        return new Vector2(x, 0);
    }
    private Vector2 CalPos()
    {
        Vector2 first = Vector2.Lerp(p0, p1, lerp_t);
        Vector2 second = Vector2.Lerp(p1, p2, lerp_t);
        Vector2 result = Vector2.Lerp(first, second, lerp_t);
        return result;
    }
    private void AddCurrency()
    {
        if (type == DroppedItemType.Gold)
        {
            CurrencyManager.Instance.Add(CurrencyType.Gold, new BigCurrency(quantity));
        }
        else if (type == DroppedItemType.SpiritBack)
        {
            CurrencyManager.Instance.Add(CurrencyType.Soul, new BigCurrency(quantity));
        }
        else if (type == DroppedItemType.SoulStone)
        {
            CurrencyManager.Instance.Add(CurrencyType.SpiritStone, new BigCurrency(quantity));
        }
    }
}
