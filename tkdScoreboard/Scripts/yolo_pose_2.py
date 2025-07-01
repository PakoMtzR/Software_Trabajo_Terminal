import cv2
from ultralytics import YOLO
from collections import defaultdict

# Cargar modelo YOLO (puedes usar uno preentrenado o entrenar uno específico)
model = YOLO('yolov8n.pt')  # Para personas
# model = YOLO('tu_modelo_entrenado.pt')  # Mejor si entrenas uno específico

# Colores para los practicantes
COLOR_ROJO = (0, 0, 255)
COLOR_AZUL = (255, 0, 0)
COLOR_TEXTO = (255, 255, 255)

# Variables para el marcador
puntaje_rojo = 0
puntaje_azul = 0
historial_patadas = defaultdict(list)

# Umbral de movimiento para detectar patada
UMBRAL_PATADA = 30  # Ajustar según necesidad

def detectar_color(region):
    """Función simple para detectar color predominante en una región"""
    # Convertir a HSV para mejor detección de color
    hsv = cv2.cvtColor(region, cv2.COLOR_BGR2HSV)
    
    # Rangos para rojo y azul
    lower_red = cv2.inRange(hsv, (0, 120, 70), (10, 255, 255))
    upper_red = cv2.inRange(hsv, (170, 120, 70), (180, 255, 255))
    red_mask = lower_red + upper_red
    
    blue_mask = cv2.inRange(hsv, (100, 120, 70), (130, 255, 255))
    
    red_pixels = cv2.countNonZero(red_mask)
    blue_pixels = cv2.countNonZero(blue_mask)
    
    if red_pixels > blue_pixels and red_pixels > 100:
        return "rojo"
    elif blue_pixels > red_pixels and blue_pixels > 100:
        return "azul"
    return "desconocido"

def detectar_patada(id_persona, current_position):
    """Detectar movimiento brusco (patada)"""
    if id_persona in historial_patadas:
        # Calcular diferencia con posición anterior
        prev_position = historial_patadas[id_persona][-1]
        movimiento = abs(current_position[0] - prev_position[0]) + abs(current_position[1] - prev_position[1])
        
        if movimiento > UMBRAL_PATADA:
            return True
    return False

# Iniciar captura de video (puede ser cámara o archivo)
video_path = r"C:\Users\FRANCISCO\Downloads\cj_nikolas.mp4"
cap = cv2.VideoCapture(video_path)  # Usar 0 para cámara web, o ruta a archivo de video

while cap.isOpened():
    ret, frame = cap.read()
    if not ret:
        break
    
    # Ejecutar detección con YOLO
    results = model.track(frame, persist=True, classes=0)  # classes=0 para personas
    
    # Procesar resultados
    if results[0].boxes.id is not None:
        boxes = results[0].boxes.xyxy.cpu().numpy().astype(int)
        ids = results[0].boxes.id.cpu().numpy().astype(int)
        
        # Diccionario para guardar info de cada practicante
        practicantes = {}
        
        for box, id in zip(boxes, ids):
            x1, y1, x2, y2 = box
            centro = ((x1 + x2) // 2, (y1 + y2) // 2)
            
            # Obtener región del torso para detectar color
            torso_height = int((y2 - y1) * 0.3)
            torso_region = frame[y1:y1+torso_height, x1:x2]
            
            color = detectar_color(torso_region)
            
            # Solo procesar si es rojo o azul
            if color in ["rojo", "azul"]:
                practicantes[id] = {"color": color, "box": box, "centro": centro}
                
                # Dibujar bounding box según color
                box_color = COLOR_ROJO if color == "rojo" else COLOR_AZUL
                cv2.rectangle(frame, (x1, y1), (x2, y2), box_color, 2)
                
                # Mostrar ID y color
                cv2.putText(frame, f"{color} ID:{id}", (x1, y1 - 10), 
                           cv2.FONT_HERSHEY_SIMPLEX, 0.5, COLOR_TEXTO, 2)
                
                # Detectar patadas
                if detectar_patada(id, centro):
                    if color == "rojo":
                        puntaje_rojo += 1
                    else:
                        puntaje_azul += 1
                    print(f"Patada detectada! {color} ID:{id}")
                
                # Guardar posición para detección de movimiento
                historial_patadas[id].append(centro)
                if len(historial_patadas[id]) > 5:  # Mantener solo las últimas posiciones
                    historial_patadas[id] = historial_patadas[id][-5:]

    # Dibujar marcador
    cv2.rectangle(frame, (10, 10), (200, 100), (0, 0, 0), -1)
    cv2.putText(frame, f"ROJO: {puntaje_rojo}", (20, 40), 
               cv2.FONT_HERSHEY_SIMPLEX, 1, COLOR_ROJO, 2)
    cv2.putText(frame, f"AZUL: {puntaje_azul}", (20, 80), 
               cv2.FONT_HERSHEY_SIMPLEX, 1, COLOR_AZUL, 2)
    
    # Mostrar frame
    cv2.imshow("Taekwondo Tracking", frame)
    
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()