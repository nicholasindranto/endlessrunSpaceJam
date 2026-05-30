using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpeed : MonoBehaviour
{
    // basis speed nya, alias speed awalnya
    public int baseSpeed = 10;

    public int currentSpeed;

    // below 50% speed penalty
    [SerializeField] private int below50SpeedPenalty = 3;

    // below 25% speed penalty
    [SerializeField] private int below25SpeedPenalty = 7;

    // untuk nyimpen speednya sebelum attack
    private int speedBeforeAttack;
    private int speedBeforeSprint;

    // record speed sebelum freeze karena FTUE
    private int speedBeforeFreeze;

    // reference ke animatornya
    [SerializeField] private Animator anim;
    // nama param nya
    public readonly string runParamName = "IsRunning";

    private void Awake() {
        currentSpeed = baseSpeed; // set dulu speed nya
    }

    private void OnEnable() {
        // subscribe ke event nya stamina
        PlayerStamina.OnStaminaTierChange += HandleStaminaTierChange;

        // subscribe ke player weaponnya biar bisa slowmo
        PlayerWeapon.OnAttackStart += HandleAttackStart;

        // subscribe biar speednya balik normal
        PlayerWeapon.OnAttackEnd += HandleAttackEnd;

        PlayerSprint.OnSprintStart        += HandleSprintStart;   // ← tambahan
        PlayerSprint.OnSprintEnd          += HandleSprintEnd;     // ← tambahan

        // subscribe pas characternya freeze
        // FTUEGameplaySequence.OnAllCharacterFreeze += HandlePlayerFreeze;
        // FTUEGameplaySequence.OnAllCharacterUnfreeze += HandlePlayerUnfreeze;

        FTUESeq1.OnAllCharacterFreeze += HandlePlayerFreeze;
        FTUESeq1.OnAllCharacterUnfreeze += HandlePlayerUnfreeze;

        FTUESeq2.OnAllCharacterFreeze += HandlePlayerFreeze;
        FTUESeq2.OnAllCharacterUnfreeze += HandlePlayerUnfreeze;

        FTUESeq3.OnAllCharacterFreeze += HandlePlayerFreeze;
        FTUESeq3.OnAllCharacterUnfreeze += HandlePlayerUnfreeze;

        FTUESeq4.OnAllCharacterFreeze += HandlePlayerFreeze;
        FTUESeq4.OnAllCharacterUnfreeze += HandlePlayerUnfreeze;
    }

    private void OnDisable() {
        PlayerStamina.OnStaminaTierChange -= HandleStaminaTierChange;

        PlayerWeapon.OnAttackStart -= HandleAttackStart;

        PlayerWeapon.OnAttackEnd -= HandleAttackEnd;

        PlayerSprint.OnSprintStart        -= HandleSprintStart;   // ← tambahan
        PlayerSprint.OnSprintEnd          -= HandleSprintEnd;     // ← tambahan

        // unsubscribe pas characternya freeze
        // FTUEGameplaySequence.OnAllCharacterFreeze -= HandlePlayerFreeze;
        // FTUEGameplaySequence.OnAllCharacterUnfreeze -= HandlePlayerUnfreeze;

        FTUESeq1.OnAllCharacterFreeze -= HandlePlayerFreeze;
        FTUESeq1.OnAllCharacterUnfreeze -= HandlePlayerUnfreeze;

        FTUESeq2.OnAllCharacterFreeze -= HandlePlayerFreeze;
        FTUESeq2.OnAllCharacterUnfreeze -= HandlePlayerUnfreeze;

        FTUESeq3.OnAllCharacterFreeze -= HandlePlayerFreeze;
        FTUESeq3.OnAllCharacterUnfreeze -= HandlePlayerUnfreeze;

        FTUESeq4.OnAllCharacterFreeze -= HandlePlayerFreeze;
        FTUESeq4.OnAllCharacterUnfreeze -= HandlePlayerUnfreeze;
    }

    private void HandlePlayerFreeze()
    {
        // simpen dulu speed sebelum freeze
        speedBeforeFreeze = currentSpeed;

        // kalau freeze maka speednya 0
        currentSpeed = 0;

        // matiin animasinya
        anim.speed = 0f;
    }

    private void HandlePlayerUnfreeze()
    {
        // kalau unfreeze maka balik ke speed normal
        currentSpeed = speedBeforeFreeze;

        // nyalain lagi animasinya
        anim.speed = 1f;
    }

    private void HandleSprintStart(int sprintSpeed, float _)
    {
        speedBeforeSprint = currentSpeed; // simpan speed sebelum sprint
        currentSpeed      = sprintSpeed;
    }

    private void HandleSprintEnd()
    {
        currentSpeed = speedBeforeSprint; // kembalikan ke speed sebelum sprint
    }

    private void HandleAttackStart(int slowSpeed)
    {
        speedBeforeAttack = currentSpeed; // di store dulu biar bisa balik ke speed semula
        currentSpeed = slowSpeed; // di slow mo
    }

    private void HandleAttackEnd()
    {
        currentSpeed = speedBeforeAttack; // balikin ke semula
    }

    public void ChangeSpeed(int percentageBelow)
    {
        switch (percentageBelow)
        {
            case 50:
                currentSpeed -= below50SpeedPenalty;
                break;
            case 25:
                currentSpeed -= below25SpeedPenalty;
                break;
            default:
                Debug.Log("nggak ada");
                break;
        }
    }

    private void HandleStaminaTierChange(PlayerStamina.StaminaTier staminaTier)
    {
        // set nya dari base speed
        currentSpeed = staminaTier switch
        {
            PlayerStamina.StaminaTier.Normal => baseSpeed,
            PlayerStamina.StaminaTier.Below50 => baseSpeed - below50SpeedPenalty,
            PlayerStamina.StaminaTier.Below25 => baseSpeed - below25SpeedPenalty,
            _ => baseSpeed
        };
    }
}
