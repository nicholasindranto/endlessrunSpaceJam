using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSprint : MonoBehaviour
{
    [Header("Sprint Settings")]
    public int   sprintSpeed          = 15;   // player speed saat sprint
    public float sprintDuration       = 2f;   // durasi sprint
    public float monsterRetreatPerSec = 5f;   // monster mundur 5/detik × 2 detik = 10 total
    public float staminaDrainOnSprint    = 20f; // drain langsung saat sprint dimulai

    // ─── Static flag — dicek TunnelLoop & AutoMoveObj ───────────────────────────
    public static bool IsSprinting { get; private set; }

    // ─── Events ─────────────────────────────────────────────────────────────────
    public static event Action<int, float> OnSprintStart; // (sprintSpeed, retreatPerSec)
    public static event Action             OnSprintEnd;

    // uinya
    public TextMeshProUGUI sprintUI;
    public int showUIDuration;
    public float fadeDuration;

    // audionya
    [SerializeField] private AudioClip sprintSFX;
    [SerializeField] private AudioSource source;

    private void OnEnable() {
        SwipeController.OnSwipeUp += HandleSwipeUp;
    }

    private void OnDisable() {
        SwipeController.OnSwipeUp -= HandleSwipeUp;
    }

    private void HandleSwipeUp()
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) return;

        if (IsSprinting) return;

        // play sfx nya
        source.PlayOneShot(sprintSFX);

        // Hanya bisa sprint saat stamina Normal (> 50% / hijau)
        if (PlayerStamina.CurrentTier != PlayerStamina.StaminaTier.Normal)
        {
            Debug.Log("Stamina tidak cukup untuk sprint!");
            SetTextAlpha(0f);
            StartCoroutine(ShowUICoroutine());
            return;
        }

        StartCoroutine(SprintCoroutine());
    }

    // ─── Input (hubungkan ke W / Up Arrow di InputActions) ──────────────────────
    public void Sprint(InputAction.CallbackContext context)
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) return;

        if (!context.started) return;
        if (IsSprinting) return;

        // play sfx nya
        source.PlayOneShot(sprintSFX);

        // Hanya bisa sprint saat stamina Normal (> 50% / hijau)
        if (PlayerStamina.CurrentTier != PlayerStamina.StaminaTier.Normal)
        {
            Debug.Log("Stamina tidak cukup untuk sprint!");
            SetTextAlpha(0f);
            StartCoroutine(ShowUICoroutine());
            return;
        }

        StartCoroutine(SprintCoroutine());
    }

    private IEnumerator ShowUICoroutine()
    {
        yield return StartCoroutine(FadeTo(1f, fadeDuration));
        yield return new WaitForSeconds(showUIDuration);
        yield return StartCoroutine(FadeTo(0f, fadeDuration));
    }

    // Helper biar tidak perlu buat Color temporary di luar coroutine
    private void SetTextAlpha(float alpha)
    {
        Color c = sprintUI.color;
        c.a = alpha;
        sprintUI.color = c;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = sprintUI.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            SetTextAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration));
            yield return null;
        }

        SetTextAlpha(targetAlpha); // pastikan nilai akhir tepat
    }

    // ─── Sprint Logic ────────────────────────────────────────────────────────────
    private IEnumerator SprintCoroutine()
    {
        IsSprinting = true;

        // Kurangi stamina langsung saat sprint dimulai
        if (GameManager.instance.player.TryGetComponent(out PlayerStamina stamina))
            stamina.ReduceStamina(staminaDrainOnSprint);

        OnSprintStart?.Invoke(sprintSpeed, monsterRetreatPerSec);

        yield return new WaitForSeconds(sprintDuration);

        OnSprintEnd?.Invoke();

        IsSprinting = false;
    }
}
