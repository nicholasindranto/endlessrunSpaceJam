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
    
    // event untuk FTUE: ketika W/Up Arrow di-tekan untuk sprint
    // public static event Action OnKeyboardUpInput;

    // uinya
    public TextMeshProUGUI sprintUI;
    public int showUIDuration;
    public float fadeDuration;

    // audionya
    [SerializeField] private AudioClip sprintSFX;
    [SerializeField] private AudioSource source;

    public bool CanUseInputOnSequence1
    {
        private set;
        get;
    }
    public bool CanUseInputOnSequence2
    {
        private set;
        get;
    }
    public bool CanUseInputOnSequence3
    {
        private set;
        get;
    }
    public bool CanUseInputOnSequence4
    {
        private set;
        get;
    }

    // lagi di sequence 4 kah?
    public bool IsInSequence4 { get; private set; }

    // event end of sequence 4
    public static event Action EndOfSequence4;

    private void OnEnable() {
        SwipeController.OnSwipeUp += HandleSwipeUp;

        // subscribe ke FTUE nya
        // FTUEGameplaySequence.OnDisableAllInput += () => CanUseInput = false;
        // FTUEGameplaySequence.OnEnableAllInput += () => CanUseInput = true;

        FTUESeq1.OnIgnoreAllInputExceptRight += () => CanUseInputOnSequence1 = false;
        FTUESeq1.OnStopIgnoringInput += () => CanUseInputOnSequence1 = true;

        FTUESeq2.OnIgnoreAllInput += () => CanUseInputOnSequence2 = false;
        FTUESeq2.OnStopIgnoringInput += () => CanUseInputOnSequence2 = true;

        FTUESeq3.OnIgnoreAllInputExceptDown += () => CanUseInputOnSequence3 = false;
        FTUESeq3.OnStopIgnoringInput += () => CanUseInputOnSequence3 = true;

        FTUESeq4.OnIgnoreAllInputExceptUp += HandleOnSequence4;
        FTUESeq4.OnStopIgnoringInput += () => CanUseInputOnSequence4 = true;


    }

    private void OnDisable() {
        SwipeController.OnSwipeUp -= HandleSwipeUp;

        FTUESeq1.OnIgnoreAllInputExceptRight -= () => CanUseInputOnSequence1 = false;
        FTUESeq1.OnStopIgnoringInput -= () => CanUseInputOnSequence1 = true;

        FTUESeq2.OnIgnoreAllInput -= () => CanUseInputOnSequence2 = false;
        FTUESeq2.OnStopIgnoringInput -= () => CanUseInputOnSequence2 = true;

        FTUESeq3.OnIgnoreAllInputExceptDown -= () => CanUseInputOnSequence3 = false;
        FTUESeq3.OnStopIgnoringInput -= () => CanUseInputOnSequence3 = true;

        FTUESeq4.OnIgnoreAllInputExceptUp -= HandleOnSequence4;
        FTUESeq4.OnStopIgnoringInput -= () => CanUseInputOnSequence4 = true;
    }

    private void HandleOnSequence4()
    {
        CanUseInputOnSequence4 = false;
        IsInSequence4 = true;
    }

    private void Start() {
        IsSprinting = false; // di awal nggak mungkin lari dong...
        // set can use inputnya true
        CanUseInputOnSequence1 = true;

        CanUseInputOnSequence2 = true;

        CanUseInputOnSequence3 = true;

        // set can use inputnya true
        CanUseInputOnSequence4 = true;
        IsInSequence4 = false;
    }

    private void HandleSwipeUp()
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) return;

        if (IsSprinting) return;

        // apakah bisa menggunakan input?
        if (!CanUseInputOnSequence1 || !CanUseInputOnSequence2 || !CanUseInputOnSequence3) return;

        // kalau lagi di sequence 4 maka invoke event nya
        if (IsInSequence4)
        {
            EndOfSequence4?.Invoke();

            // sudah tidak di sequence 4
            IsInSequence4 = false;
        }

        // play sfx nya
        source.PlayOneShot(sprintSFX);

        // Hanya bisa sprint saat stamina Normal (> 50% / hijau)
        if (PlayerStamina.CurrentTier != PlayerStamina.StaminaTier.Normal)
        {
            Debug.Log($"Stamina tidak cukup untuk sprint!");
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

        // apakah bisa menggunakan input?
        if (!CanUseInputOnSequence1 || !CanUseInputOnSequence2 || !CanUseInputOnSequence3) return;

        // kalau lagi di sequence 4 maka invoke event nya
        if (IsInSequence4)
        {
            EndOfSequence4?.Invoke();

            // sudah tidak di sequence 4
            IsInSequence4 = false;
        }

        // Trigger event untuk FTUE
        // OnKeyboardUpInput?.Invoke();

        // play sfx nya
        source.PlayOneShot(sprintSFX);

        // Hanya bisa sprint saat stamina Normal (> 50% / hijau)
        if (PlayerStamina.CurrentTier != PlayerStamina.StaminaTier.Normal)
        {
            Debug.Log("Stamina tidak cukup untuk sprint! current = {PlayerStamina.CurrentTier}");
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
