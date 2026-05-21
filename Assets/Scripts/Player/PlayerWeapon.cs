using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviour
{
    // max inventory nya
    [Header("Inventory")]
    public int maxWeaponInventory = 1;
    [SerializeField] private int currentWeaponCount = 0;

    [Header("Attack Settings")]
    public int slowMoSpeed = 1; // speednya berubah jadi berapa
    public float attackDuration = 1.5f; // durasi sebelum balik e
    public float attackRangeInMeter = 10f; // jarak player attack si monsternya
    public float stunDuration = 5f; // durasi stun si monsternya

    [Header("Screen Shake Settings")]
    public Camera cam;
    public float shakeDuration = 0.3f; // durasi shake nya
    public float shakeMagnitude = 0.15f; // seberapa jauh kamera bakalan shake

    // event buat attack sama stunnya
    public static event Action<int> OnAttackStart; // ketika start bakalan ngeslow mo tunnel, object, dan monsternya
    public static event Action OnAttackEnd; // ketika udah berakhir maka balik ke kecepatan awal
    public static event Action<float> OnMonsterStun; // ketika ke stun, berapa lama durasinya

    // reference lagi attack apa nggak
    private bool isAttacking;

    // nama tag dari weaponnya
    public readonly string weaponTagName = "Weapon";

    // ui weaponnya
    public GameObject uiWeapon;
    public TextMeshProUGUI weaponText;
    public int showUIDuration;
    public float fadeDuration;

    // audionya
    [SerializeField] private AudioClip collectSFX;
    [SerializeField] private AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    private void OnTriggerEnter(Collider other)
    {
        // kalau jumlah weapon sekarang udah lebih dari atau sama dengan max inventory nya maka skip
        if (currentWeaponCount >= maxWeaponInventory) return;

        // pick up weaponnya
        if (other.CompareTag(weaponTagName))
        {
            // play audionya
            source.PlayOneShot(collectSFX);
Debug.Log("apakah masuk");
            currentWeaponCount++; // nambah weaponnya di inventory

            ObjPool.instance.ReturnToPool(other.gameObject, ObjPool.KindOfObject.Weapon); // balikin obj weaponnya

            // Reset alpha ke 0 dulu sebelum fade in, baru aktifkan
            SetTextAlpha(0f);
            uiWeapon.SetActive(true);

            // Kondisi dihapus — selalu jalankan coroutine saat pickup
            StartCoroutine(ShowWeaponUICoroutine());
        }
    }

    private IEnumerator ShowWeaponUICoroutine()
    {
        yield return StartCoroutine(FadeTo(1f, fadeDuration));
        yield return new WaitForSeconds(showUIDuration);
        yield return StartCoroutine(FadeTo(0f, fadeDuration));

        // Sembunyikan setelah fade out selesai
        uiWeapon.SetActive(false);
    }

    // Helper biar tidak perlu buat Color temporary di luar coroutine
    private void SetTextAlpha(float alpha)
    {
        Color c = weaponText.color;
        c.a = alpha;
        weaponText.color = c;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = weaponText.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            SetTextAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration));
            yield return null;
        }

        SetTextAlpha(targetAlpha); // pastikan nilai akhir tepat
    }

    public void UseWeapon(InputAction.CallbackContext context)
    {
        if (context.started) TryAttack(); // kalau ditekan maka coba untuk attack
    }

    private void TryAttack()
    {
        // kalau lagi attau atau nggak ada weapon maka skip
        if (isAttacking || currentWeaponCount <= 0) return;

        StartCoroutine(SlowMoAttack());
    }

    private IEnumerator SlowMoAttack()
    {
        Debug.Log("apakah masuk");
        // lagi attack
        isAttacking = true;

        // karna udah dipakai jadi weaponnya hilang
        currentWeaponCount--;

        if (currentWeaponCount == 0) uiWeapon.SetActive(false); // kalau habis ya hilangin

        // dibikin slow mo dulu tunnel, object, dan monsternya
        OnAttackStart?.Invoke(slowMoSpeed);

        // hitung jarak player ke monsternya
        float distanceInPoint = transform.position.z - GameManager.instance.monster.transform.position.z;
        float distanceInMeter = distanceInPoint / 2f;

        // kalau dalam jarak serang maka kena stun
        if (distanceInMeter <= attackRangeInMeter)
        {
            OnMonsterStun?.Invoke(stunDuration);

            StartCoroutine(StartScreenShakeEffect());
        }

        // tunggu sampai durasi slow mo nya kelar
        yield return new WaitForSeconds(attackDuration);

        // kembalikan ke speed semula
        OnAttackEnd?.Invoke();

        // udah nggak attack lagi
        isAttacking = false;
    }

    private IEnumerator StartScreenShakeEffect()
    {
        // ambil posisi kameranya
        Vector3 camPos = cam.transform.localPosition;

        // timernya
        float timePassed = 0f;

        // selama belum melebihi durasi maka screen shake
        while (timePassed < shakeDuration)
        {
            // untuk ngebikin shake pakainya random.insideUnitCircle
            Vector2 offset = UnityEngine.Random.insideUnitCircle * shakeMagnitude;

            // apply ke kameranya
            cam.transform.localPosition = new Vector3(camPos.x + offset.x, camPos.y + offset.y, camPos.z);

            timePassed += Time.deltaTime; // tambahin detiknya

            yield return null;
        }

        // setelah shake, cam balik posisi awal
        cam.transform.localPosition = camPos;
    }
}
