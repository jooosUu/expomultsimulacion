using UnityEngine;

public class Leaf : MonoBehaviour
{
    [Header("Configuración")]
    public float nutritionValue = 100f;
    public float consumeRate = 10f;
    
    private float currentNutrition;
    private bool beingEaten = false;
    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        currentNutrition = nutritionValue;
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        // Actualizar escala según nutrición restante
        float scale = currentNutrition / nutritionValue;
        transform.localScale = originalScale * scale;
        
        // Cambiar transparencia según se consume
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(0.3f, 1f, scale);
            spriteRenderer.color = color;
        }
    }
    
    public void StartEating()
    {
        beingEaten = true;
    }
    
    public void Consume(float amount)
    {
        currentNutrition -= consumeRate * amount;
        
        if (currentNutrition <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    public bool IsBeingEaten()
    {
        return beingEaten;
    }
    
    public bool IsConsumed()
    {
        return currentNutrition <= 0;
    }
}