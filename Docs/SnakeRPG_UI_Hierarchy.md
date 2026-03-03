# SnakeRPG UI 레이아웃 구조도 (SampleScene 기반)

본 문서는 `Assets/Scenes/SampleScene.unity`와 `UIManager` 레퍼런스를 기준으로 정리한 UI 구조 요약입니다.
정확한 트리 구조(모든 RectTransform)는 씬에서 확인하는 것을 권장합니다.

---

**Canvas (Root)**
- UIManager (MonoBehaviour)
- 주요 하위 그룹

**1) HUD 그룹**
- 목적: 플레이 중 상시 표시
- 주요 요소(스크립트 레퍼런스 기준)
  - `HP_Text` (TMP Text) → `hpText`
  - `GoldText` (TMP Text) → `goldText`
  - `LevelText` (TMP Text) → `levelText`
  - `EXPSlider` (Slider) → `expSlider`
  - `AttackText` (TMP Text) → `attackText`
  - `SpeedText` (TMP Text) → `speedText`
  - `SurvivalTimeText` (TMP Text) → `survivalTimeText`

**2) StartScreenPanel**
- 목적: 모드 선택(수동/자동)
- 포함 요소(버튼 기반)
  - `Button_Auto` → `UIManager.OnModeSelectBtnClick(true)`
  - `Button_Manual` (유사 버튼 구조)

**3) LevelUpPanel**
- 목적: 레벨업 카드 선택
- 포함 요소(카드 3개 구성)
  - CardTitle1/2/3 (TMP Text)
  - CardDescription1/2/3 (TMP Text)
  - 각 카드 버튼 클릭 시 `UIManager.OnCardSelect(index)`
- 현재 `UIManager`의 `cardTitles`, `cardDescriptions` 배열이 씬에서 비어있음(설정 필요)

**4) GameOverPanel**
- 목적: 게임 오버 결과 표시 및 재시작
- 포함 요소
  - `FinalScoreText`
  - `FinalGoldText`
  - Restart 버튼 → `UIManager.OnRestartBtnClick()`

**5) NotificationText**
- 목적: 일시적 안내 메시지 표시
- `UIManager.ShowNotification()`으로 사용

---

**UIManager 레퍼런스 연결 요약**
- `hpText` → HP 표시
- `goldText` → Gold 표시
- `levelText` → 레벨 표시
- `expSlider` → 경험치 바
- `attackText` → 공격력 표시
- `speedText` → 이동속도 표시
- `survivalTimeText` → 생존 시간 표시
- `gameOverPanel` → GameOverPanel
- `levelUpWindow` → LevelUpPanel
- `startScreenPanel` → StartScreenPanel
- `notificationText` → NotificationText
- `floatingTextPrefab` → 플로팅 텍스트 프리팹

---

**확인/정리 권장 사항**
- `cardTitles`, `cardDescriptions` 배열을 씬에서 연결
- HUD 그룹 내 RectTransform 정렬 통일(앵커/피벗 기준)
- 모바일 입력 버튼(Up/Down/Left/Right)이 존재한다면 StartScreen 이후 활성/비활성 상태 확인
