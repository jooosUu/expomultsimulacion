using UnityEngine;
using System.Collections.Generic;

public class Ant : MonoBehaviour
{
    [Header("Configuración")]
    public float speed = 2f;
    public float detectionRadius = 3f;
    public float eatDistance = 0.3f;
    public float wanderChangeTime = 2f;
    
    private Vector2 targetPosition;
    private float wanderTimer;
    private Leaf targetLeaf;
    private bool isEating;
    private SpriteRenderer spriteRenderer;
    
    private enum AntState { Wandering, SeekingFood, Eating }
    private AntState currentState = AntState.Wandering;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetRandomTarget();
    }
    
    void Update()
    {
        switch (currentState)
        {
            case AntState.Wandering:
                Wander();
                CheckForFood();
                break;
                
            case AntState.SeekingFood:
                SeekFood();
                break;
                
            case AntState.Eating:
                Eat();
                break;
        }
        
        // Voltear sprite según dirección
        if (targetPosition.x < transform.position.x)
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;
    }
    
    void Wander()
    {
        // Moverse hacia el objetivo aleatorio
        transform.position = Vector2.MoveTowards(
            transform.position, 
            targetPosition, 
            speed * Time.deltaTime
        );
        
        // Cambiar dirección aleatoriamente
        wanderTimer += Time.deltaTime;
        if (wanderTimer >= wanderChangeTime || 
            Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            SetRandomTarget();
            wanderTimer = 0f;
        }
    }
    
    void CheckForFood()
    {
        // Buscar hojas cercanas
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        
        foreach (Collider2D hit in hits)
        {
            Leaf leaf = hit.GetComponent<Leaf>();
            if (leaf != null && !leaf.IsBeingEaten())
            {
                targetLeaf = leaf;
                currentState = AntState.SeekingFood;
                break;
            }
        }
    }
    
    void SeekFood()
    {
        if (targetLeaf == null || targetLeaf.IsConsumed())
        {
            currentState = AntState.Wandering;
            targetLeaf = null;
            return;
        }
        
        // Moverse hacia la hoja
        transform.position = Vector2.MoveTowards(
            transform.position, 
            targetLeaf.transform.position, 
            speed * Time.deltaTime
        );
        
        // Verificar si llegó a la hoja
        float distance = Vector2.Distance(transform.position, targetLeaf.transform.position);
        if (distance < eatDistance)
        {
            currentState = AntState.Eating;
            targetLeaf.StartEating();
        }
    }
    
    void Eat()
    {
        if (targetLeaf == null || targetLeaf.IsConsumed())
        {
            currentState = AntState.Wandering;
            targetLeaf = null;
            return;
        }
        
        // Consumir la hoja
        targetLeaf.Consume(Time.deltaTime);
    }
    
    void SetRandomTarget()
    {
        // Obtener límites de la cámara
        Camera cam = Camera.main;
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;
        
        targetPosition = new Vector2(
            Random.Range(-width/2 + 1, width/2 - 1),
            Random.Range(-height/2 + 1, height/2 - 1)
        );
    }
    
    void OnDrawGizmosSelected()
    {
        // Visualizar radio de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}