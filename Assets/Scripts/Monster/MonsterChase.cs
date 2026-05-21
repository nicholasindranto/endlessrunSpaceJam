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
    public float monsterStartDistance = 100f;

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

    private void Awake() {
        currentMonsterMovementPerSecond = baseMonsterMovementPerSecond;
    }

    // Start is called before the first frame update
    void Start()
    {
        // posisikan si monster di belakang player dengan dikurangi z nya sebesar distancenya
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - monsterStartDistance);

        // di update langsung
        UpdateDistanceUI();
    }

    private void OnEnable() {
        PlayerStamina.OnStaminaTierChange += HandleStaminaTierChange;

        PlayerWeapon.OnAttackStart += HandleAttackStart;

        PlayerWeapon.OnAttackEnd += HandleAttackEnd;

        PlayerWeapon.OnMonsterStun += HandleOnStunned;

        // ─── Di OnEnable / OnDisable ─────────────────────────────────────────────────
        PlayerSprint.OnSprintStart += HandleSprintStart;
        PlayerSprint.OnSprintEnd   += HandleSprintEnd;
    }

    private void OnDisable() {
        PlayerStamina.OnStaminaTierChange -= HandleStaminaTierChange;

        PlayerWeapon.OnAttackStart -= HandleAttackStart;

        PlayerWeapon.OnAttackEnd -= HandleAttackEnd;

        PlayerWeapon.OnMonsterStun -= HandleOnStunned;

        // ─── Di OnDisable ────────────────────────────────────────────────────────────
        PlayerSprint.OnSprintStart -= HandleSprintStart;
        PlayerSprint.OnSprintEnd   -= HandleSprintEnd;
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
