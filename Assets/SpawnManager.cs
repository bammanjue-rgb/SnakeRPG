using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct MonsterBalanceData
{
    public float timeThreshold; 
    public int finalHp;         // BonusHP -> FinalHP (이름은 CSV 헤더와 상관없이 로직으로 처리)
    public int finalDamage;     // BonusDamage -> FinalDamage
}

public class SpawnManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject monsterPrefab;
    public GameObject allyPrefab;
    public GameObject foodPrefab;
    
    [Header("Spawn Intervals")]
    public float monsterSpawnInterval = 5f;
    public float allySpawnInterval = 15f;
    public float foodSpawnInterval = 10f;

    [Header("Visual Settings")]
    [Range(0.1f, 1.2f)]
    public float itemSizeRatio = 0.8f; 
    
    [Header("Data Driven Balance")]
    public TextAsset balanceTableCSV; 
    
    private List<MonsterBalanceData> balanceTable = new List<MonsterBalanceData>();

    void Start()
    {
        ParseBalanceTable();

        StartCoroutine(SpawnRoutine(monsterPrefab, monsterSpawnInterval, "Monster"));
        StartCoroutine(SpawnRoutine(allyPrefab, allySpawnInterval, "Ally"));
        StartCoroutine(SpawnRoutine(foodPrefab, foodSpawnInterval, "Food"));
    }

    void ParseBalanceTable()
    {
        if (balanceTableCSV == null)
        {
            Debug.LogWarning("⚠️ [SpawnManager] CSV 파일 없음. 기본값 사용.");
            return;
        }

        string[] lines = balanceTableCSV.text.Split('\n');
        balanceTable.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            if (values.Length >= 3)
            {
                MonsterBalanceData data = new MonsterBalanceData();
                float.TryParse(values[0], out data.timeThreshold);
                int.TryParse(values[1], out data.finalHp);     // 이제 더하는게 아니라 이 값을 그대로 씁니다
                int.TryParse(values[2], out data.finalDamage); // 0이면 0이 됨
                balanceTable.Add(data);
            }
        }
        Debug.Log($"📊 밸런스 데이터 로드 완료: {balanceTable.Count}개 구간");
    }

    IEnumerator SpawnRoutine(GameObject prefab, float interval, string type)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            
            if (GameManager.Instance != null && !GameManager.Instance.isGameOver && GameManager.Instance.isGameStarted)
            {
                SpawnEntity(prefab, type);
            }
        }
    }

    void SpawnEntity(GameObject prefab, string type)
    {
        if (prefab == null) return;

        Vector3 spawnPos = GetRandomEmptyPosition();
        if (spawnPos != Vector3.zero)
        {
            GameObject newObj = Instantiate(prefab, spawnPos, Quaternion.identity);
            
            // 크기 보정 (Auto-Fit)
            if (GameManager.Instance != null)
            {
                float tileSize = GameManager.Instance.tileSize;
                SpriteRenderer sr = newObj.GetComponent<SpriteRenderer>();

                if (sr != null && sr.sprite != null)
                {
                    float targetSize = tileSize * itemSizeRatio; 
                    float spriteWidth = sr.sprite.bounds.size.x;
                    if (spriteWidth > 0)
                    {
                        float scaleFactor = targetSize / spriteWidth;
                        newObj.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
                    }
                }
                else
                {
                    newObj.transform.localScale = new Vector3(tileSize, tileSize, 1);
                }
            }

            if (type == "Monster")
            {
                ApplyMonsterScaling(newObj);
            }
        }
    }

    void ApplyMonsterScaling(GameObject monsterObj)
    {
        Monster monsterScript = monsterObj.GetComponent<Monster>();
        if (monsterScript != null && GameManager.Instance != null)
        {
            float currentTime = GameManager.Instance.survivalTime;
            
            // 기본값 (CSV가 없을 경우를 대비해 프리팹 수치 사용)
            int targetHp = monsterScript.hp;
            int targetDmg = monsterScript.damage;

            // 현재 시간에 맞는 CSV 데이터 찾기 (Absolute 방식)
            foreach (var data in balanceTable)
            {
                if (currentTime >= data.timeThreshold)
                {
                    targetHp = data.finalHp;     // 덮어쓰기
                    targetDmg = data.finalDamage; // 덮어쓰기
                }
                else
                {
                    break; 
                }
            }
            
            // 최종 적용 (0이면 진짜 0 공격력이 됨)
            // 만약 엑셀에 0이라고 적었는데 기본체력은 유지하고 싶다면 엑셀 값을 수정해야 함
            if (targetHp <= 0) targetHp = 1; // 최소 체력 안전장치

            monsterScript.SetStats(targetHp, targetDmg);
        }
    }

    Vector3 GetRandomEmptyPosition()
    {
        if (GameManager.Instance == null) return Vector3.zero;
        int maxX = GameManager.Instance.currentWidth;
        int maxY = GameManager.Instance.currentHeight;
        float tileSize = GameManager.Instance.tileSize;
        Vector3 playerPos = Vector3.zero;
        if (GameManager.Instance.player != null) playerPos = GameManager.Instance.player.transform.position;

        for (int i = 0; i < 20; i++)
        {
            int xIndex = Random.Range(0, maxX);
            int yIndex = Random.Range(0, maxY);
            float xPos = (xIndex * tileSize) + (tileSize / 2f);
            float yPos = -(yIndex * tileSize) - (tileSize / 2f);
            Vector3 candidatePos = new Vector3(xPos, yPos, 0);
            
            if (Vector3.Distance(candidatePos, playerPos) < 2.0f * tileSize) continue;
            if (!IsPositionOccupied(candidatePos)) return candidatePos;
        }
        return Vector3.zero;
    }

    bool IsPositionOccupied(Vector3 pos)
    {
        if (GameManager.Instance == null) return false;
        float tileSize = GameManager.Instance.tileSize;
        Collider2D hit = Physics2D.OverlapCircle(pos, tileSize * 0.4f);
        if (hit != null)
        {
            return true; 
        }
        return false;
    }
}
