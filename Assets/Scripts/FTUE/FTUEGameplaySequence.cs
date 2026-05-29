using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FTUEGameplaySequence : MonoBehaviour
{
    // reference instance
    public static FTUEGameplaySequence instance;

    // apakah FTUE nya udah selesai
    public bool isFTUEFinished;

    // reference ke object 

    // event buat mendisable all input
    public static event Action OnDisableAllInput;
    // sebaliknya, diaktifin lagi
    public static event Action OnEnableAllInput;

    // event all character freeze
    public static event Action OnAllCharacterFreeze;
    // event all character unfreeze
    public static event Action OnAllCharacterUnfreeze;

    // ini udah sequence ke berapa
    [SerializeField] private int currentSequence = 0;

    // fade durationnya
    [SerializeField] private float fadeDuration;

    // reference ke ui text nya
    [SerializeField] private TextMeshProUGUI firstUI;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable() {
        // subscribe ke event nya
        MainMenuManager.OnFTUEStart += StartFTUESequence;
    }

    private void OnDisable() {
        // unsubscribe dari event nya
        MainMenuManager.OnFTUEStart -= StartFTUESequence;
    }

    private void StartFTUESequence()
    {
        // mulai FTUE sequencenya
        // StartCoroutine(FTUESequence());
    }

    // private IEnumerator FTUESequence()
    // {
        
    // }

    private void OnTriggerEnter(Collider other) {
        // pas nyentuh maka langsung nambah sequence nya
        if (other.CompareTag("Obstacle") || other.CompareTag("Monster")) currentSequence++;

        // kalau obstaclenya masuk ke sini maka jangan gerak, dan all input di ignore
        OnDisableAllInput?.Invoke();

        // obstaclenya jangan gerak
        if (other.TryGetComponent(out AutoMoveObj script)) script.isMoving = false;

        // bikin all character jadi diem dulu
        OnAllCharacterFreeze?.Invoke();

        switch (currentSequence)
        {
            case 1:
                // start first sequence nya
                StartCoroutine(FirstSequence());
                break;
            case 2 :
                // StartCoroutine(SecondSequence());
                break;
            case 3 :
                // StartCoroutine(ThirdSequence());
                break;
            default:
                break;
        }
    }

    private IEnumerator FirstSequence()
    {
        // munculin ui nya
        yield return StartCoroutine(FadeTextUI(firstUI, fadeDuration, true));

        yield return StartCoroutine(FadeTextUI(firstUI, fadeDuration, false));
    }

    private IEnumerator FadeTextUI(TextMeshProUGUI text, float fadeDuration, bool inOrOut)
    {
        float elapsed = 0f;
        Color startColor = text.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(inOrOut ? 0f : 1f, inOrOut ? 1f : 0f, elapsed / fadeDuration);

            text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        text.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }
}
