using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroListManager : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;
    
    #region Unity
    private void OnEnable()
    {
        if (!HeroDataManager.Instance.IsInitialized)
        {
            Debug.LogWarning("[HeroListManager] HeroDataManager 초기화가 완료되지 않았습니다.");
            return;
        }

        RefreshHeroList();
    }
    #endregion
    private void RefreshHeroList()
    {
        var ownedHeroes = HeroDataManager.Instance.ownedHeroes;

        foreach (Transform cardTransform in parentTransform)
        {
            var infoSetter = cardTransform.GetComponent<HeroInfoSetting>();
            if (infoSetter == null || string.IsNullOrEmpty(infoSetter.HeroID))
                continue;

            bool isOwned = ownedHeroes.ContainsKey(infoSetter.HeroID);
            cardTransform.gameObject.SetActive(isOwned);
        }

        Debug.Log($"[HeroListManager] 보유 영웅 {ownedHeroes.Count}명 기준으로 카드 활성화 완료");
    }
}