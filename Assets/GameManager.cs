using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Map Settings")]
    public int baseWidth = 8;     
    public int baseHeight = 8;    
    public int currentWidth = 8;  
    public int currentHeight = 8; 
    public int maxFieldSize = 18;
    public float tileSize = 40f;
    
    [Range(0.1f, 1.0f)]
    public float wallColliderSize = 0.8f;

    [Header("Visual Settings")]
    public GameObject floorTilePrefab; 
    public Sprite wallSprite; 
    public Color floorColor1 = new Color(0.2f, 0.2f, 0.2f); 
    public Color floorColor2 = new Color(0.15f, 0.15f, 0.15f); 
    public Color borderColor = new Color(0.5f, 0.5f, 0.5f); 
    
    [Header("Camera Settings")]
    [Range(0f, 0.5f)]
    public float screenTopMargin = 0.18f; 
    public float cameraSmoothSpeed = 2.0f; 

    private GameObject floorParent;
    private GameObject wallParent;
    
    private Dictionary<Vector2Int, GameObject> wallBlockMap = new Dictionary<Vector2Int, GameObject>();

    [Header("Player Stats")]
    public int playerHP = 10;
    public int playerMaxHP = 10;
    public int playerLevel = 1;
    public int playerEXP = 0;
    public int playerAttack = 1;
    
    [Header("Speed Settings")]
    public float moveSpeed = 5.0f;      
    public float baseMoveSpeed = 5.0f;  
    private float nextSpeedUpTime = 10f; 

    public int gold = 0;
    public float survivalTime = 0f; 

    [Header("Game Mode")]
    public bool isAutoMode = false; 
    public bool isGameStarted = false; 
    public bool isGameOver = false;

    [Header("Level Up Table")]
    public List<int> levelUpExpTable = new List<int> { 100, 200, 400, 800, 1500 };

    private LevelUpCard[] currentOptionCards;

    [Header("References")]
    public UIManager uiManager; 
    public PlayerController player;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (player == null) player = FindFirstObjectByType<PlayerController>();
    }

    void Start()
    {
        Time.timeScale = 0; 
        isGameOver = false; 
        isGameStarted = false;
        survivalTime = 0f; 
        moveSpeed = baseMoveSpeed;
        
        currentWidth = baseWidth;
        currentHeight = baseHeight;

        if(floorParent == null) 
        { 
            floorParent = new GameObject("FloorHolder"); 
            floorParent.transform.SetParent(transform); 
        }
        if(wallParent == null) 
        { 
            wallParent = new GameObject("WallHolder"); 
            wallParent.transform.SetParent(transform); 
        }

        CreateFloor();
        SyncWalls(); 
        UpdateCamera(true);

        if(player != null)
        {
            float startX = (1 * tileSize) + (tileSize / 2f);
            float startY = -(1 * tileSize) - (tileSize / 2f);
            player.transform.position = new Vector3(startX, startY, 0);
            player.SnapToGrid();
        }

        UpdateUI();    
    }

    public void StartGame(bool isAuto)
    {
        isAutoMode = isAuto;
        isGameStarted = true;
        Time.timeScale = 1; 
        Debug.Log($"🚀 게임 시작!");
    }

    void Update()
    {
        if (!isGameOver && isGameStarted)
        {
            survivalTime += Time.deltaTime;
            
            if (survivalTime >= nextSpeedUpTime)
            {
                float increaseAmount = baseMoveSpeed * 0.1f;
                moveSpeed += increaseAmount; 
                nextSpeedUpTime += 10f;
            }

            if (uiManager != null) 
            {
                uiManager.UpdateTime(survivalTime);
                uiManager.UpdateStats(playerAttack, moveSpeed);
            }
            
            UpdateCamera(false);
        }
    }

    // --- 맵 관리 ---

    void CreateFloor()
    {
        if (floorTilePrefab == null) return;
        
        foreach (Transform t in floorParent.transform) Destroy(t.gameObject);

        int visualW = currentWidth + 1;
        int visualH = currentHeight + 1;

        for (int x = 0; x < visualW; x++)
        {
            for (int y = 0; y < visualH; y++)
            {
                float posX = (x * tileSize) + (tileSize / 2f);
                float posY = -(y * tileSize) - (tileSize / 2f); 

                GameObject tile = Instantiate(floorTilePrefab, new Vector3(posX, posY, 10), Quaternion.identity);
                tile.transform.SetParent(floorParent.transform);
                tile.transform.localScale = new Vector3(tileSize, tileSize, 1);

                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    bool isEven = (x + y) % 2 == 0;
                    sr.color = isEven ? floorColor1 : floorColor2;
                }
            }
        }
    }

    void SyncWalls()
    {
        HashSet<Vector2Int> targetWallCoords = new HashSet<Vector2Int>();

        int startX = -2;
        int endX = currentWidth + 2; 
        int startY = -2;              
        int endY = currentHeight + 2; 

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                bool isPlayArea = (x >= 0 && x < currentWidth && y >= 0 && y < currentHeight);
                
                if (!isPlayArea)
                {
                    bool isBorder = (x >= -2 && x < currentWidth + 2 && y >= -2 && y < currentHeight + 2);
                    if (isBorder) targetWallCoords.Add(new Vector2Int(x, y));
                }
            }
        }

        List<Vector2Int> toRemove = new List<Vector2Int>();
        foreach (var kvp in wallBlockMap)
        {
            if (!targetWallCoords.Contains(kvp.Key))
            {
                Destroy(kvp.Value); 
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove) wallBlockMap.Remove(key);

        foreach (var coord in targetWallCoords)
        {
            if (!wallBlockMap.ContainsKey(coord)) CreateSingleWallBlock(coord);
        }
    }

    void CreateSingleWallBlock(Vector2Int coord)
    {
        GameObject wall = new GameObject($"Wall_{coord.x}_{coord.y}");
        wall.transform.SetParent(wallParent.transform);
        wall.tag = "Wall";

        float posX = (coord.x * tileSize) + (tileSize / 2f);
        float posY = -(coord.y * tileSize) - (tileSize / 2f); 

        wall.transform.position = new Vector3(posX, posY, 0);

        BoxCollider2D bc = wall.AddComponent<BoxCollider2D>();
        SpriteRenderer sr = null;

        if (wallSprite != null)
        {
            sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = wallSprite;
            sr.color = borderColor;
            sr.sortingOrder = 1; 
        }

        // 스케일 자동 보정 (Auto-Fit)
        if (sr != null && sr.sprite != null)
        {
            float spriteWidth = sr.sprite.bounds.size.x;
            float scaleFactor = tileSize / spriteWidth;
            wall.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
        }
        else
        {
            wall.transform.localScale = new Vector3(tileSize, tileSize, 1);
        }
        
        bc.size = new Vector2(wallColliderSize, wallColliderSize); 

        wallBlockMap.Add(coord, wall);
    }

    // --- 카메라 ---

    void UpdateCamera(bool immediate)
    {
        Camera cam = Camera.main;
        if (cam == null) cam = FindFirstObjectByType<Camera>();
        if (cam == null) return;

        float totalExpansion = (playerLevel - 1) * 0.25f;
        int fullSteps = (int)totalExpansion;
        float remainder = totalExpansion - fullSteps;

        float virtualW = baseWidth;
        float virtualH = baseHeight;

        virtualW += (fullSteps + 1) / 2;
        virtualH += fullSteps / 2;

        if (fullSteps % 2 == 0) virtualW += remainder;
        else virtualH += remainder;

        float targetSize = CalculateTargetSize(virtualW, virtualH);

        if (immediate) cam.orthographicSize = targetSize;
        else cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * cameraSmoothSpeed);

        float mapW = virtualW * tileSize;
        float camX = mapW / 2f;
        float camY = -cam.orthographicSize * (1f - 2f * screenTopMargin);
        
        if (immediate) cam.transform.position = new Vector3(camX, camY, -10);
        else cam.transform.position = Vector3.Lerp(cam.transform.position, new Vector3(camX, camY, -10), Time.deltaTime * cameraSmoothSpeed);
    }

    float CalculateTargetSize(float w, float h)
    {
        float mapW = w * tileSize;
        float mapH = h * tileSize;
        float ratio = (float)Screen.width / (float)Screen.height;
        
        float sizeH = mapH / (2f * (1f - screenTopMargin));
        float sizeW = (mapW + 40f) / (2f * ratio);
        
        float size = Mathf.Max(sizeH, sizeW);
        return Mathf.Max(size, 5f * tileSize); 
    }

    // --- 레벨업 및 성장 ---

    void CheckLevelUp()
    {
        int requiredExp = GetRequiredExp(playerLevel);
        while (playerEXP >= requiredExp)
        {
            playerEXP -= requiredExp;
            playerLevel++;
            
            // 🔥 [추적형] 레벨업 이펙트 (플레이어를 따라다님)
            if (SoundManager.Instance) SoundManager.Instance.PlaySFX(SoundManager.Instance.levelUpClip);
            if (EffectManager.Instance && player != null) 
                EffectManager.Instance.PlayFollow(EffectManager.Instance.levelUpEffect, player.transform);

            CheckPhysicalExpansion();

            if (isAutoMode) {
                LevelUpCard[] autoCard = LevelUpCard.GetRandomCards(1);
                ApplyCardEffect(autoCard[0]);
                if (uiManager != null) uiManager.ShowNotification($"획득: {autoCard[0].title}");
            } else {
                ShowLevelUpOptions();
            }
            requiredExp = GetRequiredExp(playerLevel);
        }
    }

    void CheckPhysicalExpansion()
    {
        if (currentWidth >= maxFieldSize && currentHeight >= maxFieldSize) return;

        int totalExpansions = (playerLevel - 1) / 4;
        
        int targetWidth = baseWidth + (totalExpansions + 1) / 2;
        int targetHeight = baseHeight + totalExpansions / 2;
        
        if (targetWidth > maxFieldSize) targetWidth = maxFieldSize;
        if (targetHeight > maxFieldSize) targetHeight = maxFieldSize;

        bool changed = false;
        
        if (currentWidth != targetWidth) { currentWidth = targetWidth; changed = true; }
        if (currentHeight != targetHeight) { currentHeight = targetHeight; changed = true; }

        if (changed)
        {
            CreateFloor(); 
            SyncWalls();   
            Debug.Log($"🏟️ 구역 대개방! {currentWidth}x{currentHeight}");
            if (uiManager != null) uiManager.ShowNotification($"구역 대개방! {currentWidth}x{currentHeight}");
        }
    }

    // --- 기본 기능 ---

    public void TakeDamage(int damage) 
    { 
        if(!isGameOver)
        { 
            playerHP -= damage; 
            if(playerHP <= 0) GameOver(); 
            UpdateUI(); 
        } 
    }

    public void Heal(int amount) 
    { 
        if(!isGameOver)
        { 
            playerHP += amount; 
            if(playerHP > playerMaxHP) playerHP = playerMaxHP; 
            UpdateUI(); 
        } 
    }

    public void GainGold(int amount) 
    { 
        gold += amount; 
        UpdateUI(); 
    }

    public void GainExp(int amount) 
    { 
        playerEXP += amount; 
        CheckLevelUp(); 
        UpdateUI(); 
    }

    public int GetRequiredExp(int currentLvl) 
    { 
        int index = currentLvl - 1; 
        return (index >= 0 && index < levelUpExpTable.Count) ? levelUpExpTable[index] : 999999; 
    }

    public void GameOver() 
    { 
        isGameOver = true; 
        Debug.Log("💀 Game Over!"); 
        
        if (SoundManager.Instance) SoundManager.Instance.PlaySFX(SoundManager.Instance.gameOverClip);
        if (uiManager != null) uiManager.ShowGameOver(); 
    }

    public void RestartGame() 
    { 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }
    
    // --- 카드 관련 ---

    void ShowLevelUpOptions() 
    { 
        currentOptionCards = LevelUpCard.GetRandomCards(3); 
        if (uiManager != null) 
        { 
            uiManager.UpdateCardUI(currentOptionCards); 
            uiManager.ShowLevelUpWindow(); 
        } 
    }

    public void ApplyLevelUpCard(int index) 
    { 
        if (currentOptionCards == null || index < 0 || index >= currentOptionCards.Length) 
        { 
            if(uiManager) uiManager.HideLevelUpWindow(); 
            return; 
        } 
        ApplyCardEffect(currentOptionCards[index]); 
        if(uiManager) uiManager.HideLevelUpWindow(); 
    }

    void ApplyCardEffect(LevelUpCard card) 
    { 
        Vector3 playerPos = (player != null) ? player.transform.position : Vector3.zero;
        Color statColor = Color.yellow; 
        
        switch (card.type) 
        {
            case CardType.AttackPlus1: 
                playerAttack += 1; 
                if(uiManager) uiManager.ShowFloatingText(playerPos, "P+1", statColor); 
                break;
            case CardType.AttackPlus2: 
                playerAttack += 2; 
                if(uiManager) uiManager.ShowFloatingText(playerPos, "P+2", statColor); 
                break;
            case CardType.AttackPlus3: 
                playerAttack += 3; 
                if(uiManager) uiManager.ShowFloatingText(playerPos, "P+3", statColor); 
                break;
            case CardType.MaxHPPlus1: 
                playerMaxHP += 1; 
                Heal(1); 
                if(uiManager) { 
                    uiManager.ShowFloatingText(playerPos, "HM+1", statColor); 
                    uiManager.ShowFloatingText(playerPos, "HP+1", Color.green); 
                } 
                break;
            case CardType.MaxHPPlus2: 
                playerMaxHP += 2; 
                Heal(2); 
                if(uiManager) { 
                    uiManager.ShowFloatingText(playerPos, "HM+2", statColor); 
                    uiManager.ShowFloatingText(playerPos, "HP+2", Color.green); 
                } 
                break;
            case CardType.MaxHPPlus3: 
                playerMaxHP += 3; 
                Heal(3); 
                if(uiManager) { 
                    uiManager.ShowFloatingText(playerPos, "HM+3", statColor); 
                    uiManager.ShowFloatingText(playerPos, "HP+3", Color.green); 
                } 
                break;
            case CardType.HealPlus1: 
                Heal(1); 
                if(uiManager) uiManager.ShowFloatingText(playerPos, "HP+1", Color.green); 
                break;
            case CardType.HealPlus2: 
                Heal(2); 
                if(uiManager) uiManager.ShowFloatingText(playerPos, "HP+2", Color.green); 
                break;
            case CardType.HealPlus3: 
                Heal(3); 
                if(uiManager) uiManager.ShowFloatingText(playerPos, "HP+3", Color.green); 
                break;
        }
        UpdateUI();
    }

    void UpdateUI() 
    { 
        if (uiManager != null) 
        { 
            uiManager.UpdateHP(playerHP, playerMaxHP); 
            uiManager.UpdateGold(gold); 
            uiManager.UpdateLevel(playerLevel, playerEXP, GetRequiredExp(playerLevel)); 
            uiManager.UpdateStats(playerAttack, moveSpeed); 
        } 
    }
}