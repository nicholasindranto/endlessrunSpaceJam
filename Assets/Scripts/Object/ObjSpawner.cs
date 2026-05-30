using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjSpawner : MonoBehaviour
{
    // sisi dari lorongnya
    public enum Side
    {
        Top,
        Bottom,
        Left,
        Right
    }

    // offset dari setiap koordinatnya
    [SerializeField] private Vector3 topOffset;
    [SerializeField] private Vector3 botOffset;
    [SerializeField] private Vector3 leftOffset;
    [SerializeField] private Vector3 rightOffset;

    // timer spawn nya
    [SerializeField] private float timePassed;

    // emergence sekarang
    [SerializeField] private int currentEmergence;

    // jumlah object yang akan muncul
    [SerializeField] private int currentQuantity;

    // Start is called before the first frame update
    void Start()
    {
        SetupCurrentConfig();
    }

    // Update is called once per frame
    void Update()
    {
        // kalau belum start gamenya maka skip
        if (!MainMenuManager.instance.isAlreadyStarted) return;

        // kalau FTUE nya belum selesai maka skip juga
        if (!FTUEManager.instance.isFTUEFinished) return;

        timePassed += Time.deltaTime; // tambahin detiknya / timernya

        // kalau udah melebihi atau sama dengan maka spawn dan balikin ke 0
        if (timePassed >= currentEmergence)
        {
            timePassed = 0;
            SpawnObj();
        }
    }

    private void SpawnObj()
    {
        SetupCurrentConfig();

        GameManager.LevelConfig levelConfig = GameManager.instance.GetCurrentConfig();

        // 1. Pilih sisi-sisi yang unik (tidak ada duplikat)
        List<Side> sides = PickUniqueSides(levelConfig.quantity);

        // 2. Tentukan tipe object untuk setiap sisi
        List<ObjPool.KindOfObject> types = PickObjectTypes(sides.Count, levelConfig);

        // 3. Spawn!
        for (int i = 0; i < sides.Count; i++)
            SpawnAt(sides[i], types[i]);
    }

    // ─── PILIH SISI ─────────────────────────────────────────────────────────────

    // Menghasilkan list sisi unik yang sudah di-shuffle, sebanyak 'count'
    private List<Side> PickUniqueSides(int count)
    {
        // Semua sisi yang tersedia
        List<Side> pool = new List<Side> { Side.Top, Side.Bottom, Side.Left, Side.Right };

        // Fisher-Yates shuffle
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]); // tuple swap (C# 7+)
        }

        // Ambil sejumlah 'count' saja (max 4 karena hanya 4 sisi)
        int take = Mathf.Min(count, pool.Count);
        return pool.GetRange(0, take);
    }

    // ─── PILIH TIPE OBJECT ──────────────────────────────────────────────────────

    private List<ObjPool.KindOfObject> PickObjectTypes(int count, GameManager.LevelConfig cfg)
    {
        var types = new List<ObjPool.KindOfObject>();

        for (int i = 0; i < count; i++)
            types.Add(RollType(cfg.obstacleProb, cfg.energyProb, cfg.weaponProb));

        // ⚠️ Khusus quantity 4 (semua sisi terisi): max 3 obstacle
        // Kalau semua 4 obstacle → paksa 1 jadi non-obstacle supaya ada jalan lolos
        if (count == 4)
        {
            int obstacleCount = 0;
            foreach (var t in types)
                if (t == ObjPool.KindOfObject.Obstacle) obstacleCount++;

            if (obstacleCount == 4)
            {
                // Ubah salah satu secara random jadi energy atau weapon
                int idx  = Random.Range(0, types.Count);
                float total = cfg.energyProb + cfg.weaponProb;
                float roll  = Random.Range(0f, total);
                types[idx]  = roll < cfg.energyProb
                    ? ObjPool.KindOfObject.Energy
                    : ObjPool.KindOfObject.Weapon;
            }
        }

        return types;
    }

    // Weighted random: obstacle | energy | weapon
    private ObjPool.KindOfObject RollType(float obstacleProb, float energyProb, float weaponProb)
    {
        float roll = Random.Range(0f, 1f);

        if (roll < obstacleProb)
            return ObjPool.KindOfObject.Obstacle;
        else if (roll < obstacleProb + energyProb)
            return ObjPool.KindOfObject.Energy;
        else
            return ObjPool.KindOfObject.Weapon;
    }

    // ─── TARUH OBJECT KE SISI ───────────────────────────────────────────────────

    private void SpawnAt(Side side, ObjPool.KindOfObject type)
    {
        GameObject obj = ObjPool.instance.GetFromPool(type);
        if (obj == null) return;

        // Hitung posisi = posisi spawner + offset sisi
        Vector3 offset = side switch
        {
            Side.Top    => topOffset,
            Side.Bottom => botOffset,
            Side.Left   => leftOffset,
            Side.Right  => rightOffset,
            _           => Vector3.zero
        };

        // rotationnya
        Quaternion rotation = side switch
        {
            Side.Top => Quaternion.Euler(0f, 0f, 180f),
            Side.Bottom => Quaternion.Euler(0f, 0f, 0f),
            Side.Left => Quaternion.Euler(0f, 0f, -90f),
            Side.Right => Quaternion.Euler(0f, 0f, 90f),
            _ => Quaternion.identity
        };
// Debug.Log($"obj = {obj}, rot = {rotation}, side = {side}");
        obj.transform.localPosition = transform.localPosition + offset;
        obj.transform.localRotation = rotation;
        obj.SetActive(true);

        // Mulai gerak
        if (obj.TryGetComponent(out AutoMoveObj moveScript))
            moveScript.isMoving = true;
    }

    private void SetupCurrentConfig()
    {
        // konfigurasi dulu sekarang emergence dan quantitynya berapa
        GameManager.LevelConfig levelConfig = GameManager.instance.GetCurrentConfig();

        // assign
        currentEmergence = levelConfig.emergence;
        currentQuantity = levelConfig.quantity;
    }
}
