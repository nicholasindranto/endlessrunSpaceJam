using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private Vector2 startPosition;
    private bool    isPressing;

    // ─── Input Callbacks ─────────────────────────────────────────────────────────
    // Hubungkan ke action "Press" (Button) di InputActions
    public void OnPress(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // Jari / kursor mulai menyentuh
            startPosition = GetCurrentPosition();
            isPressing    = true;
        }
        else if (context.canceled && isPressing)
        {
            // Jari / kursor diangkat → hitung arah swipe
            Vector2 endPosition = GetCurrentPosition();
            DetectSwipe(endPosition);
            isPressing = false;
        }
    }

    // Hubungkan ke action "Position" (Value / Vector2) di InputActions
    // Diperlukan supaya GetCurrentPosition() bisa baca posisi sekarang
    private Vector2 currentPosition;
    public void OnPosition(InputAction.CallbackContext context)
    {
        currentPosition = context.ReadValue<Vector2>();
    }

    // ─── Swipe Logic ─────────────────────────────────────────────────────────────
    private void DetectSwipe(Vector2 endPosition)
    {
        Vector2 delta = endPosition - startPosition;

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

    private Vector2 GetCurrentPosition() => currentPosition;
}