using Unity.Behavior;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerController : MonoBehaviour, IDamagable
{
    private PlayerModel model;
    private PlayerView view;
    private SkillSet skillSet;

    public ObservableProperty<string> charID { get; private set; } = new(string.Empty);

    private NavMeshAgent NMagent;
    private BehaviorGraphAgent BGagent;

    public bool isSkillReady = true;
    // 스킬셋에 들어가야 할 쿨다운
    public float skillCooldown = 3f;
    public float curCool = 0f;

    private void Start()
    {
        model = GetComponent<PlayerModel>();
        view = GetComponent<PlayerView>();

        NMagent = GetComponent<NavMeshAgent>();
        BGagent = GetComponent<BehaviorGraphAgent>();

        NMagent.updateRotation = false;
        NMagent.updateUpAxis = false;

        charID.Subscribe(LoadPlayerData);
    }

    private void LoadPlayerData(string charID)
    {
        // csv 파일을 받아오는 것이 아닌 CharID와 같은 Address를 가진 Model SO를 받아와서 등록하게 함
        Addressables.LoadAssetAsync<PlayerModelSO>(charID)
            .Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    model.modelSO = handle.Result;
                    model.SetPoints();
                }
                else
                {
                    Debug.LogError($"'{charID}' 모델 로드 실패: {handle.OperationException}");
                }
            };
    }

    private void LoadPlayerSkillData()
    {

    }

    private void Update()
    {
        // 플레이어 데이터 교체 테스트를 위한 코드
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            charID.Value = "A01";
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            charID.Value = "A02";
        }

        if (curCool > 0)
        {
            isSkillReady = false;
            curCool -= Time.deltaTime;
        }
        else
        {
            isSkillReady = true;
        }
    }

    public void TakeDamage(double amount)
    {
        model.CurHealth.Value -= amount;
        Debug.Log($"현재 체력 : {model.CurHealth.Value}");
    }
}
