using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float spawnRate = 2f;
    public float minHeight = -2f;
    public float maxHeight = 2f;
    
    private float timer = 0f;

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isPlaying)
        {
            timer += Time.deltaTime;
            
            if (timer >= spawnRate)
            {
                SpawnObstacle();
                timer = 0f;
            }
        }
    }

    void SpawnObstacle()
    {
        float randomHeight = Random.Range(minHeight, maxHeight);
        Vector3 spawnPosition = new Vector3(transform.position.x, randomHeight, 0);
        GameObject obstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
        Destroy(obstacle, 10f); // Destruir después de 10 segundos
    }
}