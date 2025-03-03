using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using System.Linq; // Nếu dùng TextMeshPro, nếu dùng Text thì thay bằng UnityEngine.UI.Text

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public struct WaveEnemy
    {
        public GameObject enemyPrefab; // Prefab quái
        public int count; // Số lượng quái trong wave
    }

    [System.Serializable]
    public struct WaveConfig
    {
        public List<WaveEnemy> enemies; // Danh sách quái trong wave với số lượng cố định
        public float enemiesPerSecond; // Số lượng quái spawn mỗi giây cho wave này
    }

    [Header("Wave Configuration")]
    [SerializeField] private List<WaveConfig> waves = new List<WaveConfig>(); // Danh sách các wave

    [Header("Attributes")]
    [SerializeField] private float timeBetweenWaves = 5f; // Thời gian giữa các wave

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI waveText; // Hoặc Text nếu dùng Text cũ

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new UnityEvent();

    private int currentWave = 0; // Index của wave hiện tại (bắt đầu từ 0)
    private float timeSinceLastSpawn;
    private int enemiesAlive;
    private int enemiesLeftToSpawn;
    private bool isSpawning = false;
    private List<WaveEnemy> shuffledEnemies; // Danh sách quái đã được xáo trộn để spawn xen kẽ

    private void Awake()
    {
        onEnemyDestroy.AddListener(EnemyDestroyed);
    }

    private void Start()
    {
        if (waves.Count == 0)
        {
            Debug.LogWarning("No waves configured in EnemySpawner! Adding default wave.");
            AddDefaultWave(); // Thêm wave mặc định nếu chưa có
        }
        StartCoroutine(StartWave());
        UpdateWaveText(); // Hiển thị wave ban đầu
    }

    private void Update()
    {
        if (!isSpawning) return;

        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= (1f / waves[currentWave].enemiesPerSecond) && enemiesLeftToSpawn > 0)
        {
            SpawnEnemy();
            enemiesLeftToSpawn--;
            enemiesAlive++;
            timeSinceLastSpawn = 0f;
        }

        if (enemiesAlive == 0 && enemiesLeftToSpawn == 0)
        {
            EndWave();
        }
    }

    private void EnemyDestroyed()
    {
        enemiesAlive--;
    }

    private void EndWave()
    {
        isSpawning = false;
        currentWave++;
        timeSinceLastSpawn = 0f;
        UpdateWaveText(); // Cập nhật wave sau khi kết thúc wave

        if (currentWave < waves.Count)
        {
            StartCoroutine(StartWave());
        }
        else
        {
            Debug.Log("All waves completed!");
        }
    }

    private IEnumerator StartWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        isSpawning = true;
        enemiesLeftToSpawn = CalculateEnemiesForWave(); // Tính tổng số lượng quái cho wave hiện tại

        // Xáo trộn danh sách quái để spawn ngẫu nhiên/xen kẽ
        WaveConfig currentWaveConfig = waves[currentWave];
        shuffledEnemies = new List<WaveEnemy>();
        foreach (WaveEnemy enemy in currentWaveConfig.enemies)
        {
            for (int i = 0; i < enemy.count; i++)
            {
                shuffledEnemies.Add(enemy);
            }
        }
        shuffledEnemies.Shuffle(); // Sử dụng extension method để xáo trộn
    }

    private void SpawnEnemy()
    {
        if (shuffledEnemies == null || shuffledEnemies.Count == 0)
        {
            Debug.LogWarning("No shuffled enemies available for spawn in Wave " + (currentWave + 1) + ". Using default.");
            WaveConfig currentWaveConfig = waves[currentWave];
            GameObject prefabToSpawnx = currentWaveConfig.enemies[0].enemyPrefab; // Fallback: dùng prefab đầu tiên
            Instantiate(prefabToSpawnx, LevelManager.main.StartPoint.position, Quaternion.identity);
            return;
        }

        // Lấy quái từ danh sách đã xáo trộn và spawn
        GameObject prefabToSpawn = shuffledEnemies[0].enemyPrefab;
        shuffledEnemies.RemoveAt(0); // Xóa quái đã spawn để không lặp lại

        Instantiate(prefabToSpawn, LevelManager.main.StartPoint.position, Quaternion.identity);
    }

    private int CalculateEnemiesForWave()
    {
        // Tính tổng số lượng quái trong wave hiện tại
        WaveConfig currentWaveConfig = waves[currentWave];
        return currentWaveConfig.enemies.Sum(e => e.count);
    }

    // Hàm thêm wave mặc định (dùng trong Start nếu chưa có wave)
    private void AddDefaultWave()
    {
        WaveConfig defaultWave = new WaveConfig();
        defaultWave.enemies = new List<WaveEnemy>
        {
            new WaveEnemy { enemyPrefab = null, count = 10 } // Mặc định 10 quái, cần gán prefab trong Inspector
        };
        defaultWave.enemiesPerSecond = 0.5f; // Mặc định 0.5 quái/giây
        waves.Add(defaultWave);
    }

    // Hàm cập nhật text hiển thị wave
    private void UpdateWaveText()
    {
        if (waveText != null)
        {
            waveText.text = "Wave: " + (currentWave + 1); // Hiển thị wave bắt đầu từ 1
        }
    }

    // Thêm các phương thức để chỉnh sửa wave qua Inspector
    public void AddWave()
    {
        WaveConfig newWave = new WaveConfig();
        newWave.enemies = new List<WaveEnemy>(); // Bắt đầu với danh sách rỗng
        newWave.enemiesPerSecond = 0.5f; // Mặc định 0.5 quái/giây
        waves.Add(newWave);
    }

    public void AddEnemyToWave(int waveIndex)
    {
        if (waveIndex >= 0 && waveIndex < waves.Count)
        {
            waves[waveIndex].enemies.Add(new WaveEnemy { enemyPrefab = null, count = 1 }); // Mặc định 1 quái, cần gán prefab trong Inspector
        }
    }
}

// Extension method để xáo trộn danh sách (Fisher-Yates shuffle)
public static class ListExtensions
{
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}