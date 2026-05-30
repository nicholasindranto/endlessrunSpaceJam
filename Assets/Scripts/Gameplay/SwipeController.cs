using System;
using UnityEngine;
using Lean.Touch;

public class SwipeController : MonoBehaviour
{
    [Header("Swipe Settings")]
    [SerializeField] private float minSwipeDistance = 50f; // minimum pixel biar tidak false positive

    // ─── Events ─────────────────────────────────────────────────────────────────
    public static event Action OnSwipeLeft;
    public static event Action OnSwipeRight;
    public static event Action OnSwipeUp;
    public static event Action OnSwipeDown;

    // ─── Private ────────────────────────────────────────────────────────────────
    // Input mode tracking untuk FTUE
    // private FTUEGameplaySequence.InputMode currentInputMode = FTUEGameplaySequence.InputMode.AllEnabled;

    private void OnEnable()
    {
        // FTUEGameplaySequence.OnInputModeChange += HandleInputModeChange;
        LeanTouch.OnFingerSwipe += HandleFingerSwipe;
    }

    private void OnDisable()
    {
        // FTUEGameplaySequence.OnInputModeChange -= HandleInputModeChange;
        LeanTouch.OnFingerSwipe -= HandleFingerSwipe;
    }

    // private void HandleInputModeChange(FTUEGameplaySequence.InputMode mode)
    // {
    //     currentInputMode = mode;
    // }

    // ─── LeanTouch Swipe Handler ────────────────────────────────────────────────
    private void HandleFingerSwipe(LeanFinger finger)
    {
        // Hanya process jika finger actually swiped
        if (!finger.Swipe) return;

        // Hitung delta dari StartScreenPosition ke ScreenPosition
        Vector2 delta = finger.ScreenPosition - finger.StartScreenPosition;

        // Cek apakah jarak swipe cukup jauh
        if (delta.magnitude < minSwipeDistance) return;

        // Tentukan arah dominan (horizontal vs vertikal)
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // Swipe horizontal
            if (delta.x > 0) OnSwipeRight?.Invoke();
            else             OnSwipeLeft?.Invoke();
        }
        else
        {
            // Swipe vertikal
            if (delta.y > 0) OnSwipeUp?.Invoke();
            else             OnSwipeDown?.Invoke();
        }
    }

    // ─── Swipe Logic ─────────────────────────────────────────────────────────────
    // private void InvokeIfAllowed(Action swipeEvent)
    // {
    //     // Kalau AllEnabled, selalu invoke
    //     if (currentInputMode == FTUEGameplaySequence.InputMode.AllEnabled)
    //     {
    //         swipeEvent?.Invoke();
    //         return;
    //     }

    //     // Kalau AllDisabled, jangan invoke apapun
    //     if (currentInputMode == FTUEGameplaySequence.InputMode.AllDisabled)
    //         return;

    //     // Kalau restricted mode, check sebelum invoke
    //     if (swipeEvent == OnSwipeRight && currentInputMode == FTUEGameplaySequence.InputMode.OnlyRight)
    //         swipeEvent?.Invoke();
    //     else if (swipeEvent == OnSwipeDown && currentInputMode == FTUEGameplaySequence.InputMode.OnlyDown)
    //         swipeEvent?.Invoke();
    //     else if (swipeEvent == OnSwipeUp && currentInputMode == FTUEGameplaySequence.InputMode.OnlyUp)
    //         swipeEvent?.Invoke();
    // }
}