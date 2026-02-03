using UnityEngine;
using TMPro; 

public class Monster : MonoBehaviour
{
    [Header("Stats")]
    public int hp = 5;
    public int maxHp = 5;
    public int damage = 1;
    
    // 🔥 [복구] PlayerController가 찾고 있던 변수 추가
    public int expReward = 10;
    public int goldReward = 5; 

    [Header("UI")]
    public TextMeshPro textMesh; 
    public Color hpColor = Color.red;

    void Awake()
    {
        if (textMesh == null) textMesh = GetComponentInChildren<TextMeshPro>();
    }

    void Start()
    {
        UpdateUI();
    }

    // 외부에서 스탯 강화 시 호출
    public void SetStats(int newHp, int newDamage)
    {
        hp = newHp;
        maxHp = newHp; 
        damage = newDamage;

        // 난이도가 오르면 보상도 조금 늘려주는 센스 (선택사항)
        // expReward += 2;
        // goldReward += 1;

        UpdateUI(); 
    }

    public void UpdateUI()
    {
        if (textMesh != null)
        {
            textMesh.text = $"{hp}\n<size=70%><color=yellow>ATK {damage}</color></size>";
            textMesh.color = hpColor;
        }
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0)
        {
            Die();
        }
        else
        {
            UpdateUI(); 
        }
    }

    void Die()
    {
        if (EffectManager.Instance) EffectManager.Instance.PlayFixed(EffectManager.Instance.dieEffect, transform.position);
        
        // PlayerController가 직접 goldReward를 가져다 쓰는 구조라면
        // 여기서 GainGold를 또 호출하면 골드가 2배로 들어올 수 있으니,
        // 경험치만 여기서 지급하거나, PlayerController 로직에 따라 조절해야 합니다.
        // 현재는 안전하게 경험치만 여기서 지급합니다.
        if (GameManager.Instance) GameManager.Instance.GainExp(expReward);

        Destroy(gameObject);
    }
}