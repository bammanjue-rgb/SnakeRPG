using UnityEngine;
using System.Collections; 
using System.Collections.Generic;

public class DynamicWallManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject wallPrefab;
    public GameObject warningPrefab; 
    public float spawnInterval = 10f; 
    public float warningDuration = 2.0f; 
    public float safeZoneRadius = 3.0f; 
    
    [Range(0.1f, 1.0f)]
    public float wallColliderSize = 0.8f;

    [Header("State")]
    public int spawnCycleCount = 0; 
    public float timer = 0f;

    private List<GameObject> activeWalls = new List<GameObject>();

    void Start() { StartCoroutine(SpawnWallsRoutine()); } 

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.isGameOver || !GameManager.Instance.isGameStarted) return;
        
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            StartCoroutine(SpawnWallsRoutine());
            timer = 0f;
        }
    }

    IEnumerator SpawnWallsRoutine()
    {
        yield return null; 

        if (wallPrefab == null) yield break;

        int countToSpawn = 1 + (spawnCycleCount / 5);
        List<Vector3> targetPositions = new List<Vector3>();

        for (int i = 0; i < countToSpawn; i++)
        {
            Vector3 pos = GetRandomEmptyPosition();
            if (pos != Vector3.zero) targetPositions.Add(pos);
        }

        List<GameObject> warnings = new List<GameObject>();
        if (warningPrefab != null)
        {
            foreach (Vector3 pos in targetPositions)
            {
                GameObject w = Instantiate(warningPrefab, pos, Quaternion.identity);
                Collider2D col = w.GetComponent<Collider2D>();
                if (col != null) Destroy(col);
                warnings.Add(w);
            }
        }

        yield return new WaitForSeconds(warningDuration);

        foreach (GameObject w in warnings) if(w != null) Destroy(w);

        if (!GameManager.Instance.isGameOver)
        {
            ClearWalls(); 

            foreach (Vector3 pos in targetPositions)
            {
                // 소환 직전 한 번 더 체크 (그 사이에 플레이어나 아이템이 이동했을 수 있음)
                if (IsPositionOccupied(pos)) continue;

                GameObject newWall = Instantiate(wallPrefab, pos, Quaternion.identity);
                
                if (GameManager.Instance != null)
                {
                    float tileSize = GameManager.Instance.tileSize;
                    SpriteRenderer sr = newWall.GetComponent<SpriteRenderer>();
                    
                    if (sr != null && sr.sprite != null)
                    {
                        float spriteWidth = sr.sprite.bounds.size.x;
                        float scaleFactor = tileSize / spriteWidth;
                        newWall.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
                    }
                    else
                    {
                        newWall.transform.localScale = new Vector3(tileSize, tileSize, 1);
                    }
                    
                    BoxCollider2D bc = newWall.GetComponent<BoxCollider2D>();
                    if (bc == null) bc = newWall.AddComponent<BoxCollider2D>();
                    bc.size = new Vector2(wallColliderSize, wallColliderSize); 
                }
                
                activeWalls.Add(newWall);
            }
            Debug.Log($"🧱 [WallSpawn] 벽 {activeWalls.Count}개 생성 시도 완료");
            spawnCycleCount++;
        }
    }

    void ClearWalls()
    {
        for (int i = activeWalls.Count - 1; i >= 0; i--) if (activeWalls[i] != null) Destroy(activeWalls[i]);
        activeWalls.Clear();
    }

    Vector3 GetRandomEmptyPosition()
    {
        if (GameManager.Instance == null) return Vector3.zero;
        int maxX = GameManager.Instance.currentWidth;
        int maxY = GameManager.Instance.currentHeight;
        float tileSize = GameManager.Instance.tileSize;
        
        Vector3 playerPos = Vector3.zero;
        if (GameManager.Instance.player != null) playerPos = GameManager.Instance.player.transform.position;

        int maxAttempts = 50;

        for (int i = 0; i < maxAttempts; i++)
        {
            int xIndex = Random.Range(0, maxX);
            int yIndex = Random.Range(0, maxY);
            float xPos = (xIndex * tileSize) + (tileSize / 2f);
            float yPos = -(yIndex * tileSize) - (tileSize / 2f);
            Vector3 candidatePos = new Vector3(xPos, yPos, 0);
            
            // 1. 플레이어 안전거리 체크
            float dist = Vector3.Distance(candidatePos, playerPos);
            if (dist < safeZoneRadius * tileSize) continue; 

            // 2. 점유 상태 체크 (아군, 아이템 등)
            if (!IsPositionOccupied(candidatePos))
            {
                return candidatePos;
            }
        }
        
        return Vector3.zero;
    }

    // 🔥 [신규 기능] 해당 위치에 무언가 있는지 정밀 검사
    bool IsPositionOccupied(Vector3 pos)
    {
        if (GameManager.Instance == null) return false;
        float tileSize = GameManager.Instance.tileSize;

        // 감지 반경을 0.45f로 늘려 거의 타일 전체를 검사
        Collider2D hit = Physics2D.OverlapCircle(pos, tileSize * 0.45f);
        
        if (hit != null)
        {
            // 바닥(Floor) 타일은 무시하고, 그 외(Ally, Food, Wall, Monster)가 있으면 '점유됨'으로 판단
            // (바닥에 콜라이더가 없다면 태그 체크 안 해도 되지만, 안전을 위해)
            if (hit.CompareTag("Untagged") || hit.CompareTag("Floor")) 
            {
                return false; // 바닥만 있으면 빈 곳으로 취급
            }
            return true; // Ally, Food, Wall 등이 있으면 True
        }
        return false;
    }
}