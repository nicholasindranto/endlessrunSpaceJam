using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // singleton
    public static GameManager instance;

    // reference ke timer nya
    public SurvivalTime timeScript;

    // udah game over apa belum
    private bool isGameOver;

    public GameObject gameOverUI;

    // reference ke playernya
    public GameObject player;

    // reference ke monsternya juga
    public GameObject monster;

    [System.Serializable]
    public struct LevelConfig //konfigurasi levelnya
    {
        public int emergence;       // interval spawn dalam detik
        public int quantity;        // jumlah object yang spawn sekaligus
        public float obstacleProb;
        public float energyProb;
        public float weaponProb;
    }

    [Header("Level Configs (per menit)")]
    public LevelConfig lvl1;   // 0–1 menit
    public LevelConfig lvl2;   // 1–2 menit
    public LevelConfig lvl3;   // 2–3 menit
    public LevelConfig lvl4;   // 3–4 menit
    public LevelConfig lvl5;   // 4–5 menit
    public LevelConfig lvl6;   // 5–6 menit
    public LevelConfig lvl7;   // 6–7 menit
    public LevelConfig lvlEndless; // 7+ menit

    // Ambil config sesuai waktu sekarang
    public LevelConfig GetCurrentConfig()
    {
        float s = timeScript.currentSecond; // ambil detik sekarang
        if      (s >= 420f) return lvlEndless;
        else if (s >= 360f) return lvl7;
        else if (s >= 300f) return lvl6;
        else if (s >= 240f) return lvl5;
        else if (s >= 180f) return lvl4;
        else if (s >= 120f) return lvl3;
        else if (s >= 60f)  return lvl2;
        else                return lvl1;
    }

    private void Awake() {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject);
        } else Destroy(gameObject);
    }

    private void OnEnable() {
        MonsterChase.OnPlayerCaught += OnPlayerCaughtAndHitObstacle;
        PlayerDie.OnHitObstacle += OnPlayerCaughtAndHitObstacle;
    }

    private void OnDisable() {
        MonsterChase.OnPlayerCaught -= OnPlayerCaughtAndHitObstacle;
        PlayerDie.OnHitObstacle -= OnPlayerCaughtAndHitObstacle;
    }

    private void OnPlayerCaughtAndHitObstacle()
    {
        // kalau game over maka skip
        if (isGameOver) return;

        isGameOver = true;

        // berhentiin gamenya
        Time.timeScale = 0;

        // munculin ui game overnya
        gameOverUI.SetActive(true);
    }

    public void RestartGame(InputAction.CallbackContext context)
    {
        // kalau belum game over ya jangan
        if (!isGameOver) return;

        if (context.started) Restart();
    }

    public void Restart()
    {
        Debug.Log("masuk ke restart");
        // kalau ditekan maka game di jalankan lagi dan masuk ke scene ini lagi
        Time.timeScale = 1;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
