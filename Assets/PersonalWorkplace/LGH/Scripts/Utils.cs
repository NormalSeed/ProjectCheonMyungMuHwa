using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class Utils
{
    /// <summary>
    /// CSV 파일을 비동기적으로 로드하는 메서드, onLoaded가 콜백함수이므로 =>로 함수 정의 필요
    /// </summary>
    /// <param name="csvName"></param>
    /// <param name="onLoaded"></param>
    public static void LoadCSV(string csvName, System.Action<string> onLoaded)
    {
        Addressables.LoadAssetAsync<TextAsset>(csvName).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                TextAsset csvFile = handle.Result;
                onLoaded?.Invoke(csvFile.text);
            }
            else
            {
                Debug.LogError($"CSV 파일 로딩 실패: {csvName}");
                onLoaded?.Invoke(null);
            }
        };
    }
}
