using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FTUESeq4 : MonoBehaviour
{
    /*
    1. disable all input dulu
    2. masuk coroutine, tunggu 2 detik lalu 
       freeze character, ignore all input, show ui nya juga
    3. tunggu pemain tekan tombol atas
    4. jalanin lagi all gameplay
       unfreeze character, stop ignoring input, ilangin uinya
    5. start energy movement dan masuk ke sequence 5
    */

    // reference ke energy object
    [SerializeField] private GameObject energy;

    // event untuk freeze character
    public static event Action OnAllCharacterFreeze;
    public static event Action OnAllCharacterUnfreeze;
    public static event Action OnIgnoreAllInputExceptUp;
    public static event Action OnStopIgnoringInput;
    public static event Action OnEnterSequence5;
    public static event Action OnTimerStopAndStaminaStop;
    public static event Action OnTimerAndStaminaResume;

    // reference ke uinya
    public TextMeshProUGUI sequence4UIText;
    public float fadeDuration;

    private void OnEnable() {
        FTUESeq3.OnEnterSequence4 += StartFTUESequence4;

        PlayerSprint.EndOfSequence4 += HandleEndOfSequence4;
    }

    private void OnDisable() {
        FTUESeq3.OnEnterSequence4 -= StartFTUESequence4;

        PlayerSprint.EndOfSequence4 -= HandleEndOfSequence4;
    }

    private void HandleEndOfSequence4()
    {
        // trigger event untuk unfreeze character
        OnAllCharacterUnfreeze?.Invoke();
        OnStopIgnoringInput?.Invoke();
        OnTimerAndStaminaResume?.Invoke();
        
        // ilangin uinya
        StartCoroutine(FadeTextUI(sequence4UIText, fadeDuration, false));

        // start energy movement dan masuk ke sequence 5
        if (energy.TryGetComponent(out AutoMoveObj energyScript))
            energyScript.isMoving = true;

        // trigger masuk ke sequence 5
        OnEnterSequence5?.Invoke();
    }

    private void StartFTUESequence4()
    {
        // disable all input dulu
        OnIgnoreAllInputExceptUp?.Invoke();

        // masuk coroutinenya
        StartCoroutine(HandleSequence4());
    }

    private IEnumerator HandleSequence4()
    {
        yield return new WaitForSeconds(5.5f);

        // pause gameplay
        OnAllCharacterFreeze?.Invoke();
        OnIgnoreAllInputExceptUp?.Invoke(); // make sure aja
        OnTimerStopAndStaminaStop?.Invoke();

        // tampilkan uinya
        StartCoroutine(FadeTextUI(sequence4UIText, fadeDuration, true));
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
