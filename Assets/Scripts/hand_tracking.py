import cv2
import mediapipe as mp
import socket
import time
import numpy as np

# Configuración UDP
UDP_IP = "127.0.0.1"
UDP_PORT = 5052
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Configuración MediaPipe
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(
    max_num_hands=1,
    min_detection_confidence=0.7,
    min_tracking_confidence=0.5)
mp_draw = mp.solutions.drawing_utils

cap = cv2.VideoCapture(0)

print("===========================================")
print("🎮 CONTROL DE GESTOS CON MANOS INICIADO 🎮")
print("===========================================")
print("Presiona 'q' para salir")
print("\nGESTOS DISPONIBLES:")
print("  ✋ Mano arriba/abajo = Control Pitch")
print("  👈👉 Mano izq/der = Control Roll")  
print("  ✊ Puño cerrado = Acelerar")
print("  👍 Pulgar arriba = Disparar")
print("===========================================\n")

# Variables para suavizado
prev_x, prev_y = 0.0, 0.0
smoothing_factor = 0.7

def count_fingers_up(hand_landmarks):
    """Cuenta cuántos dedos están levantados"""
    fingers = []
    
    # Pulgar (comparar x en vez de y)
    if hand_landmarks.landmark[4].x < hand_landmarks.landmark[3].x:
        fingers.append(1)
    else:
        fingers.append(0)
    
    # Otros dedos (índice, medio, anular, meñique)
    finger_tips = [8, 12, 16, 20]
    finger_pips = [6, 10, 14, 18]
    
    for tip, pip in zip(finger_tips, finger_pips):
        if hand_landmarks.landmark[tip].y < hand_landmarks.landmark[pip].y:
            fingers.append(1)
        else:
            fingers.append(0)
    
    return sum(fingers)

def detect_fist(hand_landmarks):
    """Detecta si la mano está en forma de puño"""
    # Si todos los dedos están abajo (0 o 1 dedo arriba)
    return count_fingers_up(hand_landmarks) <= 1

def detect_thumbs_up(hand_landmarks):
    """Detecta gesto de pulgar arriba"""
    fingers_up = count_fingers_up(hand_landmarks)
    # Solo pulgar arriba (1 dedo) y que esté realmente arriba
    thumb_up = hand_landmarks.landmark[4].y < hand_landmarks.landmark[2].y
    return fingers_up == 1 and thumb_up

def detect_open_hand(hand_landmarks):
    """Detecta mano completamente abierta"""
    return count_fingers_up(hand_landmarks) >= 4

while True:
    success, img = cap.read()
    if not success:
        break

    img = cv2.flip(img, 1)  # Espejo
    imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    results = hands.process(imgRGB)

    gesture_text = "Sin mano detectada"
    data_to_send = "0.0,0.0,0,0,0"  # x,y,isFist,isThumbsUp,isOpenHand

    if results.multi_hand_landmarks:
        for hand_lms in results.multi_hand_landmarks:
            # Dibujar mano
            mp_draw.draw_landmarks(img, hand_lms, mp_hands.HAND_CONNECTIONS)

            # Obtener coordenadas de la muñeca
            wrist = hand_lms.landmark[0]
            
            # Calcular posición relativa (centro de la pantalla es 0,0)
            x = (wrist.x - 0.5) * 2  # -1 a 1
            y = (wrist.y - 0.5) * 2  # -1 a 1
            
            # Suavizado de movimiento
            x = prev_x * smoothing_factor + x * (1 - smoothing_factor)
            y = prev_y * smoothing_factor + y * (1 - smoothing_factor)
            prev_x, prev_y = x, y

            # Detectar gestos
            is_fist = 1 if detect_fist(hand_lms) else 0
            is_thumbs_up = 1 if detect_thumbs_up(hand_lms) else 0
            is_open_hand = 1 if detect_open_hand(hand_lms) else 0
            
            # Determinar texto del gesto
            if is_thumbs_up:
                gesture_text = "👍 PULGAR ARRIBA - Disparar"
                cv2.putText(img, "DISPARAR!", (10, 100), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 3)
            elif is_fist:
                gesture_text = "✊ PUÑO - Acelerar"
                cv2.putText(img, "ACELERAR!", (10, 100), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 255), 3)
            elif is_open_hand:
                gesture_text = "✋ MANO ABIERTA - Frenar"
                cv2.putText(img, "FRENAR", (10, 100), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 0), 3)
            else:
                gesture_text = "✋ Control normal"

            # Enviar datos a Unity: x,y,isFist,isThumbsUp,isOpenHand
            data_to_send = f"{x:.3f},{-y:.3f},{is_fist},{is_thumbs_up},{is_open_hand}"
            sock.sendto(str.encode(data_to_send), (UDP_IP, UDP_PORT))

            # Mostrar coordenadas
            cv2.putText(img, f"X: {x:.2f} | Y: {-y:.2f}", (10, 30), 
                       cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 255), 2)
            cv2.putText(img, gesture_text, (10, 60), 
                       cv2.FONT_HERSHEY_SIMPLEX, 0.6, (255, 255, 255), 2)
            
            # Dibujar centro de referencia
            h, w, c = img.shape
            cv2.circle(img, (w//2, h//2), 10, (0, 255, 0), 2)
            cv2.line(img, (w//2 - 20, h//2), (w//2 + 20, h//2), (0, 255, 0), 2)
            cv2.line(img, (w//2, h//2 - 20), (w//2, h//2 + 20), (0, 255, 0), 2)

    # Mostrar estado de conexión
    cv2.putText(img, f"UDP: {UDP_IP}:{UDP_PORT}", (10, img.shape[0] - 10), 
               cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 1)

    cv2.imshow("MediaPipe Hand Controller", img)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
print("\n✅ Control de gestos finalizado")
