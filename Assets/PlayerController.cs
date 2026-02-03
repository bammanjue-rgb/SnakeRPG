using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveInterval = 0.2f; 
    
    [Header("State")]
    private Vector2 direction = Vector2.right; 
    private Vector2 nextDirection = Vector2.right; 
    private float timer; 
    private Vector3 headTargetPos; 
    
    // 꼬리들의 목표 지점을 관리하는 리스트
    private List<Vector3> bodyTargetPositions = new List<Vector3>(); 
    
    [Header("Party")]
    public List<Transform> partyMembers = new List<Transform>();
    public GameObject partyMemberPrefab; 
    
    // 이동 경로 기록 (필요 시 사용)
    private List<Vector3> gridHistory = new List<Vector3>();

    void Start()
    {
        headTargetPos = transform.position;
        gridHistory.Add(transform.position);

        // [안전장치] 프리팹 확인
        if (partyMemberPrefab == null)
        {
            Debug.LogError("❌ [PlayerController] partyMemberPrefab이 비어있습니다! 인스펙터에서 프리팹을 할당해주세요.");
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.isGameOver || !GameManager.Instance.isGameStarted) return;
        if (GameManager.Instance != null && GameManager.Instance.moveSpeed > 0) moveInterval = 1f / GameManager.Instance.moveSpeed;

        HandleInput();

        timer += Time.deltaTime;
        if (timer >= moveInterval)
        {
            if (TryUpdateTargets()) { }
            timer = 0;
        }
        SmoothMove();
    }
    
    public void SnapToGrid()
    {
        if (GameManager.Instance == null) return;
        float tileSize = GameManager.Instance.tileSize;
        
        float snappedX = Mathf.Floor(transform.position.x / tileSize) * tileSize + (tileSize / 2f);
        float snappedY = Mathf.Ceil(transform.position.y / tileSize) * tileSize - (tileSize / 2f); 
        
        Vector3 newPos = new Vector3(snappedX, snappedY, 0);
        transform.position = newPos;
        headTargetPos = newPos;
    }

    void HandleInput() { if (Input.GetKeyDown(KeyCode.UpArrow)) SetDirection(Vector2.up); else if (Input.GetKeyDown(KeyCode.DownArrow)) SetDirection(Vector2.down); else if (Input.GetKeyDown(KeyCode.LeftArrow)) SetDirection(Vector2.left); else if (Input.GetKeyDown(KeyCode.RightArrow)) SetDirection(Vector2.right); }
    public void SetDirection(Vector2 dir) { if (dir == Vector2.up && direction != Vector2.down) nextDirection = Vector2.up; else if (dir == Vector2.down && direction != Vector2.up) nextDirection = Vector2.down; else if (dir == Vector2.left && direction != Vector2.right) nextDirection = Vector2.left; else if (dir == Vector2.right && direction != Vector2.left) nextDirection = Vector2.right; }
    public void OnMobileInput(string dirName) { if (dirName == "Up") SetDirection(Vector2.up); else if (dirName == "Down") SetDirection(Vector2.down); else if (dirName == "Left") SetDirection(Vector2.left); else if (dirName == "Right") SetDirection(Vector2.right); }

    bool TryUpdateTargets()
    {
        direction = nextDirection;
        float tileSize = GameManager.Instance.tileSize;
        Vector3 potentialPos = headTargetPos + (Vector3)(direction * tileSize);

        Collider2D hit = Physics2D.OverlapCircle(potentialPos, tileSize * 0.05f);

        if (hit != null)
        {
            if (hit.CompareTag("Wall")) 
            { 
                Debug.Log("💀 벽 충돌!"); 
                if (GameManager.Instance != null) GameManager.Instance.GameOver(); 
                return false; 
            }
            else if (hit.CompareTag("Body")) 
            { 
                Debug.Log("💀 꼬리 충돌!"); 
                if (GameManager.Instance != null) GameManager.Instance.GameOver(); 
                return false; 
            }
            else if (hit.CompareTag("Monster"))
            {
                if (GameManager.Instance != null)
                {
                    Monster monsterScript = hit.GetComponent<Monster>();
                    if (monsterScript != null)
                    {
                        if (SoundManager.Instance) SoundManager.Instance.PlaySFX(SoundManager.Instance.attackClip);
                        if (EffectManager.Instance) EffectManager.Instance.PlayFixed(EffectManager.Instance.attackEffect, potentialPos);

                        int myAtk = GameManager.Instance.playerAttack; if (myAtk < 1) myAtk = 1;
                        int monsterHp = monsterScript.hp; int monsterDmg = monsterScript.damage;
                        int hitsNeeded = Mathf.CeilToInt((float)monsterHp / myAtk);
                        int damageToPlayer = (hitsNeeded - 1) * monsterDmg; if (damageToPlayer < 0) damageToPlayer = 0;

                        if (GameManager.Instance.playerHP > damageToPlayer)
                        {
                            if (damageToPlayer > 0) 
                            { 
                                GameManager.Instance.TakeDamage(damageToPlayer); 
                                if(UIManager.Instance) UIManager.Instance.ShowFloatingText(transform.position, $"HP-{damageToPlayer}", Color.red); 
                                
                                if (SoundManager.Instance) SoundManager.Instance.PlaySFX(SoundManager.Instance.hitClip);
                                if (EffectManager.Instance) EffectManager.Instance.PlayFixed(EffectManager.Instance.hitEffect, transform.position);
                            }
                            
                            GameManager.Instance.GainExp(20); 
                            GameManager.Instance.GainGold(monsterScript.goldReward);
                            
                            if (SoundManager.Instance) SoundManager.Instance.PlaySFX(SoundManager.Instance.dieClip);
                            if (EffectManager.Instance) EffectManager.Instance.PlayFixed(EffectManager.Instance.dieEffect, potentialPos);

                            Destroy(hit.gameObject); 
                            Debug.Log($"⚔️ 승리!");
                        }
                        else 
                        { 
                            GameManager.Instance.TakeDamage(damageToPlayer); 
                            Debug.Log("💀 패배..."); 
                            return false; 
                        }
                    }
                }
                else return false; 
            }
        }
        
        // 꼬리 이동 로직 업데이트
        if (partyMembers.Count > 0)
        {
            // 리스트 크기 동기화 (AddPartyMember와 타이밍 이슈 방지)
            while (bodyTargetPositions.Count < partyMembers.Count) 
            {
                // 데이터가 모자라면 마지막 꼬리 위치 혹은 머리 위치로 채움 (0,0,0 방지)
                if(bodyTargetPositions.Count > 0) bodyTargetPositions.Add(bodyTargetPositions[bodyTargetPositions.Count-1]);
                else bodyTargetPositions.Add(headTargetPos);
            }

            // 꼬리 위치 당기기 (뒤에서부터 앞의 위치를 가져옴)
            for (int i = partyMembers.Count - 1; i > 0; i--) 
                bodyTargetPositions[i] = bodyTargetPositions[i - 1];
            
            // 첫 번째 꼬리는 머리가 있던 위치로 이동
            bodyTargetPositions[0] = headTargetPos;
        }

        headTargetPos = potentialPos;
        gridHistory.Insert(0, headTargetPos);
        if (gridHistory.Count > partyMembers.Count + 10) gridHistory.RemoveAt(gridHistory.Count - 1);
        return true;
    }

    void SmoothMove()
    {
        float tileSize = GameManager.Instance.tileSize;
        float speed = tileSize / moveInterval; 
        transform.position = Vector3.MoveTowards(transform.position, headTargetPos, speed * Time.deltaTime);
        
        // 꼬리 부드러운 이동
        for (int i = 0; i < partyMembers.Count; i++) 
        {
            if (partyMembers[i] != null && i < bodyTargetPositions.Count) 
            {
                partyMembers[i].position = Vector3.MoveTowards(partyMembers[i].position, bodyTargetPositions[i], speed * Time.deltaTime);
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (GameManager.Instance == null) return;
        
        if (other.CompareTag("Food")) 
        { 
            GameManager.Instance.Heal(1); 
            GameManager.Instance.GainGold(10); 
            
            if(UIManager.Instance) { 
                UIManager.Instance.ShowFloatingText(transform.position, "HP+1", Color.green); 
                UIManager.Instance.ShowFloatingText(transform.position, "G+10", Color.yellow); 
            }
            
            if (SoundManager.Instance) SoundManager.Instance.PlaySFX(SoundManager.Instance.eatClip);
            if (EffectManager.Instance) EffectManager.Instance.PlayFixed(EffectManager.Instance.eatEffect, other.transform.position);

            Destroy(other.gameObject); 
        }
        else if (other.CompareTag("Ally")) 
        { 
            AddPartyMember(); // 꼬리 추가 함수 호출
            GameManager.Instance.GainExp(10); 
            
            if(UIManager.Instance) UIManager.Instance.ShowFloatingText(transform.position, "EXP+10", Color.cyan); 
            
            if (SoundManager.Instance) SoundManager.Instance.PlaySFX(SoundManager.Instance.allyClip);
            if (EffectManager.Instance) EffectManager.Instance.PlayFixed(EffectManager.Instance.allyEffect, other.transform.position);

            Destroy(other.gameObject); 
        }
    }

    // [수정됨] 꼬리 추가 로직 안정화
    public void AddPartyMember()
    {
        if (partyMemberPrefab == null)
        {
            Debug.LogError("❌ [AddPartyMember] partyMemberPrefab이 할당되지 않았습니다! 인스펙터를 확인하세요.");
            return;
        }

        // 생성 위치 결정: 꼬리가 있다면 마지막 꼬리 위치, 없다면 머리 위치
        Vector3 spawnPos = transform.position;
        if (partyMembers.Count > 0)
        {
            spawnPos = partyMembers[partyMembers.Count - 1].position;
            // 만약 목표 위치 리스트가 있다면 더 정확한 위치인 '마지막 꼬리의 목표 지점' 사용
            if (bodyTargetPositions.Count > 0) 
                spawnPos = bodyTargetPositions[bodyTargetPositions.Count - 1];
        }
        else
        {
            spawnPos = headTargetPos; // 꼬리가 없을 땐 머리가 가고 있는/있던 곳
        }

        // 꼬리 생성
        GameObject newBody = Instantiate(partyMemberPrefab, spawnPos, Quaternion.identity);
        
        // 리스트에 등록
        partyMembers.Add(newBody.transform);
        
        // **중요**: 새로 생긴 꼬리의 목표 위치도 현재 생성 위치로 설정 (다음 틱까지 대기)
        bodyTargetPositions.Add(spawnPos); 
    }
}