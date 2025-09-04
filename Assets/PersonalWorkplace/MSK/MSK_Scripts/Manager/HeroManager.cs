using Firebase.Database;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    public static HeroManager Instance { get; private set; }

    public Dictionary<string, HeroData> ownedHeroes = new Dictionary<string, HeroData>();

    private string _uid;
    private DatabaseReference _dbRef;

    public void Start()
    {
        _uid = CurrencyManager.Instance.UserID;
        _dbRef = CurrencyManager.Instance.DbRef;
    }

    private void LoadAllHeroData()
    {
        if (string.IsNullOrEmpty(_uid))
            return;

        var heroInfoPath = _dbRef.Child("users").Child(_uid).Child("character").Child("charInfo");

        heroInfoPath.GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("Firebase 데이터 가져오기 실패");
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                ownedHeroes.Clear(); // 기존 데이터 초기화

                foreach (var child in snapshot.Children)
                {
                    string heroId = child.Key;
                    string json = child.GetRawJsonValue();

                    HeroData hero = JsonUtility.FromJson<HeroData>(json);
                    ownedHeroes[heroId] = hero;
                }
                Debug.Log($"총 {ownedHeroes.Count}명의 영웅 정보를 불러왔습니다.");
            }
        });
    }
}