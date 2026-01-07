using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

[System.Serializable]
public class ColorFadeAnimation
{
    public Graphic[] TargetGraphics;
    public Color ColorBefore;
    public Color ColorAfter;
    public float FadeDuration;
    public Ease FadeEase = Ease.Linear;
}

[RequireComponent(typeof(Button), typeof(RectTransform))]
public class InteractableButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("트윈 사용자 설정")]
    [SerializeField] protected Vector3 unscaledVector = Vector3.one;
    [SerializeField] protected Vector3 scaledVector = Vector3.one * 1.12f;
    [SerializeField] protected float tweenDuration = 0.14f;
    [SerializeField] protected Ease tweenEase = Ease.OutCirc;
    [Space(20)]

    [Header("그래픽 색상 변경")]
    [SerializeField] protected List<ColorFadeAnimation> colorFadeAnimations = new();
    [Space(20)]

    public RectTransform rect;
    public Button button;
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] GameObject disabledDisplay;

    protected virtual void Awake()
    {
        rect = GetComponent<RectTransform>();
        button = GetComponent<Button>();
    }
    public void Toggle(bool value, string reason = null)
    {
        button.interactable = value;
        disabledDisplay.SetActive(!value);

        if (value && description != null && reason != null) description.text = reason;
    }
    public virtual void OnPointerEnter(PointerEventData data)
    {
        rect.DOScale(scaledVector, tweenDuration).SetEase(tweenEase);

        foreach (var c in colorFadeAnimations)
        {
            foreach (var t in c.TargetGraphics)
            {
                t.DOColor(c.ColorAfter, c.FadeDuration).SetEase(c.FadeEase);
            }
        }
    }
    public virtual void OnPointerExit(PointerEventData data)
    {
        rect.DOScale(unscaledVector, tweenDuration).SetEase(tweenEase);

        foreach (var c in colorFadeAnimations)
        {
            foreach (var t in c.TargetGraphics)
            {
                t.DOColor(c.ColorBefore, c.FadeDuration).SetEase(c.FadeEase);
            }
        }
    }
}