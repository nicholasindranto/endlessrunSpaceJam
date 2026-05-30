using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SurvivalTime : MonoBehaviour
{
    // reference ke ui timernya
    public TextMeshProUGUI timerText;

    // reference ke detik sekarang
    public float currentSecond;

    // untuk cek apakah stamina dan timer harus berhenti atau tidak
    public bool IsTimerWorking { get; private set; }

    private void OnEnable() {
        FTUESeq1.OnTimerStopAndStaminaStop += () => IsTimerWorking = false;
        FTUESeq2.OnTimerStopAndStaminaStop += () => IsTimerWorking = false;
        FTUESeq3.OnTimerStopAndStaminaStop += () => IsTimerWorking = false;
        FTUESeq4.OnTimerStopAndStaminaStop += () => IsTimerWorking = false;

        FTUESeq1.OnTimerAndStaminaResume += () => IsTimerWorking = true;
        FTUESeq2.OnTimerAndStaminaResume += () => IsTimerWorking = true;
        FTUESeq3.OnTimerAndStaminaResume += () => IsTimerWorking = true;
        FTUESeq4.OnTimerAndStaminaResume += () => IsTimerWorking = true;
    }

    private void OnDisable() {
        FTUESeq1.OnTimerStopAndStaminaStop -= () => IsTimerWorking = false;
        FTUESeq2.OnTimerStopAndStaminaStop -= () => IsTimerWorking = false;
        FTUESeq3.OnTimerStopAndStaminaStop -= () => IsTimerWorking = false;
        FTUESeq4.OnTimerStopAndStaminaStop -= () => IsTimerWorking = false;

        FTUESeq1.OnTimerAndStaminaResume -= () => IsTimerWorking = true;
        FTUESeq2.OnTimerAndStaminaResume -= () => IsTimerWorking = true;
        FTUESeq3.OnTimerAndStaminaResume -= () => IsTimerWorking = true;
        FTUESeq4.OnTimerAndStaminaResume -= () => IsTimerWorking = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentSecond = 0f;
        IsTimerWorking = true;
    }

    // Update is called once per frame
    void Update()
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) return;

        if (IsTimerWorking)
        {
            currentSecond += Time.deltaTime; // tambahin detiknya
        }

        UpdateUITimer(); // update ui timernya
    }

    private void UpdateUITimer()
    {
        // hitung menitnya dengan dibagi 60 lalu bulatkan menjadi int
        int minutes = Mathf.FloorToInt(currentSecond / 60f);

        // detiknya dengan modulo 60
        int seconds = Mathf.FloorToInt(currentSecond % 60f);

        // format menjadi format yang sesuai yaitu mm:ss dan tampilkan ke ui nya
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
