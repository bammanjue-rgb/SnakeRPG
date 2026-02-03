using UnityEngine;

public class WarningEffect : MonoBehaviour
{
    [Header("Settings")]
    public float blinkSpeed = 10f; 
    public float duration = 2.0f;  
    
    private SpriteRenderer sr;
    private Color baseColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        
        baseColor = new Color(1f, 0f, 0f, 0.4f); 
        if (sr != null) sr.color = baseColor;
    }

    void Update()
    {
        if (sr != null)
        {
            float alpha = 0.3f + Mathf.PingPong(Time.time * blinkSpeed, 0.4f); 
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }
    }
    
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}