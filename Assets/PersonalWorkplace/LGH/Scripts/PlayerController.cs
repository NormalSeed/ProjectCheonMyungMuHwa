using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour, IDamagable
{
    //private PlayerModel model;
    private PlayerView view;

    private NavMeshAgent NMagent;
    private BehaviorGraphAgent BGagent;

    public bool isSkillReady = true;
    // 스킬셋에 들어가야 할 쿨다운
    public float skillCooldown = 3f;
    public float curCool = 0f;

    private void Start()
    {
        //model = GetComponent<PlayerModel>();
        view = GetComponent<PlayerView>();

        NMagent = GetComponent<NavMeshAgent>();
        BGagent = GetComponent<BehaviorGraphAgent>();

        NMagent.updateRotation = false;
        NMagent.updateUpAxis = false;
    }

    private void LoadPlayerData()
    {
        // 테스트를 위한 TestPlayerData.csv 파일 받아오기, TestPlayerData는 Addressable로 체크되어있어야 함
        Utils.LoadCSV("TestPlayerData", csvText =>
        {
            if (csvText != null)
            {
                // ID 기준으로 플레이어 데이터 받아오기
            }
        });

    }

    private void Update()
    {
        // Behavior Graph용 스킬 사용 가능 온오프 테스트코드
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isSkillReady = !isSkillReady;
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
        //model.CurHealth.Value -= amount;
        //Debug.Log($"현재 체력 : {model.CurHealth.Value}");
    }
}
