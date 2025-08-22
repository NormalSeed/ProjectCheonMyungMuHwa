using UnityEngine;

public class Coin : MonoBehaviour
{

    private Transform par;
    private Vector2 p0;

    private Vector2 p1;
    private Vector2 p2;

    private float lerp_t;

    void Awake()
    {
        par = transform.parent;
    }


    private void OnEnable()
    {
        transform.parent = null;
        p0 = transform.position;
        p2 = p0 + GetRandomPos();
        p1 = Vector2.Lerp(p0, p2, 0.5f) + 1.5f* Vector2.up;
        lerp_t = 0;
    }

    void Update()
    {
        if (lerp_t >= 1) return;
        transform.localPosition = CalPos();
        lerp_t += Time.deltaTime * 0.7f;

    }
    private void OnDisable() //문제 있음
    {
        transform.parent = par;
        transform.localPosition = Vector2.zero;
    }

    private Vector2 GetRandomPos()
    {
        float x = Random.Range(-2.5f, 2.5f);
        return new Vector2(x, 0);
    }
    private Vector2 CalPos()
    {
        Vector2 first = Vector2.Lerp(p0, p1, lerp_t);
        Vector2 second = Vector2.Lerp(p1, p2, lerp_t);
        Vector2 result = Vector2.Lerp(first, second, lerp_t);
        return result;
    }
}
