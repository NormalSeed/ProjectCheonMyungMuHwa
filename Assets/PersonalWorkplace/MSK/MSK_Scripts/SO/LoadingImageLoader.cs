using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class LoadingImageLoader : MonoBehaviour
{

    [SerializeField] private Image loadingImage;
    [SerializeField] private string addressableLabel = "face";

    private List<Sprite> loadedSprites = new List<Sprite>();

    public void ShowRandomLoadingImage()
    {
        Addressables.LoadAssetsAsync<Sprite>(addressableLabel, null).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedSprites = new List<Sprite>(handle.Result);

                if (loadedSprites.Count > 0)
                {
                    int index = Random.Range(0, loadedSprites.Count);
                    loadingImage.sprite = loadedSprites[index];
                    loadingImage.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.LogError("이미지 로딩 실패: Addressables에서 Sprite 불러오기 실패");
            }
        };
    }

    public void HideLoadingImage()
    {
        loadingImage.gameObject.SetActive(false);
    }

    public void ShowTapToStart()
    {
        HideLoadingImage();
        // Tap to Start UI 활성화
    }
}