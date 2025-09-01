using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroListManager : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;

    private DatabaseReference _dbRef;
    private string _uid;

    private void OnEnable()
    {
        _uid = CurrencyManager.Instance.UserID;
        _dbRef = CurrencyManager.Instance.DbRef;

        StartCoroutine(RefreshHeroList());
    }

    private IEnumerator RefreshHeroList()
    {
        var ownedIds = new List<string>();
        var charInfoRef = _dbRef.Child("users").Child(_uid).Child("character").Child("charInfo");

        var loadTask = charInfoRef.GetValueAsync();
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Exception != null)
        {
            Debug.LogError($"Failed to load hero info: {loadTask.Exception}");
            yield break;
        }

        DataSnapshot snapshot = loadTask.Result;
        foreach (var child in snapshot.Children)
        {
            var hasHero = child.Child("hasHero");
            if (hasHero.Exists && (bool)hasHero.Value)
                ownedIds.Add(child.Key);
        }

        foreach (Transform cardTransform in parentTransform)
        {
            var infoSetter = cardTransform.GetComponent<HeroInfoSetting>();
            if (infoSetter == null || infoSetter.chardata == null)
                continue;

            string heroId = infoSetter.chardata.HeroID;
            bool isOwned = ownedIds.Contains(heroId);
            cardTransform.gameObject.SetActive(isOwned);
        }
    }
}