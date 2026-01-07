using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 다음에 나올 아이템(동글이)과 홀드된 아이템을 화면에 표시하는 UI 전용 매니저
/// 게임 로직과 분리되어 오직 UI 표시만 담당합니다
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Next UI 이미지 연결")]
    [SerializeField]
    private Image nextItemImage; // 다음에 나올 동글이 이미지 컴포넌트

    [Header("Hold UI 이미지 연결")]
    [SerializeField]
    private Image holdItemImage; // 보관함 동글이 이미지 컴포넌트

    [Header("아이템 스프라이트 리스트 (10개)")]
    [SerializeField]
    private Sprite[] itemSprites; // 레벨별 동글이 이미지들 (0~9)

    [Header("옵션")]
    [SerializeField]
    private bool preserveAspect = true; // 이미지 비율 유지 여부

    void Start()
    {
        // Next 이미지 비율 유지 설정
        if (nextItemImage != null)
        {
            nextItemImage.preserveAspect = preserveAspect;
        }

        // Hold 이미지 초기 상태 설정 (숨김 처리)
        if (holdItemImage != null)
        {
            holdItemImage.preserveAspect = preserveAspect;
            holdItemImage.enabled = false; // 처음엔 비활성화

            Color c = holdItemImage.color;
            c.a = 0; // 완전 투명
            holdItemImage.color = c;
        }
    }

    /// <summary>
    /// 다음 아이템의 이미지를 변경하는 함수
    /// GameManager에서 호출됩니다
    /// </summary>
    /// <param name="level">표시할 동글이의 레벨 (0~9)</param>
    public void SetNextItemDisplay(int level)
    {
        // 유효성 검사: 레벨이 배열 범위 내에 있는지 확인
        if (level < 0 || level >= itemSprites.Length)
        {
            Debug.LogWarning("유효하지 않은 레벨: " + level + ". 0~" + (itemSprites.Length - 1) + " 사이여야 합니다.");
            return;
        }

        // UI 이미지가 연결되어 있는지 확인
        if (nextItemImage == null)
        {
            Debug.LogError("Next Item Image가 연결되지 않았습니다!");
            return;
        }

        // 스프라이트가 등록되어 있는지 확인
        if (itemSprites[level] == null)
        {
            Debug.LogError("레벨 " + level + "의 스프라이트가 등록되지 않았습니다!");
            return;
        }

        // 이미지 교체 (핵심 로직)
        nextItemImage.sprite = itemSprites[level];
    }

    /// <summary>
    /// 홀드 아이템의 이미지를 변경하는 함수
    /// GameManager에서 호출됩니다
    /// </summary>
    /// <param name="level">표시할 동글이의 레벨 (-1이면 숨김)</param>
    public void SetHoldItemDisplay(int level)
    {
        if (holdItemImage == null) return;

        if (level == -1)
        {
            holdItemImage.enabled = false;
            return;
        }

        holdItemImage.enabled = true;
        holdItemImage.sprite = itemSprites[level];

        Color c = holdItemImage.color;
        c.a = 1f;
        holdItemImage.color = c;

        StopAllCoroutines();
        StartCoroutine(HoldPunchEffect());
    }

    IEnumerator HoldPunchEffect()
    {
        holdItemImage.transform.localScale = Vector3.one * 1.2f;
        while (holdItemImage.transform.localScale.x > 1.01f)
        {
            holdItemImage.transform.localScale = Vector3.Lerp(holdItemImage.transform.localScale, Vector3.one, Time.deltaTime * 10f);
            yield return null;
        }
        holdItemImage.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Next 이미지를 원본 크기로 되돌리는 함수 (선택사항)
    /// </summary>
    public void ResetToNativeSize()
    {
        if (nextItemImage != null)
        {
            nextItemImage.SetNativeSize();
        }
    }

    /// <summary>
    /// Next UI 이미지를 숨기거나 보이게 하는 함수
    /// </summary>
    public void SetVisibility(bool isVisible)
    {
        if (nextItemImage != null)
        {
            nextItemImage.enabled = isVisible;
        }
    }

    /// <summary>
    /// 현재 등록된 스프라이트 개수를 반환
    /// </summary>
    public int GetSpriteCount()
    {
        return itemSprites.Length;
    }
}