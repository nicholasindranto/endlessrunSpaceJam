using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SurvivalTime : MonoBehaviour
{
    // reference ke ui timernya
    public TextMeshProUGUI timerText;

    // reference ke detik sekarang
    [SerializeField] private float currentSecond;

    // Start is called before the first frame update
    void Start()
    {
        currentSecond = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        currentSecond += Time.deltaTime; // tambahin detiknya

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
