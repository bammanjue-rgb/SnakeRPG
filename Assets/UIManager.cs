using UnityEngine;
using UnityEngine.UI; 
using TMPro; 
using System.Collections; 
using System.Collections.Generic; 

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("기존 HUD 연결")]
    public Slider hpSlider;
    public TMP_Text hpText;
    public TMP_Text goldText; 
    public TMP_Text levelText;
    public Slider expSlider;

    [Header("신규 HUD 연결")]
    public TMP_Text attackText;      
    public TMP_Text speedText;       
    public TMP_Text survivalTimeText;

    [Header("패널 연결")]
    public GameObject gameOverPanel;
    public GameObject levelUpWindow;
    
    [Header("모드 선택 및 알림")]
    public GameObject startScreenPanel;
    public TMP_Text notificationText;

    [Header("레벨업 카드 텍스트 연결")]
    public TMP_Text[] cardTitles;
    public TMP_Text[] cardDescriptions;

    [Header("플로팅 텍스트 설정")]
    public GameObject floatingTextPrefab; 
    public float textSpacing = 25f; 
    
    private List<FloatingText> activeFloatingTexts = new List<FloatingText>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (startScreenPanel != null) startScreenPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelUpWindow != null) levelUpWindow.SetActive(false);
        if (notificationText != null) notificationText.gameObject.SetActive(false);
        Time.timeScale = 0; 
    }

    public void ShowFloatingText(Vector3 worldPosition, string message, Color color)
    {
        if (floatingTextPrefab == null) return;

        for (int i = activeFloatingTexts.Count - 1; i >= 0; i--)
        {
            if (activeFloatingTexts[i] == null) activeFloatingTexts.RemoveAt(i);
        }

        foreach (FloatingText txt in activeFloatingTexts)
        {
            if (txt != null) txt.BumpUp(textSpacing);
        }

        if (activeFloatingTexts.Count >= 3)
        {
            if (activeFloatingTexts[0] != null) Destroy(activeFloatingTexts[0].gameObject);
            activeFloatingTexts.RemoveAt(0);
        }

        Vector3 spawnPos = worldPosition + new Vector3(0, 15f, -5f); 
        GameObject go = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
        FloatingText ftScript = go.GetComponent<FloatingText>();
        
        if (ftScript != null)
        {
            ftScript.Setup(message, color);
            activeFloatingTexts.Add(ftScript);
        }
    }

    public void ShowNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationText.gameObject.SetActive(true);
            StopAllCoroutines(); 
            StartCoroutine(FadeOutNotification(2f)); 
        }
    }

    IEnumerator FadeOutNotification(float duration)
    {
        yield return new WaitForSecondsRealtime(duration); 
        if (notificationText != null) notificationText.gameObject.SetActive(false);
    }

    public void OnModeSelectBtnClick(bool isAuto)
    {
        if (startScreenPanel != null) startScreenPanel.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.StartGame(isAuto);
    }

    public void OnCardSelect(int cardIndex) 
    { 
        if (GameManager.Instance != null) GameManager.Instance.ApplyLevelUpCard(cardIndex); 
        HideLevelUpWindow(); 
    }

    public void OnRestartBtnClick() 
    { 
        Time.timeScale = 1; 
        if (GameManager.Instance != null) GameManager.Instance.RestartGame(); 
    }

    public void UpdateHP(int currentHP, int maxHP)
    {
        if (hpSlider != null) { hpSlider.maxValue = maxHP; hpSlider.value = currentHP; }
        if (hpText != null) hpText.text = $"HP: {currentHP} / {maxHP}";
    }

    public void UpdateGold(int gold)
    {
        if (goldText != null) goldText.text = $"Gold: {gold}";
    }

    public void UpdateLevel(int level, int currentExp, int requiredExp)
    {
        if (levelText != null) levelText.text = $"Lv.{level}";
        if (expSlider != null)
        {
            expSlider.maxValue = requiredExp;
            expSlider.value = currentExp;
        }
    }

    public void UpdateStats(int attack, float speed)
    {
        if (attackText != null) attackText.text = $"ATK: {attack}";
        if (speedText != null) speedText.text = $"SPD: {speed:F1}";
    }

    public void UpdateTime(float time)
    {
        if (survivalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60F);
            int seconds = Mathf.FloorToInt(time % 60F);
            survivalTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void UpdateCardUI(LevelUpCard[] cards)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (i >= cardTitles.Length || i >= cardDescriptions.Length) break;
            if (cardTitles[i] != null) cardTitles[i].text = cards[i].title;
            if (cardDescriptions[i] != null) cardDescriptions[i].text = cards[i].description;
        }
    }

    public void ShowGameOver() { if (gameOverPanel != null) gameOverPanel.SetActive(true); }
    public void ShowLevelUpWindow() { if (levelUpWindow != null) { levelUpWindow.SetActive(true); Time.timeScale = 0; } }
    public void HideLevelUpWindow() { if (levelUpWindow != null) { levelUpWindow.SetActive(false); Time.timeScale = 1; } }
}