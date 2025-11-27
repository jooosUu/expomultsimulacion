using UnityEngine;

/// <summary>
/// Estructura de datos para almacenar información de gestos de mano
/// </summary>
[System.Serializable]
public class HandGestureData
{
    public Vector3 position;        // Posición normalizada de la mano (-1 a 1)
    public bool isFist;             // Puño cerrado
    public bool isThumbsUp;         // Pulgar arriba
    public bool isOpenHand;         // Mano completamente abierta
    public float timestamp;         // Momento de captura

    public HandGestureData()
    {
        position = Vector3.zero;
        isFist = false;
        isThumbsUp = false;
        isOpenHand = false;
        timestamp = Time.time;
    }

    public HandGestureData(Vector3 pos, bool fist, bool thumbs, bool open)
    {
        position = pos;
        isFist = fist;
        isThumbsUp = thumbs;
        isOpenHand = open;
        timestamp = Time.time;
    }

    public override string ToString()
    {
        return $"Hand: Pos({position.x:F2}, {position.y:F2}) | Fist:{isFist} | Thumbs:{isThumbsUp} | Open:{isOpenHand}";
    }
}
