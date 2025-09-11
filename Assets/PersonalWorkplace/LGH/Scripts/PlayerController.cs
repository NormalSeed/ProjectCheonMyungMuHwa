using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;

public class PlayerController : MonoBehaviour, IDamagable
{
    [Inject]
    private EquipmentManager equipmentManager;

    public PlayerModel model;
    public PlayerView view;
    public GameObject skillSet;
    public GameObject SPUMAsset;
    public SPUM_Prefabs spumController;
    public CharacterEquipment equipment;

    public int partyNum;

    public ObservableProperty<string> charID { get; private set; } = new(string.Empty);

    public NavMeshAgent NMagent;
    private BehaviorGraphAgent BGagent;

    [Header("스킬 관련 필드")]
    public bool isSkillReady = true;
    public bool isSkill1Ready = true;
    public bool isSkill2Ready = false;
    
    public float curCool = 0f;
    public int skill2Count = 5;

    [Header("보호막 관련 필드")]
    private float shieldAmount = 0f;
    private float shieldDuration = 0f;
    private float shieldElapsed = 0f;
    public bool isShieldActive = false;

    public event System.Action OnModelLoaded;

    private void Awake()
    {
        //model = GetComponent<PlayerModel>();
        //view = GetComponent<PlayerView>();
        //equipment = GetComponent<CharacterEquipment>();

        //NMagent = GetComponent<NavMeshAgent>();
        //BGagent = GetComponent<BehaviorGraphAgent>();
        //BGagent.enabled = false;

        //NMagent.updateRotation = false;
        //NMagent.updateUpAxis = false;
    }

    private void OnEnable()
    {
        model = GetComponent<PlayerModel>();
        view = GetComponent<PlayerView>();
        equipment = GetComponent<CharacterEquipment>();

        NMagent = GetComponent<NavMeshAgent>();
        BGagent = GetComponent<BehaviorGraphAgent>();
        BGagent.enabled = false;

        NMagent.updateRotation = false;
        NMagent.updateUpAxis = false;

        charID.Subscribe(LoadPlayerData);

        // 플레이어 모델이 로드된 후 실행될 이벤트
        OnModelLoaded += () =>
        {
            if (BGagent != null)
            {
                BGagent.enabled = true;
            }

            equipment.SetCharID();

            PartyManager.Instance.CheckSynergy();
            // 훈련 데이터가 이미 로딩됐으면 바로 Modifier 적용
            if (TrainingManager.Instance.IsTrainingDataLoaded)
            {
                TrainingManager.Instance.ApplyTrainingModifiersToPlayer(this);
            }
            Debug.Log("모델 로드됨");
        };

        GameEvents.OnHeroLevelChanged += HandleHeroLevelChanged;
    }

    private void OnDisable()
    {
        charID.Unsubscribe(LoadPlayerData);
        OnModelLoaded = null;
        GameEvents.OnHeroLevelChanged -= HandleHeroLevelChanged;
    }

    /// <summary>
    /// charID를 기준으로 charID_model 형식의 주소를 가진 어드레서블 에셋을 불러와 플레이어 데이터를 로딩하는 메서드
    /// </summary>
    /// <param name="charID"></param>
    private void LoadPlayerData(string charID)
    {
        if (string.IsNullOrEmpty(charID))
        {
            Debug.LogWarning("charID가 null 또는 빈 문자열입니다. 캐릭터 오브젝트를 비활성화합니다.");
            gameObject.SetActive(false);
            return;
        }

        // Addressable 로딩 형식
        // csv 파일을 받아오는 것이 아닌 CharID와 같은 Address를 가진 Model SO를 받아와서 등록하게 함
        Addressables.LoadAssetAsync<PlayerModelSO>(charID + "_model")
            .Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    model.modelSO = handle.Result;

                    // Firebase에서 캐릭터 레벨 정보 불러오기
                    string userID = CurrencyManager.Instance.UserID;
                    Debug.Log("현재 유저 아이디 : " + userID);
                    string path = $"users/{userID}/character/charInfo/{charID}";

                    FirebaseDatabase.DefaultInstance
                        .GetReference(path)
                        .GetValueAsync()
                        .ContinueWithOnMainThread(task =>
                        {
                            if (task.IsFaulted)
                            {
                                Debug.LogError("Firebase 요청 실패: " + task.Exception);
                                return;
                            }

                            if (task.IsCanceled)
                            {
                                Debug.LogWarning("Firebase 요청이 취소됨");
                                return;
                            }

                            if (task.IsCompleted && task.Result.Exists)
                            {
                                DataSnapshot snapshot = task.Result;

                                int level = int.Parse(snapshot.Child("level").Value.ToString());
                                int stage = int.Parse(snapshot.Child("stage").Value.ToString());

                                // 모델에 적용
                                model.modelSO.Level = level;
                                model.modelSO.Grade = stage;

                                model.SetPoints(); // 능력치 계산
                                equipment.InitializeEquippedSlotsFromManager(equipmentManager.allEquipments);
                            }
                            else
                            {
                                Debug.LogWarning($"Firebase에서 '{charID}' 캐릭터 데이터를 찾을 수 없습니다.");
                            }
                        });

                    LoadPlayerSPUMAsset(charID, model.modelSO.SkillSetID);
                }
                else
                {
                    gameObject.SetActive(false);
                    Debug.LogError($"'{charID}' 모델 로드 실패: {handle.OperationException}");
                }
            };

        // Resources 폴더에서 PlayerModelSO 로드
        //var modelSO = Resources.Load<PlayerModelSO>($"LGH/PlayerModels/{charID}_model");
        //if (modelSO != null)
        //{
        //    model.modelSO = modelSO;
        //    model.SetPoints();

        //    LoadPlayerSPUMAsset(charID);
        //    LoadPlayerSkillData(model.modelSO.SkillSetID);
        //    OnModelLoaded?.Invoke();
        //}
        //else
        //{
        //    Debug.LogError($"'{charID}' 모델 로드 실패: Resources/LGH/PlayerModels/{charID}_model");
        //}
    }

    private void LoadPlayerSkillData(string skillSetID)
    {
        // 기존 skillSet 제거
        if (skillSet != null)
        {
            Destroy(skillSet);
            Addressables.ReleaseInstance(skillSet);
            skillSet = null;
        }

        Addressables.LoadAssetAsync<GameObject>(skillSetID)
        .Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject skillSetInstance = Instantiate(handle.Result, transform);
                skillSet = skillSetInstance;

                // 컴포넌트 초기화도 여기서
                var skillSetComponent = skillSet.GetComponent<SkillSet>();
                skillSetComponent.Init(this); // PlayerController를 넘겨주는 방식
                OnModelLoaded?.Invoke();
            }
            else
            {
                Debug.LogError($"SkillSet 로드 실패: {handle.OperationException}");
            }
        };

        //var prefab = Resources.Load<GameObject>($"LGH/SkillSets/{skillSetID}");
        //if (prefab != null)
        //{
        //    GameObject skillSetInstance = Instantiate(prefab, transform);
        //    skillSet = skillSetInstance;

        //    var skillSetComponent = skillSet.GetComponent<SkillSet>();
        //    skillSetComponent?.Init(this);
        //}
        //else
        //{
        //    Debug.LogError($"SkillSet 로드 실패: Resources/SkillSets/{skillSetID}");
        //}
    }

    private void LoadPlayerSPUMAsset(string charID, string skillSetID)
    {
        // 기존 SPUMAsset 제거
        if (SPUMAsset != null)
        {
            Destroy(SPUMAsset);
            Addressables.ReleaseInstance(SPUMAsset);
            SPUMAsset = null;
            spumController = null;
        }

        Addressables.LoadAssetAsync<GameObject>(charID + "_SPUM")
        .Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject SPUMInstance = Instantiate(handle.Result, transform);
                SPUMInstance.transform.localPosition = Vector3.zero;
                SPUMInstance.transform.localScale = Vector3.one;
                SPUMAsset = SPUMInstance;
                spumController = SPUMAsset.GetComponent<SPUM_Prefabs>();
                // 이걸 해줘야 애니메이션이 등록됨
                spumController.OverrideControllerInit();
                LoadPlayerSkillData(skillSetID);
            }
            else
            {
                Debug.LogError($"SPUM 로드 실패: {handle.OperationException}");
            }
        };

        //var prefab = Resources.Load<GameObject>($"LGH/SPUMAssets/{charID}_SPUM");
        //if (prefab != null)
        //{
        //    GameObject SPUMInstance = Instantiate(prefab, transform);
        //    SPUMInstance.transform.localPosition = Vector3.zero;
        //    SPUMInstance.transform.localScale = Vector3.one;
        //    SPUMAsset = SPUMInstance;
        //spumController = SPUMAsset.GetComponent<SPUM_Prefabs>();
        //// 이걸 해줘야 애니메이션이 등록됨
        //spumController.OverrideControllerInit();
        //}
        //else
        //{
        //    Debug.LogError($"SPUM 로드 실패: Resources/SPUMAssets/{charID}_SPUM");
        //}
    }

    private void Update()
    {
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

        // 보호막 지속시간 처리
        if (isShieldActive)
        {
            shieldElapsed += Time.deltaTime;
            if (shieldElapsed >= shieldDuration)
            {
                isShieldActive = false;
                shieldAmount = 0f;
                Debug.Log("보호막 지속시간 종료");
            }
        }
    }

    /// <summary>
    /// 보호막 버프가 들어왔을 때 보호막을 적용시키는 메서드
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="duration"></param>
    public void ApplyShield(float amount, float duration)
    {
        shieldAmount = amount;
        shieldDuration = duration;
        shieldElapsed = 0f;
        isShieldActive = true;

        Debug.Log($"보호막 적용됨: {amount} / {duration}s");
    }

    public void TakeDamage(double amount)
    {
        if (isShieldActive)
        {
            shieldAmount -= (float)amount;

            if (shieldAmount <= 0f)
            {
                float leftoverDamage = -shieldAmount;
                shieldAmount = 0f;
                isShieldActive = false;

                model.CurHealth.Value -= leftoverDamage;
                Debug.Log("보호막이 깨졌습니다. 남은 피해 적용됨.");
            }
            else
            {
                Debug.Log($"보호막으로 피해 흡수됨. 남은 보호막: {shieldAmount}");
            }
        }
        else
        {
            model.CurHealth.Value -= amount;
            Debug.Log($"현재 체력 : {model.CurHealth.Value}");
        }
    }

    private void HandleHeroLevelChanged(int newLevel)
    {
        model.modelSO.Level = newLevel;
        model.SetPoints();
    }
}
