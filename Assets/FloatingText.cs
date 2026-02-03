using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 20f;       
    public float dampTime = 0.2f;       
    
    [Header("Appearance Settings")]
    public float fadeDuration = 1.0f;   
    public float lifeTime = 1.5f;       
    public Vector3 startScale = new Vector3(0.7f, 0.7f, 1f); 
    public Vector3 peakScale = new Vector3(1.2f, 1.2f, 1f);  
    
    private TMP_Text textMesh;
    private float alpha;
    private float timer;
    private Color startColor;
    
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero; 

    void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
        if (textMesh == null) textMesh = GetComponentInChildren<TMP_Text>();
    }

    public void Setup(string message, Color color)
    {
        if (textMesh != null)
        {
            textMesh.text = message;
            textMesh.color = color;
            startColor = color;
            
            // 🔥 Fix: 컴파일 에러 수정 (MeshRenderer 접근)
            MeshRenderer rend = GetComponent<MeshRenderer>();
            if (rend != null)
            {
                rend.sortingOrder = 100; 
            }
        }

        alpha = 1f;
        timer = 0f;
        
        targetPosition = transform.position; 
        transform.localScale = startScale;   
        
        Destroy(gameObject, lifeTime);
    }

    public void BumpUp(float amount)
    {
        targetPosition += new Vector3(0, amount, 0);
    }

    void Update()
    {
        timer += Time.deltaTime;

        targetPosition += Vector3.up * moveSpeed * Time.deltaTime;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, dampTime);

        if (timer < 0.1f)
        {
            float t = timer / 0.1f;
            transform.localScale = Vector3.Lerp(startScale, peakScale, t);
        }
        else if (timer < 0.25f)
        {
            float t = (timer - 0.1f) / 0.15f;
            transform.localScale = Vector3.Lerp(peakScale, Vector3.one, t);
        }

        float fadeStartTime = lifeTime - fadeDuration;
        if (timer >= fadeStartTime)
        {
            float progress = (timer - fadeStartTime) / fadeDuration;
            alpha = Mathf.Lerp(1f, 0f, progress);

            if (textMesh != null)
            {
                Color newColor = startColor;
                newColor.a = alpha;
                textMesh.color = newColor;
            }
        }
    }
}