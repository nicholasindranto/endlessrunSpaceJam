using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class FTUEGameplaySequence : MonoBehaviour
{
    // ─── Singleton ───────────────────────────────────────────────────────────────
    public static FTUEGameplaySequence instance;

    // ─── FTUE State ──────────────────────────────────────────────────────────────
    public bool isFTUEFinished;
    
    // input restriction mode
    public enum InputMode { AllDisabled, OnlyRight, OnlyDown, OnlyUp, AllEnabled }
    private InputMode currentInputMode = InputMode.AllEnabled;

    // ─── Events untuk control input & character ──────────────────────────────────
    public static event Action OnDisableAllInput;
    public static event Action OnEnableAllInput;
    public static event Action OnAllCharacterFreeze;
    public static event Action OnAllCharacterUnfreeze;
    
    // event buat notify input mode change
    public static event Action<InputMode> OnInputModeChange;

    // ─── FTUE Sequence State ────────────────────────────────────────────────────
    private int currentSequence = 0;
    private bool isWaitingForInput = false;
    private bool weaponCollectedInSequence2 = false; // flag untuk detect weapon collection

    // ─── UI References ──────────────────────────────────────────────────────────
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private TextMeshProUGUI sequenceUI;
    
    // UI text untuk setiap sequence
    private string[] sequenceTexts = new string[4]
    {
        "Obstacle is coming, avoid it by go to the right side!",
        "You found a weapon, use it to attack the monster.",
        "The monster is getting closer! Attack it before it catch you!",
        "Sprint to run faster and make the monster fall behind."
    };

    // reference ke obstacle dan weaponnya
    [SerializeField] private GameObject obstacle;
    [SerializeField] private GameObject weapon;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        MainMenuManager.OnFTUEStart += StartFTUESequence;
        SwipeController.OnSwipeRight += HandleFTUESwipeRight;
        SwipeController.OnSwipeDown += HandleFTUESwipeDown;
        SwipeController.OnSwipeUp += HandleFTUESwipeUp;
        PlayerWeapon.OnWeaponCollected += HandleWeaponCollected;
        
        // Subscribe ke keyboard input events
        // TunnelRotate.OnKeyboardRightInput += HandleFTUEKeyboardRight;
        // PlayerWeapon.OnKeyboardDownInput += HandleFTUEKeyboardDown;
        // PlayerSprint.OnKeyboardUpInput += HandleFTUEKeyboardUp;
    }

    private void OnDisable()
    {
        MainMenuManager.OnFTUEStart -= StartFTUESequence;
        SwipeController.OnSwipeRight -= HandleFTUESwipeRight;
        SwipeController.OnSwipeDown -= HandleFTUESwipeDown;
        SwipeController.OnSwipeUp -= HandleFTUESwipeUp;
        PlayerWeapon.OnWeaponCollected -= HandleWeaponCollected;
        
        // Unsubscribe dari keyboard input events
        // TunnelRotate.OnKeyboardRightInput -= HandleFTUEKeyboardRight;
        // PlayerWeapon.OnKeyboardDownInput -= HandleFTUEKeyboardDown;
        // PlayerSprint.OnKeyboardUpInput -= HandleFTUEKeyboardUp;
    }

    private void StartFTUESequence()
    {
        // mulai FTUE dari sequence 0
        currentSequence = 0;
        isFTUEFinished = false;
    }

    // ─── Input Handlers untuk FTUE ───────────────────────────────────────────────
    private void HandleFTUESwipeRight()
    {
        if (!isWaitingForInput) return;
        if (currentInputMode != InputMode.OnlyRight) return;
        
        isWaitingForInput = false;
    }

    private void HandleFTUEKeyboardRight()
    {
        if (!isWaitingForInput) return;
        if (currentInputMode != InputMode.OnlyRight) return;
        
        isWaitingForInput = false;
    }

    private void HandleFTUESwipeDown()
    {
        if (!isWaitingForInput) return;
        if (currentInputMode != InputMode.OnlyDown) return;
        
        isWaitingForInput = false;
    }

    private void HandleFTUEKeyboardDown()
    {
        if (!isWaitingForInput) return;
        if (currentInputMode != InputMode.OnlyDown) return;
        
        isWaitingForInput = false;
    }

    private void HandleFTUESwipeUp()
    {
        if (!isWaitingForInput) return;
        if (currentInputMode != InputMode.OnlyUp) return;
        
        isWaitingForInput = false;
    }

    private void HandleFTUEKeyboardUp()
    {
        if (!isWaitingForInput) return;
        if (currentInputMode != InputMode.OnlyUp) return;
        
        isWaitingForInput = false;
    }

    private void HandleWeaponCollected()
    {
        // Flag bahwa weapon sudah collected di sequence 2
        if (currentSequence == 2)
        {
            weaponCollectedInSequence2 = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Hanya jalan kalau FTUE belum selesai
        if (isFTUEFinished) return;

        if (other.CompareTag("Obstacle") && currentSequence == 0)
        {
            currentSequence++;
            
            // Sequence 1: Show UI, restrict input to right only
            SetInputMode(InputMode.OnlyRight);
            OnAllCharacterFreeze?.Invoke(); // Freeze backward movement
            
            // Stop obstacle movement (di-pause dulu)
            if (other.TryGetComponent(out AutoMoveObj obstacleScript))
                obstacleScript.isMoving = false;

            StartCoroutine(Sequence1_ObstacleAvoidance(other)); // Pass obstacle object
        }
        else if (other.CompareTag("Weapon") && currentSequence == 1)
        {
            currentSequence++;
            
            // Sequence 2: Pause 3 detik sebelum resume movement
            SetInputMode(InputMode.AllDisabled);
            OnDisableAllInput?.Invoke();
            OnAllCharacterFreeze?.Invoke();
            
            // Stop weapon movement
            if (other.TryGetComponent(out AutoMoveObj weaponScript))
                weaponScript.isMoving = false;

            StartCoroutine(Sequence2_WeaponPickup());
        }
        else if (other.CompareTag("Monster") && currentSequence == 2)
        {
            currentSequence++;
            
            // Sequence 3: Show UI, wait for player to attack
            SetInputMode(InputMode.OnlyDown);
            OnDisableAllInput?.Invoke();
            OnAllCharacterFreeze?.Invoke();
            
            // Stop monster movement
            if (other.TryGetComponent(out AutoMoveObj monsterScript))
                monsterScript.isMoving = false;

            StartCoroutine(Sequence3_MonsterAttack());
        }
    }

    // ─── Sequence Methods ────────────────────────────────────────────────────────

    private IEnumerator Sequence1_ObstacleAvoidance(Collider obstacleCollider)
    {
        // Show UI
        sequenceUI.text = sequenceTexts[0];
        yield return StartCoroutine(FadeTextUI(sequenceUI, fadeDuration, true));

        // Wait untuk player input (swipe right / D / Right Arrow)
        isWaitingForInput = true;
        yield return new WaitUntil(() => !isWaitingForInput);

        // Fade out UI
        yield return StartCoroutine(FadeTextUI(sequenceUI, fadeDuration, false));

        // Resume gameplay: unfreeze character & obstacle mulai gerak lagi
        OnAllCharacterUnfreeze?.Invoke();
        SetInputMode(InputMode.AllEnabled);
        
        // Resume obstacle movement
        if (obstacleCollider.TryGetComponent(out AutoMoveObj obstacleScript))
            obstacleScript.isMoving = true;
    }

    private IEnumerator AdvanceFromSequence1()
    {
        // This method is not used anymore, keeping it for safety
        yield break;
    }

    private IEnumerator Sequence2_WeaponPickup()
    {
        // Show UI dulu
        sequenceUI.text = sequenceTexts[1];
        yield return StartCoroutine(FadeTextUI(sequenceUI, fadeDuration, true));

        // Pause selama 3 detik (semua freeze, no input)
        yield return new WaitForSeconds(3f);

        // Fade out UI
        yield return StartCoroutine(FadeTextUI(sequenceUI, fadeDuration, false));

        // Unfreeze character movement TAPI tetap disable input
        // Jadi player bisa dapat weapon sambil bergerak
        OnAllCharacterUnfreeze?.Invoke();
        // SetInputMode tetap AllDisabled supaya player nggak bisa input
        
        // Wait untuk weapon di-collect (automatic via OnWeaponCollected event)
        yield return new WaitUntil(() => weaponCollectedInSequence2);

        // Weapon sudah di-collect! Tunggu monster datang
        // Input tetap disabled, character tetap bisa gerak
        yield return new WaitUntil(() => currentSequence > 2); // Wait sampai monster trigger OnTriggerEnter
    }

    private IEnumerator Sequence3_MonsterAttack()
    {
        // Show UI
        sequenceUI.text = sequenceTexts[2];
        yield return StartCoroutine(FadeTextUI(sequenceUI, fadeDuration, true));

        // Wait untuk player attack input (swipe down / S / Down Arrow)
        isWaitingForInput = true;
        yield return new WaitUntil(() => !isWaitingForInput);

        // Fade out UI
        yield return StartCoroutine(FadeTextUI(sequenceUI, fadeDuration, false));

        // Resume gameplay sepenuhnya
        isFTUEFinished = true;
        OnEnableAllInput?.Invoke();
        OnAllCharacterUnfreeze?.Invoke();
        SetInputMode(InputMode.AllEnabled);
    }

    private IEnumerator AdvanceFromSequence3()
    {
        // This method is not used anymore, keeping it for safety
        yield break;
    }

    // ─── Helper Methods ─────────────────────────────────────────────────────────
    
    private void SetInputMode(InputMode mode)
    {
        currentInputMode = mode;
        OnInputModeChange?.Invoke(mode);
    }

    private IEnumerator FadeTextUI(TextMeshProUGUI text, float duration, bool inOrOut)
    {
        float elapsed = 0f;
        Color startColor = text.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(inOrOut ? 0f : 1f, inOrOut ? 1f : 0f, elapsed / duration);
            text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        text.color = new Color(startColor.r, startColor.g, startColor.b, inOrOut ? 1f : 0f);
    }
}
