using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager instance;

    private void Awake() {
        instance = this;
    }

    [SerializeField] private AudioClip gameStartSFX;
    [SerializeField] private AudioSource source;

    [SerializeField] private float fadeDuration = 0.5f;

    public bool isAlreadyStarted;

    // Hubungkan ke OnClick di Inspector → drag GameObject button-nya ke parameter
    public void OnButtonClick(GameObject buttonObj)
    {
        isAlreadyStarted = true;

        if (source != null && gameStartSFX != null)
            source.PlayOneShot(gameStartSFX);

        if (buttonObj.TryGetComponent(out Image img))
            StartCoroutine(FadeOutImage(img));
    }

    private IEnumerator FadeOutImage(Image img)
    {
        // Nonaktifkan button supaya tidak bisa diklik lagi selama/sesudah fade
        if (img.TryGetComponent(out Button btn)) btn.interactable = false;

        float elapsed = 0f;
        Color startColor = img.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            img.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        img.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }
}
