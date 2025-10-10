using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class CharImage : MonoBehaviour
{
    // bugfix : 버그때문에 인식을 못하는 스크립트라 추가한 한 줄

    [SerializeField] private Image _characterImage;
    [SerializeField] private GameObject _lockOverlay;

    public Sprite CurrentSprite { get; private set; }

    private AsyncOperationHandle<Sprite>? _currentHandle;

    public void SetCharacter(PlayerModelSO model)
    {
        if (model == null || string.IsNullOrEmpty(model.SpriteKey)) {
            Debug.LogWarning("잘못된 캐릭터 데이터");
            return;
        }

        // 기존 핸들 해제
        if (_currentHandle.HasValue) {
            Addressables.Release(_currentHandle.Value);
            _currentHandle = null;
        }

        // 어드레서블로 로드
        var handle = Addressables.LoadAssetAsync<Sprite>(model.SpriteKey);
        _currentHandle = handle;

        handle.Completed += op => {
            if (op.Status == AsyncOperationStatus.Succeeded) {
                CurrentSprite = op.Result;
                _characterImage.sprite = CurrentSprite;
            }
            else {
                Debug.LogError($"Failed to load sprite: {model.SpriteKey}");
            }
        };
    }

    public void SetLocked(bool isLocked)
    {
        if (_lockOverlay != null)
            _lockOverlay.SetActive(isLocked);

        _characterImage.color = isLocked ? new Color(1, 1, 1, 0.5f) : Color.white;
    }

    private void OnDestroy()
    {
        if (_currentHandle.HasValue) {
            Addressables.Release(_currentHandle.Value);
        }
    }
}
