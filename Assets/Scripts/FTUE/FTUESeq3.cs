using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FTUESeq3 : MonoBehaviour
{
    /*
    1. gerakin obstacle dan weaponnya
    2. disable all input dulu
    3. kalau monsternya nyentuh collider, maka pause semua gameplay
       freeze character, ignore all input, object tidak gerak, show ui nya juga
    4. tunggu pemain tekan tombol bawah
    5. jalanin lagi all gameplay
       unfreeze character, stop ignoring input, object gerak lagi, ilangin uinya
    6. masuk ke sequence 4
    */

    // reference ke obstacle dan weaponnya
    [SerializeField] private GameObject weapon;
    [SerializeField] private GameObject energy;

    // event untuk freeze character
    public static event Action OnAllCharacterFreeze;
    public static event Action OnAllCharacterUnfreeze;
    public static event Action OnIgnoreAllInputExceptDown;
    public static event Action OnStopIgnoringInput;
    public static event Action OnEnterSequence4;
    public static event Action OnTimerStopAndStaminaStop;
    public static event Action OnTimerAndStaminaResume;

    // reference ke uinya
    public Image sequence3UIText;
    public float fadeDuration;

    private void OnEnable() {
        FTUESeq2.OnEnterSequence3 += StartFTUESequence3;

        PlayerWeapon.EndOfSequence3 += HandleEndOfSequence3;
    }

    private void OnDisable() {
        FTUESeq2.OnEnterSequence3 -= StartFTUESequence3;

        PlayerWeapon.EndOfSequence3 -= HandleEndOfSequence3;
    }

    private void HandleEndOfSequence3()
    {
        // jalanin kedua objectnya
        if (weapon.TryGetComponent(out AutoMoveObj weaponScript))
            weaponScript.isMoving = true;
        if (energy.TryGetComponent(out AutoMoveObj energyScript))
            energyScript.isMoving = true;

        // trigger event untuk freeze character
        OnAllCharacterUnfreeze?.Invoke();
        OnStopIgnoringInput?.Invoke();
        OnTimerAndStaminaResume?.Invoke();
        // ilangin uinya
        StartCoroutine(FadeTextUI(sequence3UIText, fadeDuration, false));

        // masuk ke sequence keempat
        OnEnterSequence4?.Invoke();
    }

    private void StartFTUESequence3()
    {
        // gerakin dulu kedua objectnya
        if (weapon.TryGetComponent(out AutoMoveObj weaponScript))
            weaponScript.isMoving = true;
        if (energy.TryGetComponent(out AutoMoveObj energyScript))
            energyScript.isMoving = true;

        // disable all input dulu
        OnIgnoreAllInputExceptDown?.Invoke();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Monster"))
        {
            // kalau bukan di sequence 3, maka skip
            if (FTUEManager.instance.currentSequence != 3) return;

            // pause gameplay
            OnAllCharacterFreeze?.Invoke();
            OnIgnoreAllInputExceptDown?.Invoke();
            OnTimerStopAndStaminaStop?.Invoke();

            // tampilkan uinya
            StartCoroutine(FadeTextUI(sequence3UIText, fadeDuration, true));

            // object berhenti
            if (weapon.TryGetComponent(out AutoMoveObj weaponScript))
                weaponScript.isMoving = false;
            if (energy.TryGetComponent(out AutoMoveObj energyScript))
                energyScript.isMoving = false;
        }
    }

    private IEnumerator FadeTextUI(Image image, float duration, bool inOrOut)
    {
        float elapsed = 0f;
        Color color = image.color; // Ambil warna dasar gambar

        float startAlpha = inOrOut ? 0f : 1f;
        float endAlpha = inOrOut ? 1f : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Gunakan startAlpha dan endAlpha yang sudah pasti, bukan dari text.color saat itu
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            image.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // Pastikan nilai akhir benar-benar presisi
        image.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}
