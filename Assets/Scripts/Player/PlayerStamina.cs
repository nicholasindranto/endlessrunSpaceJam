using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    // stamina tier, sekarang posisinya dimana
    public enum StaminaTier
    {
        Normal,
        Below50,
        Below25
    }

    // stamina setting
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float drainPerSecond = 2f;
    public float energyPickUpAmount = 10f;

    [Header("Stamina Bar UI")]
    // reference ke stamina bar nya
    public Image staminaBar;

    // event biar bisa memanggil seluruh fungsi kalau tier staminanya berubah
    public static event Action<StaminaTier> OnStaminaTierChange;

    // reference ke tier sekarang
    private StaminaTier currentTier = StaminaTier.Normal;
    // Tambah static property biar bisa dicek dari luar tanpa referensi
    public static StaminaTier CurrentTier { get; private set; }
    // reference ke waktu udah berlalu
    private float timePassed;

    // reference ke warna bar uinya
    private static readonly Color green = Color.green;
    private static readonly Color red = Color.red;
    private static readonly Color yellow = Color.yellow;

    // reference nama tag dari energy
    private readonly string energyTagName = "Energy";

    // audionya
    [SerializeField] private AudioClip collectSFX;
    [SerializeField] private AudioSource source;

    public bool IsReducing { private set; get; }

    private void OnEnable() {
        FTUESeq1.OnTimerStopAndStaminaStop += () => IsReducing = false;
        FTUESeq2.OnTimerStopAndStaminaStop += () => IsReducing = false;
        FTUESeq3.OnTimerStopAndStaminaStop += () => IsReducing = false;
        FTUESeq4.OnTimerStopAndStaminaStop += () => IsReducing = false;
        
        FTUESeq1.OnTimerAndStaminaResume += () => IsReducing = true;
        FTUESeq2.OnTimerAndStaminaResume += () => IsReducing = true;
        FTUESeq3.OnTimerAndStaminaResume += () => IsReducing = true;
        FTUESeq4.OnTimerAndStaminaResume += () => IsReducing = true;
    }

    private void OnDisable() {
        FTUESeq1.OnTimerStopAndStaminaStop -= () => IsReducing = false;
        FTUESeq2.OnTimerStopAndStaminaStop -= () => IsReducing = false;
        FTUESeq3.OnTimerStopAndStaminaStop -= () => IsReducing = false;
        FTUESeq4.OnTimerStopAndStaminaStop -= () => IsReducing = false;
        
        FTUESeq1.OnTimerAndStaminaResume -= () => IsReducing = true;
        FTUESeq2.OnTimerAndStaminaResume -= () => IsReducing = true;
        FTUESeq3.OnTimerAndStaminaResume -= () => IsReducing = true;
        FTUESeq4.OnTimerAndStaminaResume -= () => IsReducing = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        // setup awal stamina nya full
        currentStamina = maxStamina;

        // set ke normal dulu dong di awal
        CurrentTier = StaminaTier.Normal;

        IsReducing = true;

        UpdateStaminaBarUI();
    }

    // Update is called once per frame
    void Update()
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) return;

        timePassed += Time.deltaTime;

        if(timePassed >= 1f)
        {
            timePassed -= 1f;

            if (IsReducing)
            {
                DrainStamina();
            }
        }
    }

    // Tambah method ini — dipanggil PlayerSprint saat sprint dimulai
    public void ReduceStamina(float amount)
    {
        currentStamina = Mathf.Max(0f, currentStamina - amount);
        UpdateStaminaBarUI();
        CheckStaminaTier();
    }

    private void UpdateStaminaBarUI()
    {
        // ambil percentage staminanya
        float fill = currentStamina / maxStamina;
        staminaBar.fillAmount = fill;

        // ubah warnanya juga
        float percentage = fill * 100f;
        if (percentage > 50f) staminaBar.color = green;
        else if (percentage > 25f) staminaBar.color = yellow;
        else staminaBar.color = red;
    }

    private void DrainStamina()
    {
        // kurangin tapi jangan sampai negatif
        currentStamina = Mathf.Max(0f, currentStamina - drainPerSecond);

        UpdateStaminaBarUI();

        // pas udah dikurangin, check tiernya apakah turun
        CheckStaminaTier();
    }

    private void CheckStaminaTier()
    {
        // bikin dulu percentagenya
        float percentage = (currentStamina / maxStamina) * 100;

        // cek tiernya lalu masukin ke newTiernya
        StaminaTier newTier;
        if (percentage > 50f) newTier = StaminaTier.Normal;
        else if (percentage > 25f) newTier = StaminaTier.Below50;
        else newTier = StaminaTier.Below25;

        // kalau newtier dan tier sekarang sama, maka nggak ada perubahan jadi skip
        if (newTier == currentTier) return;

        currentTier = newTier; // update tiernya

        CurrentTier = newTier; // ← update static property juga

        // invoke eveny nya
        OnStaminaTierChange?.Invoke(currentTier);
    }

    private void AddStamina(float amount)
    {
        // tambahin tapi jangan melebihi max nya, pakai min
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);

        UpdateStaminaBarUI();

        // pas udah dikurangin, check tiernya apakah turun
        CheckStaminaTier();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(energyTagName))
        {
            // play audionya
            source.PlayOneShot(collectSFX);

            AddStamina(energyPickUpAmount); // kalau dapat energy, tambah stamina

            // balikin ke pool obj nya
            ObjPool.instance.ReturnToPool(other.gameObject, ObjPool.KindOfObject.Energy);
        }
    }
}
