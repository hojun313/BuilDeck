using UnityEngine;
using TMPro;
using System.Collections;

public class CardDisplay : MonoBehaviour
{
    public TextMeshPro rankText;
    public TextMeshPro suitText;

    private Vector3 _currentUnselectedScale;
    private Vector3 _currentBaseLocalPosition;
    private bool _wasSelected = false;
    private const float selectedScaleMultiplier = 1.2f;
    private const float selectedYOffset = 0.5f;

    void Awake()
    {
        _currentUnselectedScale = transform.localScale;
    }

    public void SetSelected(bool isSelected)
    {
        Debug.Log($"[CardDisplay] SetSelected: {gameObject.name}, isSelected: {isSelected}, CurrentScale: {transform.localScale}");
        if (isSelected)
        {
            if (!_wasSelected)
            {
                _currentUnselectedScale = transform.localScale;
                _currentBaseLocalPosition = transform.localPosition;
                _wasSelected = true;
            }
            transform.localScale = _currentUnselectedScale * selectedScaleMultiplier;
            // transform.localPosition = _currentBaseLocalPosition + new Vector3(0, selectedYOffset, 0); // Y축 이동 제거
        }
        else
        {
            if (_wasSelected)
            {
                transform.localScale = _currentUnselectedScale;
                transform.localPosition = _currentBaseLocalPosition;
            }
        }
    }

    public void SetCard(Card card)
    {
        if (card == null)
        {
            rankText.text = "";
            suitText.text = "";
            return;
        }

        rankText.text = GetRankString(card.cardRank.Value);
        suitText.text = GetSuitString(card.cardSuit.Value);

        if (card.cardSuit.Value == Card.Suit.Heart || card.cardSuit.Value == Card.Suit.Diamond)
        {
            rankText.color = Color.red;
            suitText.color = Color.red;
        }
        else
        {
            rankText.color = Color.black;
            suitText.color = Color.black;
        }
    }

    private string GetRankString(Card.Rank rank)
    {
        switch (rank)
        {
            case Card.Rank.Ace: return "A";
            case Card.Rank.Jack: return "J";
            case Card.Rank.Queen: return "Q";
            case Card.Rank.King: return "K";
            default: return ((int)rank).ToString();
        }
    }

    private string GetSuitString(Card.Suit suit)
    {
        switch (suit)
        {
            case Card.Suit.Spade: return "♠";
            case Card.Suit.Heart: return "♥";
            case Card.Suit.Diamond: return "♦";
            case Card.Suit.Club: return "♣";
            default: return "?";
        }
    }

    public void ResetSelectionState()
    {
        // 카드가 선택된 적이 없던 상태로 되돌려, 다음 선택 시 위치를 새로 저장하도록 합니다.
        _wasSelected = false;
    }

    void OnMouseDown()
    {
        if (FindObjectOfType<GameManager>() == null || !Unity.Netcode.NetworkManager.Singleton.IsClient) return;

        Unity.Netcode.NetworkObject playerObject = Unity.Netcode.NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObject != null)
        {
            Player localPlayer = playerObject.GetComponent<Player>();
            if (localPlayer != null)
            {
                localPlayer.RequestSelectCard(GetComponent<Card>());
                StartCoroutine(PlayCardAnimation()); // 카드 선택 시 애니메이션 시작
            }
        }
    }

    private IEnumerator PlayCardAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Quaternion originalRotation = transform.localRotation;

        // 스케일 업
        float timer = 0f;
        float duration = 0.1f; // 애니메이션 지속 시간
        Vector3 targetScale = originalScale * 1.1f; // 10% 커지게
        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;

        // 스케일 다운 및 약간의 회전
        timer = 0f;
        duration = 0.1f; // 애니메이션 지속 시간
        Quaternion targetRotation = originalRotation * Quaternion.Euler(0, 0, 5); // 5도 회전
        while (timer < duration)
        {
            // transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / duration); // 이 줄을 제거
            transform.localRotation = Quaternion.Lerp(originalRotation, targetRotation, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        // transform.localScale = originalScale; // 이 줄을 제거
        transform.localRotation = originalRotation;
    }
}