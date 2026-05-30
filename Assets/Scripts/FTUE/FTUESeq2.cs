using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FTUESeq2 : MonoBehaviour
{
    /*
    1. gerakin obstacle dan weaponnya
    2. disable all input dulu
    3. kalau weaponnya nyentuh collider, maka masuk coroutine untuk pause gameplay
       freeze character, ignore all input, object tidak gerak, show ui nya juga
    4. tunggu 3 detik
    5. jalanin lagi all gameplay
       unfreeze character, stop ignoring input, object gerak lagi, ilangin uinya
    6. masuk ke sequence 3
    */

    // reference ke obstacle dan weaponnya
    [SerializeField] private GameObject obstacle;
    [SerializeField] private GameObject weapon;
    [SerializeField] private GameObject energy;

    // event untuk freeze character
    public static event Action OnAllCharacterFreeze;
    public static event Action OnAllCharacterUnfreeze;
    public static event Action OnIgnoreAllInput;
    public static event Action OnStopIgnoringInput;
    public static event Action OnEnterSequence3;
    public static event Action OnTimerStopAndStaminaStop;
    public static event Action OnTimerAndStaminaResume;

    // reference ke uinya
    public TextMeshProUGUI sequence2UIText;
    public float fadeDuration;

    private void OnEnable() {
        FTUESeq1.OnEnterSequence2 += StartFTUESequence2;
    }

    private void OnDisable() {
        FTUESeq1.OnEnterSequence2 -= StartFTUESequence2;
    }

    private void StartFTUESequence2()
    {
        // gerakin dulu kedua objectnya
        if (obstacle.TryGetComponent(out AutoMoveObj obstacleScript))
            obstacleScript.isMoving = true;
        if (weapon.TryGetComponent(out AutoMoveObj weaponScript))
            weaponScript.isMoving = true;
        if (energy.TryGetComponent(out AutoMoveObj energyScript))
            energyScript.isMoving = true;

        // disable all input dulu
        OnIgnoreAllInput?.Invoke();
    }

    private IEnumerator HandleEndOfSequence2()
    {
        // pause gameplay
        OnAllCharacterFreeze?.Invoke();
        OnIgnoreAllInput?.Invoke(); // make sure aja
        OnTimerStopAndStaminaStop?.Invoke();

        // tampilkan uinya
        StartCoroutine(FadeTextUI(sequence2UIText, fadeDuration, true));

        // object berhenti
        if (obstacle.TryGetComponent(out AutoMoveObj obstacleScript))
            obstacleScript.isMoving = false;
        if (weapon.TryGetComponent(out AutoMoveObj weaponScript))
            weaponScript.isMoving = false;
        if (energy.TryGetComponent(out AutoMoveObj energyScript))
            energyScript.isMoving = false;

        // tunggu selama 3 detik
        yield return new WaitForSeconds(3f);

        // jalanin kedua objectnya
        obstacleScript.isMoving = true;
        weaponScript.isMoving = true;
        energyScript.isMoving = true;

        // trigger event untuk freeze character
        OnAllCharacterUnfreeze?.Invoke();
        OnStopIgnoringInput?.Invoke();
        OnTimerAndStaminaResume?.Invoke();

        // ilangin uinya
        StartCoroutine(FadeTextUI(sequence2UIText, fadeDuration, false));

        // masuk ke sequence ketiga
        OnEnterSequence3?.Invoke();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Weapon"))
        {
            // kalau bukan di sequence 2, maka skip
            if (FTUEManager.instance.currentSequence != 2) return;

            StartCoroutine(HandleEndOfSequence2());
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
