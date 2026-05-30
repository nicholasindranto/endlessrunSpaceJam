using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FTUESeq1 : MonoBehaviour
{
    /*
    1. gerakin obstacle dan weaponnya
    2. kalau obstaclenya nyentuh collider, maka pause semua gameplay
       freeze character, ignore all input except for right, object tidak gerak
    3. tampilkan uinya
    4. tunggu sampai player menekan tombol kanan
    5. jalanin lagi all gameplay
       unfreeze character, stop ignoring input, object gerak lagi, ilangin uinya
    6. masuk ke sequence 2
    */

    // reference ke obstacle dan weaponnya
    [SerializeField] private GameObject obstacle;
    [SerializeField] private GameObject weapon;
    [SerializeField] private GameObject energy;

    // event untuk freeze character
    public static event Action OnAllCharacterFreeze;
    public static event Action OnAllCharacterUnfreeze;
    public static event Action OnIgnoreAllInputExceptRight;
    public static event Action OnStopIgnoringInput;
    public static event Action OnEnterSequence2;
    public static event Action OnTimerStopAndStaminaStop;
    public static event Action OnTimerAndStaminaResume;

    // reference ke uinya
    public TextMeshProUGUI sequence1UIText;
    public float fadeDuration;

    private void OnEnable() {
        MainMenuManager.OnFTUEStart += StartFTUESequence1;

        TunnelRotate.EndOfSequence1 += HandleEndOfSequence1;
    }

    private void OnDisable() {
        MainMenuManager.OnFTUEStart -= StartFTUESequence1;

        TunnelRotate.EndOfSequence1 -= HandleEndOfSequence1;
    }

    private void StartFTUESequence1()
    {
        // gerakin dulu kedua objectnya
        if (obstacle.TryGetComponent(out AutoMoveObj obstacleScript))
            obstacleScript.isMoving = true;
        if (weapon.TryGetComponent(out AutoMoveObj weaponScript))
            weaponScript.isMoving = true;
        if (energy.TryGetComponent(out AutoMoveObj energyScript))
            energyScript.isMoving = true;
    }

    private void HandleEndOfSequence1()
    {
        // jalanin kedua objectnya
        if (obstacle.TryGetComponent(out AutoMoveObj obstacleScript))
            obstacleScript.isMoving = true;
        if (weapon.TryGetComponent(out AutoMoveObj weaponScript))
            weaponScript.isMoving = true;
        if (energy.TryGetComponent(out AutoMoveObj energyScript))
            energyScript.isMoving = true;
        // trigger event untuk freeze character
        OnAllCharacterUnfreeze?.Invoke();
        OnStopIgnoringInput?.Invoke();
        OnTimerAndStaminaResume?.Invoke();

        // ilangin uinya
        StartCoroutine(FadeTextUI(sequence1UIText, fadeDuration, false));

        // masuk ke sequence kedua
        OnEnterSequence2?.Invoke();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Obstacle"))
        {
            // pause gameplay
            OnAllCharacterFreeze?.Invoke();
            OnIgnoreAllInputExceptRight?.Invoke();
            OnTimerStopAndStaminaStop?.Invoke();

            // tampilkan uinya
            StartCoroutine(FadeTextUI(sequence1UIText, fadeDuration, true));

            // object berhenti
            if (obstacle.TryGetComponent(out AutoMoveObj obstacleScript))
                obstacleScript.isMoving = false;
            if (weapon.TryGetComponent(out AutoMoveObj weaponScript))
                weaponScript.isMoving = false;
            if (energy.TryGetComponent(out AutoMoveObj energyScript))
                energyScript.isMoving = false;
        }
    }

    private IEnumerator FadeTextUI(TextMeshProUGUI text, float duration, bool inOrOut)
    {
        float elapsed = 0f;
        Color color = text.color; // Ambil warna dasar teks

        float startAlpha = inOrOut ? 0f : 1f;
        float endAlpha = inOrOut ? 1f : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Gunakan startAlpha dan endAlpha yang sudah pasti, bukan dari text.color saat itu
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            text.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // Pastikan nilai akhir benar-benar presisi
        text.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}
