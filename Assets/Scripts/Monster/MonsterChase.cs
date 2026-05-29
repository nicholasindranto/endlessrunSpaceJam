using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MonsterChase : MonoBehaviour
{
    [Header("Chase Setting")]
    // monster nya startnya dimana
    public float monsterStartDistance;

    // threshold player ketangkep
    public float caughtThreshold = 7f;

    // pergerakan monster per detik
    public float baseMonsterMovementPerSecond = 0.5f; // default
    // 3 jenis movement speed based on player speed
    private float movementSpeedWhenPlayerUnder50 = 1.5f;
    private float movementSpeedWhenPlayerUnder25 = 4f;

    // reference ke current speednya
    private float currentMonsterMovementPerSecond;

    [Header("Distance UI")]
    // reference ke ui tulisan jaraknya
    public TextMeshProUGUI distanceUI;

    // threshold ui distance nya
    public float redDistanceInMeter = 15f;
    public float yellowDistanceInMeter = 25f;

    // timernya
    private float timePassed;

    // apakah udah ketangkep
    private bool isCaught;

    // warna tulisan uinya
    private static readonly Color green = Color.green;
    private static readonly Color red = Color.red;
    private static readonly Color yellow = Color.yellow;

    public static event Action OnPlayerCaught;

    // stun systemnya
    // apakah lagi ke stun
    private bool isStunned;
    // apakah lagi chasing
    private bool isChasingPaused;

    // reference ke monster speed yang sekarang
    private float speedBeforeStunned;

    // reference ke animator
    [SerializeField] private Animator anim;

    // nama param animasi chasenya
    public readonly string chaseParam = "IsChasing";

    // ─── Tambah fields ini ───────────────────────────────────────────────────────
    private bool  isSprintRetreating;
    private float sprintRetreatPerSec;

    // reference ke audio roar dan hurt nya
    [SerializeField] private AudioClip roar, hurt;
    [SerializeField] private AudioSource source;
    [SerializeField] private float minEmergenceRoarSFX;
    [SerializeField] private float maxEmergenceRoarSFX;

    // simpan dulu kecepatannya sebelum freeze
    private float speedBeforeFreeze;

    private void Awake() {
        currentMonsterMovementPerSecond = baseMonsterMovementPerSecond;
    }

    // Start is called before the first frame update
    void Start()
    {
        // posisikan si monster di belakang player dengan dikurangi z nya sebesar distancenya
        transform.position = new Vector3(transform.position.x, transform.position.y, GameManager.instance.player.transform.position.z - monsterStartDistance);

        // di update langsung
        UpdateDistanceUI();

        // bikin suara roar nya
        StartCoroutine(RoarSFXCoroutine());
    }

    private IEnumerator RoarSFXCoroutine()
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) yield return null;

        while (!isCaught) // ← berhenti otomatis saat game over
        {
            // null check biar tidak crash kalau lupa assign di Inspector
            if (source != null && roar != null)
                source.PlayOneShot(roar);

            // float overload → hasil benar-benar acak, bukan hanya bilangan bulat
            float waitTime = UnityEngine.Random.Range(minEmergenceRoarSFX, maxEmergenceRoarSFX);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void OnEnable() {
        PlayerStamina.OnStaminaTierChange += HandleStaminaTierChange;

        PlayerWeapon.OnAttackStart += HandleAttackStart;

        PlayerWeapon.OnAttackEnd += HandleAttackEnd;

        PlayerWeapon.OnMonsterStun += HandleOnStunned;

        // ─── Di OnEnable / OnDisable ─────────────────────────────────────────────────
        PlayerSprint.OnSprintStart += HandleSprintStart;
        PlayerSprint.OnSprintEnd   += HandleSprintEnd;

        // subscribe pas characternya freeze
        FTUEGameplaySequence.OnAllCharacterFreeze += HandleMonsterFreeze;
        FTUEGameplaySequence.OnAllCharacterUnfreeze += HandleMonsterUnfreeze;
    }

    private void OnDisable() {
        PlayerStamina.OnStaminaTierChange -= HandleStaminaTierChange;

        PlayerWeapon.OnAttackStart -= HandleAttackStart;

        PlayerWeapon.OnAttackEnd -= HandleAttackEnd;

        PlayerWeapon.OnMonsterStun -= HandleOnStunned;

        // ─── Di OnDisable ────────────────────────────────────────────────────────────
        PlayerSprint.OnSprintStart -= HandleSprintStart;
        PlayerSprint.OnSprintEnd   -= HandleSprintEnd;

        // unsubscribe pas characternya freeze
        FTUEGameplaySequence.OnAllCharacterFreeze -= HandleMonsterFreeze;
        FTUEGameplaySequence.OnAllCharacterUnfreeze -= HandleMonsterUnfreeze;
    }

    private void HandleMonsterFreeze()
    {
        // simpan dulu kecepatan sebelum freeze
        speedBeforeFreeze = currentMonsterMovementPerSecond;

        // kalau freeze maka speednya 0
        currentMonsterMovementPerSecond = 0;

        // nggak chasing lagi
        isChasingPaused = true;

        // matikin animasinya
        anim.SetBool(chaseParam, false);
    }

    private void HandleMonsterUnfreeze()
    {
        // kalau unfreeze maka balik ke speed normal
        currentMonsterMovementPerSecond = speedBeforeFreeze;

        // bisa chase lagi
        isChasingPaused = false;

        // nyalain lagi animasinya
        anim.SetBool(chaseParam, true);
    }

    // ─── Handlers ────────────────────────────────────────────────────────────────
    private void HandleSprintStart(int _, float retreatPerSec)
    {
        sprintRetreatPerSec = retreatPerSec;
        isSprintRetreating  = true;
    }

    private void HandleSprintEnd()
    {
        isSprintRetreating = false;
    }

    private void HandleAttackStart(int _)
    {
        // simpan dulu kecepatan awalnya
        speedBeforeStunned = currentMonsterMovementPerSecond;

        // ubah kecepatan jadi 0
        currentMonsterMovementPerSecond = 0f;

        // nggak chasing lagi
        isChasingPaused = true;
    }

    private void HandleAttackEnd()
    {
        // kalau lagi ke stun maka skip
        if (isStunned) return;

        // balikin ke kecepatan sebelumnya
        currentMonsterMovementPerSecond = speedBeforeStunned;

        // bisa chase lagi
        isChasingPaused = false;
    }

    private void HandleOnStunned(float stunDuration)
    {
        // start coroutine untuk stun nya
        StartCoroutine(StunCoroutine(stunDuration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        if (source != null && hurt != null)
        source.PlayOneShot(hurt); // ← hurt play saat monster kena attack

        // ke stun
        isStunned = true;

        // matikin animasinya
        anim.SetBool(chaseParam, false);

        // tunggu durasinya
        yield return new WaitForSeconds(duration);

        // balikin kondisinya karna udah selesai
        isStunned = false;
        isChasingPaused = false;
        currentMonsterMovementPerSecond = speedBeforeStunned;

        anim.SetBool(chaseParam, true);
    }

    private void HandleStaminaTierChange(PlayerStamina.StaminaTier staminaTier)
    {
        // set langsung ke currrent speed nya
        currentMonsterMovementPerSecond = staminaTier switch
        {
            PlayerStamina.StaminaTier.Normal => baseMonsterMovementPerSecond,
            PlayerStamina.StaminaTier.Below50 => movementSpeedWhenPlayerUnder50,
            PlayerStamina.StaminaTier.Below25 => movementSpeedWhenPlayerUnder25,
            _ => baseMonsterMovementPerSecond
        };
    }

    // Update is called once per frame
    void Update()
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) return;

        // kalau udah ketangkep maka skip
        if (isCaught) return;

        // cek apakah ketangkep
        CheckCaught();

        // kalau misalkan di stun, maka instead of maju, monsternya mundur
        if (isStunned) RetreatMonster();
        if (isSprintRetreating) SprintRetreat();
        if (!isStunned && !isSprintRetreating && !isChasingPaused) ChasePlayer(); // kalau nggak di pause ya ngejar

        timePassed += Time.deltaTime;
        // kalau udah melewati 1 detik maka distance berkurang lalu reset balik ke 0
        if (timePassed >= 1f)
        {
            timePassed -= 1f; // kurangin lagi biar balik

            // update uinya juga
            UpdateDistanceUI();
        }
    }

    // ─── Tambah method ini ───────────────────────────────────────────────────────
    private void SprintRetreat()
    {
        // mundur sejauh sprintRetreatPerSec per detik × deltaTime = smooth
        transform.Translate(Vector3.back * sprintRetreatPerSec * Time.deltaTime, Space.World);
    }

    private void RetreatMonster()
    {
        // mundurin si monster sesuai kecepatan si playernya
        if (GameManager.instance.player.TryGetComponent(out PlayerSpeed script))
            transform.Translate(Vector3.back * script.currentSpeed * Time.deltaTime, Space.World);
    }

    private void ChasePlayer()
    {
        // tambahin posisi si monsternya
        // transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + currentMonsterMovementPerSecond);
        transform.Translate(Vector3.forward * currentMonsterMovementPerSecond * Time.deltaTime, Space.World);
    }

    private void UpdateDistanceUI()
    {
        // kalkulasi distance sekarang (masih poin unity)
        float distanceInPoint = Mathf.Max(0f, GameManager.instance.player.transform.position.z - transform.position.z); // pake max biar nggak negatif

        // ubah menjadi meter
        int distanceInMeter = Mathf.FloorToInt(distanceInPoint / 2f);

        // tampilkan ke uinya
        distanceUI.text = $"{distanceInMeter} m";

        // ubah warnanya juga
        if (distanceInMeter <= redDistanceInMeter) distanceUI.color = red;
        else if (distanceInMeter <= yellowDistanceInMeter) distanceUI.color = yellow;
        else distanceUI.color = green;
    }

    private void CheckCaught()
    {
        // itung jaraknya dulu
        float distanceInPoint = GameManager.instance.player.transform.position.z - transform.position.z;

        if (distanceInPoint <= caughtThreshold)
        {
            isCaught = true;

            OnPlayerCaught?.Invoke();
        }
    }

    public void ChangeMonsterSpeed(int percentageBelow)
    {
        switch (percentageBelow)
        {
            case 50:
                currentMonsterMovementPerSecond -= movementSpeedWhenPlayerUnder50;
                break;
            case 25:
                currentMonsterMovementPerSecond -= movementSpeedWhenPlayerUnder25;
                break;
            default:
                Debug.Log("nggak ada");
                break;
        }
    }
}
