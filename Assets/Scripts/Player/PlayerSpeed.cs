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
    }

    private void OnDisable() {
        PlayerStamina.OnStaminaTierChange -= HandleStaminaTierChange;

        PlayerWeapon.OnAttackStart -= HandleAttackStart;

        PlayerWeapon.OnAttackEnd -= HandleAttackEnd;
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
