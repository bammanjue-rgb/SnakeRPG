# SnakeRPG 기획서 (코드/씬 기반)

본 문서는 현재 프로젝트 코드와 `Assets/Scenes/SampleScene.unity`, 프리팹 구성을 분석하여 작성한 오프라인용 기획서입니다.
기준일: 2026-02-25

---

**목차**
1. 프로젝트 개요
2. 핵심 게임 루프
3. 조작/입력
4. 맵/카메라 시스템
5. 플레이어/파티(스네이크) 시스템
6. 전투/충돌 규칙
7. 성장/레벨업 카드 시스템
8. 스폰/밸런스 시스템
9. UI/UX 구성
10. 사운드/이펙트 시스템
11. 씬 구성 요약 (SampleScene)
12. 프리팹 카탈로그
13. 데이터/수치 테이블
14. UX 플로우 차트
15. 발견된 불일치/리스크
16. 관련 파일 목록

---

**프로젝트 개요**
- 장르: 그리드 기반 스네이크 + 생존 RPG 하이브리드
- 목표: 생존 시간을 늘리며 레벨업으로 전투 능력 강화
- 핵심 재미: 이동/충돌 리스크 관리, 성장 선택, 맵 확장과 난이도 상승

---

**핵심 게임 루프**
1. 시작 화면에서 모드 선택(수동/자동)
2. 타일 그리드 이동(스네이크 방식)
3. 몬스터/음식/아군 스폰
4. 전투 및 아이템 획득 → 경험치/골드 증가
5. 레벨업 시 카드 선택(자동 모드는 자동 적용)
6. 시간 경과로 이동 속도 상승, 레벨에 따라 맵 확장
7. HP 0 또는 벽/자기 몸 충돌 시 게임 오버

---

**조작/입력**
- 키보드: 화살표 키로 방향 전환
- 모바일: `OnMobileInput("Up/Down/Left/Right")` 호출로 방향 전환
- 입력은 즉시 방향을 바꾸지만 역방향 전환은 제한됨

---

**맵/카메라 시스템**
- 타일 기반 맵 생성
- 기본 맵 크기: `baseWidth/baseHeight` (씬 기준 8x8)
- 최대 맵 크기: `maxFieldSize` (씬 기준 18x18)
- 맵 확장: 레벨 4마다 확장(가로/세로 번갈아 증가)
- 외곽 벽 자동 생성(플레이 영역 밖, -2~+2 여유 영역 포함)
- 카메라는 맵 크기에 맞춰 줌과 위치가 자동 보정됨
- 상단 UI 영역 확보를 위한 `screenTopMargin` 적용

---

**플레이어/파티(스네이크) 시스템**
- 일정 시간 간격(`moveInterval`)마다 그리드 이동
- 이동 경로를 기반으로 꼬리(파티 멤버)가 따라옴
- 파티 멤버 획득(Ally) 시 꼬리 추가
- 꼬리는 머리의 이전 위치를 타겟으로 부드럽게 이동

---

**전투/충돌 규칙**
- 충돌 타입
  - `Wall` 태그: 즉시 게임 오버
  - `Body` 태그(꼬리): 즉시 게임 오버
  - `Monster` 태그: 교환 전투 계산
- 몬스터 전투 계산
  - 필요한 공격 횟수 = `ceil(monsterHP / playerATK)`
  - 플레이어 피해 = `(hitsNeeded - 1) * monsterDamage`
  - 플레이어 HP가 피해보다 크면 승리
    - 피해 적용 후 몬스터 제거
    - 경험치 +20, 골드 +몬스터 보상
  - 그렇지 않으면 사망

---

**성장/레벨업 카드 시스템**
- 경험치가 테이블 기준치를 넘으면 레벨업
- 레벨업 시 효과
  - 레벨업 이펙트(플레이어 추적형)
  - 맵 확장 체크
  - 수동 모드: 카드 3장 제시 → 선택 적용
  - 자동 모드: 랜덤 카드 1장 자동 적용
- 카드 종류
  - 공격력 +1/+2/+3
  - 최대 HP +1/+2/+3
  - 즉시 회복 +1/+2/+3

---

**스폰/밸런스 시스템**
- 스폰 대상: 몬스터, 음식, 아군
- 스폰 위치: 무작위 타일, 플레이어와 일정 거리 이상, 충돌 없는 위치
- 스폰 간격(씬 기준)
  - 몬스터: 10초
  - 아군: 10초
  - 음식: 10초
- 몬스터 밸런싱
  - CSV 테이블 기반으로 생존 시간에 따라 HP/공격력 덮어쓰기
  - CSV가 없으면 프리팹 기본값 사용

---

**UI/UX 구성**
- HUD
  - HP 텍스트
  - Gold 텍스트
  - Level 텍스트
  - EXP 슬라이더
  - ATK/Speed 텍스트
  - 생존 시간 표시(00:00)
- 패널
  - StartScreenPanel: 수동/자동 선택 버튼
  - LevelUpPanel: 레벨업 카드 UI
  - GameOverPanel: 최종 점수/재시작
- 알림
  - NotificationText: 일시적 메시지 표시
- 플로팅 텍스트
  - 피해/회복/획득 표시

---

**사운드/이펙트 시스템**
- 사운드 매니저
  - 공격/사망/섭취/아군/레벨업/피격/게임오버 클립
  - `PlayOneShot`으로 중첩 재생
- 이펙트 매니저
  - 고정형: 공격/사망/피격/섭취/아군
  - 추적형: 레벨업
  - 애니메이터/파티클 길이로 자동 삭제

---

**씬 구성 요약 (SampleScene)**
- Root 오브젝트
  - `Main Camera`
  - `Canvas` (UIManager 포함)
  - `EventSystem`
  - `Player`
  - `GameManager`
  - `SpawnManager`
  - `SoundManager`
  - `EffectManager`
  - `DynamicWallManager`

- 매니저 설정(씬 기준)
  - GameManager
    - tileSize: 50
    - baseWidth/baseHeight: 8x8
    - maxFieldSize: 18
    - baseMoveSpeed: 2
    - playerHP/playerMaxHP: 5
    - playerAttack: 1
  - SpawnManager
    - monster/ally/food interval: 10s
    - balance CSV 연결됨
  - DynamicWallManager
    - spawnInterval: 60s
    - warningDuration: 2s
    - safeZoneRadius: 1

---

**프리팹 카탈로그**
- `Assets/Prefabs/Monster.prefab`
  - Tag: `Monster`
  - Components: SpriteRenderer, BoxCollider2D(IsTrigger), Rigidbody2D(Kinematic), Monster 스크립트, HP 텍스트 child
- `Assets/Prefabs/Ally.prefab`
  - Tag: `Ally`
  - Components: SpriteRenderer, BoxCollider2D(IsTrigger), Rigidbody2D(Kinematic)
- `Assets/Prefabs/Food.prefab`
  - Tag: `Food`
  - Components: SpriteRenderer, BoxCollider2D(IsTrigger), Rigidbody2D(Kinematic), Food 스크립트
- `Assets/Prefabs/PartyMember.prefab`
  - Tag: `Body`
  - Components: SpriteRenderer, BoxCollider2D, Rigidbody2D(Dynamic)
- `Assets/Prefabs/DynamicWall.prefab`
  - Tag: `Wall`
  - Components: SpriteRenderer, BoxCollider2D
- `Assets/Prefabs/Wall.prefab`
  - Tag: `Monster` (주의: 충돌/판정 의미 불일치 가능)
  - Components: SpriteRenderer, BoxCollider2D, Rigidbody2D
- `Assets/Prefabs/WarningIndicator.prefab`
  - Components: SpriteRenderer, WarningEffect
- `Assets/Prefabs/FloorTilePrefab.prefab`
  - Components: SpriteRenderer (sortingOrder = -10)
- `Assets/Prefabs/FloatingText.prefab`
  - Components: TextMeshPro, FloatingText 스크립트
- `Assets/Prefabs/HP_Text.prefab`
  - Components: TextMeshPro

---

**데이터/수치 테이블**
- 레벨업 경험치(코드 기준)
  - 1→2: 100
  - 2→3: 200
  - 3→4: 400
  - 4→5: 800
  - 5→6: 1500
- 이동 속도 증가
  - 10초마다 baseMoveSpeed의 10%만큼 증가
- 전투 계산
  - hitsNeeded = ceil(monsterHP / playerATK)
  - damageToPlayer = (hitsNeeded - 1) * monsterDamage

---

**UX 플로우 차트**
```
[게임 실행]
   │
   ▼
[StartScreenPanel]
   ├─(수동 선택)─▶ [게임 시작]
   └─(자동 선택)─▶ [게임 시작(자동)]
   │
   ▼
[이동/스폰/전투 루프]
   │
   ├─(아이템 획득/전투)→ EXP/Gold 증가
   │
   ├─(레벨업 조건 충족)
   │     ├─ 수동: [LevelUpPanel] 카드 선택 → 효과 적용
   │     └─ 자동: 카드 자동 적용
   │
   ├─(시간 경과) 이동 속도 상승
   ├─(레벨 상승) 맵 확장
   │
   └─(HP 0 또는 충돌) → [GameOverPanel]
            │
            └─(재시작 버튼) → 씬 재로드
```

---

**발견된 불일치/리스크**
- `Wall.prefab`가 Tag `Monster`로 설정됨
  - 충돌 판정이 `Wall` 태그 기준이므로 의도한 동작과 다를 수 있음
- `FloatingText.prefab`에 스크립트 필드 불일치 가능성
  - 프리팹 직렬화 필드와 현재 스크립트 필드 구성이 다름
- `UIManager` 카드 텍스트 배열이 씬에서 비어 있음
  - 레벨업 카드 UI에 텍스트가 반영되지 않을 수 있음
- GameManager 기본값(코드)과 씬 값이 다름
  - 코드 기본 tileSize=40, 씬 tileSize=50
  - 코드 기본 moveSpeed=5, 씬 baseMoveSpeed=2

---

**관련 파일 목록**
- `Assets/GameManager.cs`
- `Assets/PlayerController.cs`
- `Assets/SpawnManager.cs`
- `Assets/UIManager.cs`
- `Assets/Monster.cs`
- `Assets/LevelUpCard.cs`
- `Assets/FloatingText.cs`
- `Assets/Food.cs`
- `Assets/SoundManager.cs`
- `Assets/EffectManager.cs`
- `Assets/DynamicWallManager.cs`
- `Assets/WarningEffect.cs`
- `Assets/Scenes/SampleScene.unity`
- `Assets/Prefabs/*.prefab`

---

필요하면 다음 단계로 이어서 정리할 수 있습니다.
- 밸런스 수치 재정의표(CSV 포함)
- UI 레이아웃 구조도(계층 포함)
- 디자인/아트 가이드 문서(톤, 색상, 폰트 규칙)
