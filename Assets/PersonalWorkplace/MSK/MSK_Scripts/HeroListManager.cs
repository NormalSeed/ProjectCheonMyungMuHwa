using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroListManager : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;

    private DatabaseReference _dbRef;
    private string _uid;

    #region Unity
    private void OnEnable()
    {
        _uid = CurrencyManager.Instance.UserID;
        _dbRef = CurrencyManager.Instance.DbRef;

        StartCoroutine(RefreshHeroList());
    }
    #endregion

    private IEnumerator RefreshHeroList()
    {
        var ownedIds = new HashSet<string>();
        var charInfoRef = _dbRef.Child("users").Child(_uid).Child("character").Child("charInfo");

        var loadTask = charInfoRef.GetValueAsync();
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Exception != null)
        {
            Debug.LogError($"[HeroListManager] Firebase 로딩 실패: {loadTask.Exception}");
            yield break;
        }

        DataSnapshot snapshot = loadTask.Result;
        foreach (var child in snapshot.Children)
        {
            var hasHero = child.Child("hasHero");
            if (hasHero.Exists && hasHero.Value is bool owned && owned)
                ownedIds.Add(child.Key);
        }

        foreach (Transform cardTransform in parentTransform)
        {
            var infoSetter = cardTransform.GetComponent<HeroInfoSetting>();
            if (infoSetter == null || string.IsNullOrEmpty(infoSetter.HeroID))
                continue;

            bool isOwned = ownedIds.Contains(infoSetter.HeroID);
            cardTransform.gameObject.SetActive(isOwned);
        }

        Debug.Log($"[HeroListManager] 보유 영웅 {ownedIds.Count}명 기준으로 카드 활성화 완료");
    }
}