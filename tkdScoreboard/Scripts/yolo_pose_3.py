import cv2
import numpy as np
from ultralytics import YOLO
from collections import defaultdict
from scipy.spatial.distance import euclidean

# Configuración de colores
COLOR_ROJO = (0, 0, 255)
COLOR_AZUL = (255, 0, 0)
COLOR_ESQUELETO = (0, 255, 0)
COLOR_TEXTO = (255, 255, 255)
COLOR_ALERTA = (0, 165, 255)  # Naranja para patadas

# Cargar modelo YOLO con pose estimation
model = YOLO('yolov8n-pose.pt')  # Requiere versión pose

# Parámetros de detección
UMBRAL_ALTURA_PATA = 0.9  # Razón tobillo/cadera para considerar pierna levantada
UMBRAL_VELOCIDAD = 0.15   # Velocidad mínima (relativa al tamaño de la persona)
UMBRAL_ACELERACION = 0.2  # Aceleración mínima
TIEMPO_ENTRE_PATADAS = 1.0  # segundos
HISTORIAL_FRAMES = 5      # Frames a considerar para velocidad/aceleración

# Estructuras para seguimiento
historial_posiciones = defaultdict(list)
historial_tiempos = defaultdict(list)
puntaje_rojo = 0
puntaje_azul = 0
ultimas_patadas = defaultdict(float)

def detectar_color(region):
    """Detecta color predominante en región (rojo/azul)"""
    hsv = cv2.cvtColor(region, cv2.COLOR_BGR2HSV)
    lower_red = cv2.inRange(hsv, (0, 120, 70), (10, 255, 255))
    upper_red = cv2.inRange(hsv, (170, 120, 70), (180, 255, 255))
    red_mask = lower_red + upper_red
    blue_mask = cv2.inRange(hsv, (100, 120, 70), (130, 255, 255))
    red_pixels = cv2.countNonZero(red_mask)
    blue_pixels = cv2.countNonZero(blue_mask)
    return "rojo" if red_pixels > blue_pixels else "azul" if blue_pixels > 100 else None

def calcular_movimiento(historial, historial_t, frame_time):
    """Calcula velocidad y aceleración del pie"""
    if len(historial) < 3:
        return 0, 0
    
    # Obtener posiciones y tiempos recientes
    pos_actual = historial[-1]
    pos_anterior = historial[-2]
    pos_antigua = historial[-3]
    
    t_actual = frame_time
    t_anterior = historial_t[-2]
    t_antigua = historial_t[-3]
    
    # Calcular velocidades
    vel_actual = euclidean(pos_actual, pos_anterior) / (t_actual - t_anterior)
    vel_anterior = euclidean(pos_anterior, pos_antigua) / (t_anterior - t_antigua)
    
    # Calcular aceleración
    aceleracion = (vel_actual - vel_anterior) / (t_actual - t_anterior)
    
    return vel_actual, aceleracion

def detectar_patada(keypoints, id_persona, frame_time, bbox_height):
    """Detecta patadas usando múltiples criterios"""
    if len(keypoints) < 17 or frame_time - ultimas_patadas[id_persona] < TIEMPO_ENTRE_PATADAS:
        return False
    
    # Índices COCO para keypoints
    LEFT_ANKLE = 15
    RIGHT_ANKLE = 16
    LEFT_HIP = 11
    RIGHT_HIP = 12
    
    # Obtener posiciones actuales (normalizadas)
    left_ankle = keypoints[LEFT_ANKLE][:2]
    right_ankle = keypoints[RIGHT_ANKLE][:2]
    left_hip = keypoints[LEFT_HIP][:2]
    right_hip = keypoints[RIGHT_HIP][:2]
    
    # Criterio 1: Pierna levantada (tobillo sobre cadera)
    pierna_levantada = False
    ankle_pos = None
    
    if left_ankle[1] < left_hip[1] * UMBRAL_ALTURA_PATA:  # Tobillo izquierdo levantado
        pierna_levantada = True
        ankle_pos = left_ankle
    elif right_ankle[1] < right_hip[1] * UMBRAL_ALTURA_PATA:  # Tobillo derecho levantado
        pierna_levantada = True
        ankle_pos = right_ankle
    
    if not pierna_levantada:
        return False
    
    # Actualizar historial de posiciones
    if id_persona in historial_posiciones:
        if len(historial_posiciones[id_persona]) >= HISTORIAL_FRAMES:
            historial_posiciones[id_persona].pop(0)
            historial_tiempos[id_persona].pop(0)
        historial_posiciones[id_persona].append(ankle_pos)
        historial_tiempos[id_persona].append(frame_time)
    else:
        historial_posiciones[id_persona] = [ankle_pos]
        historial_tiempos[id_persona] = [frame_time]
    
    # Criterio 2: Movimiento suficiente (velocidad y aceleración)
    if len(historial_posiciones[id_persona]) >= 3:
        velocidad, aceleracion = calcular_movimiento(
            historial_posiciones[id_persona],
            historial_tiempos[id_persona],
            frame_time
        )
        
        # Normalizar por tamaño de la persona
        velocidad_rel = velocidad / bbox_height
        aceleracion_rel = aceleracion / bbox_height
        
        if (velocidad_rel > UMBRAL_VELOCIDAD and 
            aceleracion_rel > UMBRAL_ACELERACION):
            ultimas_patadas[id_persona] = frame_time
            return True
    
    return False


# Iniciar captura de video (puede ser cámara o archivo)
video_path = r"C:\Users\FRANCISCO\Downloads\cj_nikolas.mp4"
cap = cv2.VideoCapture(video_path)  # Usar 0 para cámara web, o ruta a archivo de video

fps = cap.get(cv2.CAP_PROP_FPS)
frame_time = 0

while cap.isOpened():
    ret, frame = cap.read()
    if not ret:
        break
    
    frame_time += 1/fps if fps > 0 else 0.033
    frame = cv2.flip(frame, 1)  # Efecto espejo
    
    # Detección con YOLO-Pose
    results = model.track(frame, persist=True, classes=0)  # Solo personas
    
    if results[0].boxes.id is not None:
        boxes = results[0].boxes.xyxy.cpu().numpy().astype(int)
        ids = results[0].boxes.id.cpu().numpy().astype(int)
        keypoints = results[0].keypoints.xy.cpu().numpy()
        
        for box, id, kpts in zip(boxes, ids, keypoints):
            x1, y1, x2, y2 = box
            bbox_height = y2 - y1
            
            # Detectar color del peto (parte superior del bbox)
            torso_region = frame[y1:y1+int(bbox_height*0.4), x1:x2]
            color = detectar_color(torso_region)
            
            if color in ["rojo", "azul"]:
                # Dibujar bounding box
                box_color = COLOR_ROJO if color == "rojo" else COLOR_AZUL
                cv2.rectangle(frame, (x1, y1), (x2, y2), box_color, 2)
                
                # Dibujar esqueleto (solo puntos clave visibles)
                for i, kpt in enumerate(kpts):
                    if kpt[2] > 0.3:  # Confianza > 30%
                        cv2.circle(frame, tuple(map(int, kpt[:2])), 4, COLOR_ESQUELETO, -1)
                
                # Detectar patadas
                if detectar_patada(kpts, id, frame_time, bbox_height):
                    if color == "rojo":
                        puntaje_rojo += 1
                        alert_pos = (x1, y1 - 30)
                    else:
                        puntaje_azul += 1
                        alert_pos = (x1, y1 - 60)
                    
                    # Mostrar alerta de patada
                    cv2.putText(frame, "PATADA!", alert_pos, 
                               cv2.FONT_HERSHEY_SIMPLEX, 0.8, COLOR_ALERTA, 2)
                
                # Mostrar ID
                cv2.putText(frame, f"{color} ID:{id}", (x1, y1-10), 
                          cv2.FONT_HERSHEY_SIMPLEX, 0.6, COLOR_TEXTO, 2)
    
    # Dibujar marcador
    cv2.rectangle(frame, (10, 10), (250, 110), (0, 0, 0, 0.7), -1)
    cv2.putText(frame, f"ROJO: {puntaje_rojo}", (20, 50), 
               cv2.FONT_HERSHEY_SIMPLEX, 1, COLOR_ROJO, 2)
    cv2.putText(frame, f"AZUL: {puntaje_azul}", (20, 90), 
               cv2.FONT_HERSHEY_SIMPLEX, 1, COLOR_AZUL, 2)
    
    cv2.imshow("Deteccion Avanzada de Patadas", frame)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()