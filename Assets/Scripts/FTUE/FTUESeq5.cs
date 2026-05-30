using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FTUESeq5 : MonoBehaviour
{
    /*
    1. energy datang ke arahnya
    2. kalau energynya nyentuh collider, maka pause semua gameplay
       freeze character, ignore all input, object tidak gerak, show ui nya juga
    3. tampilkan uinya selama 3 detik
    4. jalanin lagi all gameplay
       unfreeze character, stop ignoring input, ilangin uinya
    5. FTUE selesai
    */

    // reference ke energy object
    [SerializeField] private GameObject energy;

    // reference ke uinya
    public Image sequence5UIText;
    public float fadeDuration;

    private void OnEnable() {
        FTUESeq4.OnEnterSequence5 += StartFTUESequence5;
    }

    private void OnDisable() {
        FTUESeq4.OnEnterSequence5 -= StartFTUESequence5;
    }

    private void StartFTUESequence5()
    {
        // disable all input dulu
        // OnIgnoreAllInput?.Invoke();

        // pastikan jalanin energy objectnya
        if (energy.TryGetComponent(out AutoMoveObj energyScript))
            energyScript.isMoving = true;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Energy"))
        {
            // kalau bukan di sequence 5, maka skip
            if (FTUEManager.instance.currentSequence != 5) return;

            StartCoroutine(HandleEndOfSequence5());
        }
    }

    private IEnumerator HandleEndOfSequence5()
    {
        // pause gameplay
        // OnAllCharacterFreeze?.Invoke();
        // OnIgnoreAllInput?.Invoke(); // make sure aja
        // OnTimerStopAndStaminaStop?.Invoke();

        // tampilkan uinya
        StartCoroutine(FadeTextUI(sequence5UIText, fadeDuration, true));

        // object berhenti
        // if (energy.TryGetComponent(out AutoMoveObj energyScript))
        //     energyScript.isMoving = false;

        // tunggu selama 3 detik
        yield return new WaitForSeconds(3f);

        // jalanin energy objectnya
        // if (energy.TryGetComponent(out AutoMoveObj energyScript))
        //     energyScript.isMoving = true;

        // trigger event untuk unfreeze character
        // OnAllCharacterUnfreeze?.Invoke();
        // OnStopIgnoringInput?.Invoke();
        // OnTimerAndStaminaResume?.Invoke();

        // ilangin uinya
        StartCoroutine(FadeTextUI(sequence5UIText, fadeDuration, false));

        // FTUE selesai, bisa masuk ke gameplay normal
        FTUEManager.instance.isFTUEFinished = true;
        // game objectnya di disable aja biar nggak mengaktifkan mekanisme ftue nya lagi
        FTUEManager.instance.gameObject.SetActive(false);
        // reset sequence nya, biar kalau misal player balik ke main menu lagi, ftue nya bisa jalan dari awal lagi
        FTUEManager.instance.currentSequence = 0;
        // matiin uinya juga
        FTUEManager.instance.ftueUI.SetActive(false);
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
