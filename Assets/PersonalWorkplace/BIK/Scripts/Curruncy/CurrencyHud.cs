using TMPro;
using UnityEngine;
using VContainer;

public class CurrencyHud : MonoBehaviour
{
    [Header("상단 UI")]
    [SerializeField] private TMP_Text _goldText;

    [Header("상단 Open 시 UI")]
    [SerializeField] private TMP_Text _openGoldText;
    [SerializeField] private TMP_Text _jewelText;
    [SerializeField] private TMP_Text _soulText;
    [SerializeField] private TMP_Text _spiritStoneText;
    [SerializeField] private TMP_Text _summonTicketText;
    [SerializeField] private TMP_Text _invitationTicketText;
    [SerializeField] private TMP_Text _challengeTicketText;

    [Inject] private IGameCurrencyController _currency;  // 조작용
    [Inject] private ICurrencyModel _model;              // 조회/이벤트용

    private void Start()
    {
        // 최초 한 번 전체 표시
        RefreshAll();

        // 변경 이벤트 구독
        _model.OnChanged += OnCurrencyChanged;
    }

    private void OnDestroy()
    {
        _model.OnChanged -= OnCurrencyChanged;
    }

    private void OnCurrencyChanged(CurrencyId id, BigCurrency amount)
    {
        if (id == CurrencyIds.Gold) {
            _goldText.text = Format(amount);
            _openGoldText.text = Format(amount);
        }
        else if (id == CurrencyIds.Jewel) {
            _jewelText.text = Format(amount);
        }
        else if (id == CurrencyIds.Soul) {
            _soulText.text = Format(amount);
        }
        else if (id == CurrencyIds.SpiritStone) {
            _spiritStoneText.text = Format(amount);
        }
        else if (id == CurrencyIds.SummonTicket) {
            _summonTicketText.text = Format(amount);
        }
        else if (id == CurrencyIds.InvitationTicket) {
            _invitationTicketText.text = Format(amount);
        }
        else if (id == CurrencyIds.ChallengeTicket) {
            _challengeTicketText.text = Format(amount);
        }
        // 필요하면 다른 재화도 추가
    }

    private void RefreshAll()
    {
        _goldText.text = Format(_currency.Get(CurrencyIds.Gold));
        _openGoldText.text = Format(_currency.Get(CurrencyIds.Gold));
        _jewelText.text = Format(_currency.Get(CurrencyIds.Jewel));
        _soulText.text = Format(_currency.Get(CurrencyIds.Soul));
        _spiritStoneText.text = Format(_currency.Get(CurrencyIds.SpiritStone));
        _summonTicketText.text = Format(_currency.Get(CurrencyIds.SummonTicket));
        _invitationTicketText.text = Format(_currency.Get(CurrencyIds.InvitationTicket));
        _challengeTicketText.text = Format(_currency.Get(CurrencyIds.ChallengeTicket));
    }

    private static string Format(BigCurrency v)
    {
        return v.ToString();
    }

    // 버튼 테스트용
    public void OnClick_Currency(CurrencyType currencyID)
    {
        _currency.Add(aaCurrencyId(currencyID), new BigCurrency(100, 0));
    }
    public enum CurrencyType
    {
        Jewel,
        Gold,
        Soul,
        SpiritStone,
        SummonTicket,
        InvitationTicket,
        ChallengeTicket
    }

    private CurrencyId aaCurrencyId(CurrencyType type)
    {
        CurrencyId id = CurrencyIds.Gold; // 예시로 Gold 사용
        switch (type) {
            case CurrencyType.Jewel:
                id = CurrencyIds.Jewel;
                break;
            case CurrencyType.Gold:
                id = CurrencyIds.Gold;
                break;
            case CurrencyType.Soul:
                id = CurrencyIds.Soul;
                break;
            case CurrencyType.SpiritStone:
                id = CurrencyIds.SpiritStone;
                break;
            case CurrencyType.SummonTicket:
                id = CurrencyIds.SummonTicket;
                break;
            case CurrencyType.InvitationTicket:
                id = CurrencyIds.InvitationTicket;
                break;
            case CurrencyType.ChallengeTicket:
                id = CurrencyIds.ChallengeTicket;
                break;
        }

        return id;
    }
}
