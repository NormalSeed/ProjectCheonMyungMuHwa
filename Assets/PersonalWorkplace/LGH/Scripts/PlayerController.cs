using Unity.Behavior;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerController : MonoBehaviour, IDamagable
{
    public PlayerModel model;
    public PlayerView view;
    public GameObject skillSet;
    public GameObject SPUMAsset;

    public ObservableProperty<string> charID { get; private set; } = new(string.Empty);

    private NavMeshAgent NMagent;
    private BehaviorGraphAgent BGagent;

    public bool isSkillReady = true;
    public bool isSkill1Ready = true;
    public bool isSkill2Ready = false;
    
    public float curCool = 0f;
    public int skill2Count = 5;

    private void Start()
    {
        model = GetComponent<PlayerModel>();
        view = GetComponent<PlayerView>();

        NMagent = GetComponent<NavMeshAgent>();
        BGagent = GetComponent<BehaviorGraphAgent>();

        NMagent.updateRotation = false;
        NMagent.updateUpAxis = false;

        charID.Subscribe(LoadPlayerData);
        charID.Value = "A01";
    }

    private void LoadPlayerData(string charID)
    {
        // csv 파일을 받아오는 것이 아닌 CharID와 같은 Address를 가진 Model SO를 받아와서 등록하게 함
        Addressables.LoadAssetAsync<PlayerModelSO>(charID + "_model")
            .Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    model.modelSO = handle.Result;
                    model.SetPoints();

                    LoadPlayerSPUMAsset(charID);
                    LoadPlayerSkillData(model.modelSO.SkillSetID);
                }
                else
                {
                    Debug.LogError($"'{charID}' 모델 로드 실패: {handle.OperationException}");
                }
            };
    }

    private void LoadPlayerSkillData(string skillSetID)
    {
        Addressables.LoadAssetAsync<GameObject>(skillSetID)
        .Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject skillSetInstance = Instantiate(handle.Result, transform);
                skillSet = skillSetInstance;

                // 컴포넌트 초기화도 여기서
                var skillSetComponent = skillSet.GetComponent<SkillSet>();
                skillSetComponent.Init(this); // 예시: PlayerController를 넘겨주는 방식
            }
            else
            {
                Debug.LogError($"SkillSet 로드 실패: {handle.OperationException}");
            }
        };
    }

    private void LoadPlayerSPUMAsset(string charID)
    {
        Addressables.LoadAssetAsync<GameObject>(charID + "_SPUM")
        .Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject SPUMInstance = Instantiate(handle.Result, transform);
                SPUMInstance.transform.localPosition = Vector3.zero;
                SPUMInstance.transform.localScale = Vector3.one;
                SPUMAsset = SPUMInstance;
            }
            else
            {
                Debug.LogError($"SPUM 로드 실패: {handle.OperationException}");
            }
        };
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
            isSkill1Ready = false;
            curCool -= Time.deltaTime;
        }
        else
        {
            isSkillReady = true;
            isSkill1Ready = true;
        }

        if (skill2Count > 0)
        {
            isSkill2Ready = false;
        }
        else
        {
            isSkillReady = true;
            isSkill2Ready = true;
        }
    }

    public void TakeDamage(double amount)
    {
        model.CurHealth.Value -= amount;
        Debug.Log($"현재 체력 : {model.CurHealth.Value}");
    }
}
