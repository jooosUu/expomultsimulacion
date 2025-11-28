using UnityEngine;
using System.Collections.Generic; // Necesario para usar la lista de cuerpos

public class OrbitalMechanics : MonoBehaviour
{
    // Variables de configuración (visibles en el Inspector)
    public float mass; // Masa del cuerpo (escalada)
    public Vector3 initialVelocity; // Velocidad inicial (necesaria para la órbita)

    // Variables internas (privadas)
    private Vector3 currentVelocity;
    // Constante de Gravitación Universal (debes escalarla)
    // Este valor de ejemplo es arbitrario; ajústalo hasta que veas órbitas estables.
    private const float G_SCALED = 0.0001f;

    // Lista estática de todos los cuerpos en la escena
    private static List<OrbitalMechanics> AllBodies;

    void Awake()
    {
        // Inicializa la lista si es nula
        if (AllBodies == null)
            AllBodies = new List<OrbitalMechanics>();

        // Añade este cuerpo a la lista de todos los cuerpos
        AllBodies.Add(this);
    }

    void Start()
    {
        // Asigna la velocidad inicial al empezar la simulación
        currentVelocity = initialVelocity;
    }

    void FixedUpdate()
    {
        // El bucle principal de la física
        ApplyGravity(); // 1. Calcula la fuerza gravitacional neta
        MoveBody();    // 2. Aplica la fuerza (Integración)
    }

    void ApplyGravity()
    {
        Vector3 netAcceleration = Vector3.zero;

        // Itera sobre la lista de todos los cuerpos celestes
        foreach (OrbitalMechanics other in AllBodies)
        {
            // Evita calcular la fuerza de un cuerpo sobre sí mismo
            if (other != this)
            {
                Vector3 direction = other.transform.position - transform.position; // Vector de posición
                float distanceSq = direction.sqrMagnitude; // Distancia al cuadrado (más rápido que usar .magnitude)

                // Si la distancia es muy cercana (colisión), evita dividir por cero
                if (distanceSq < 0.001f) continue;

                // Magnitud de la Aceleración: a = (G * M) / r^2
                // Usamos la masa del OTRO cuerpo (M)
                float accelerationMagnitude = (G_SCALED * other.mass) / distanceSq;

                // Suma la aceleración total vectorial
                netAcceleration += direction.normalized * accelerationMagnitude;
            }
        }

        // Aplica la aceleración total a la velocidad
        currentVelocity += netAcceleration * Time.fixedDeltaTime;
    }

    void MoveBody()
    {
        // Aplica la velocidad a la posición
        transform.position += currentVelocity * Time.fixedDeltaTime;
    }
}