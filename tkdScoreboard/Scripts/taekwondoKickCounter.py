import cv2
import numpy as np
from ultralytics import YOLO
from collections import defaultdict
import json
import os
from datetime import datetime

class TaekwondoKickCounter:
    def __init__(self):
        # Cargar modelo YOLOv8 con pose estimation
        self.model = YOLO('yolov8n-pose.pt')  # o 'yolov8n-pose.pt' para mejor precisión
        
        # Variables para seguimiento
        self.players = {}  # {id: {'color': 'red/blue', 'kick_count': int, 'prev_foot_pos': (x,y)}}
        self.next_id = 0
        self.kick_threshold = 50  # Umbral de movimiento para considerar patada
        
        # Rangos de color para petos (en HSV)
        self.color_ranges = {
            'red': ([0, 100, 100], [10, 255, 255]),
            'blue': ([100, 100, 100], [130, 255, 255])
        }

        # Archivo de salida para resultados
        self.output_filename = "taekwondo_results.json"
        
    def detect_vest_color(self, bbox, frame):
        """Detecta el color del peto dentro del bounding box"""
        x1, y1, x2, y2 = map(int, bbox)
        roi = frame[y1:y2, x1:x2]
        
        # Convertir a HSV para mejor detección de color
        hsv = cv2.cvtColor(roi, cv2.COLOR_BGR2HSV)
        
        # Buscar el color predominante
        max_pixels = 0
        vest_color = None
        
        for color, (lower, upper) in self.color_ranges.items():
            mask = cv2.inRange(hsv, np.array(lower), np.array(upper))
            pixels = cv2.countNonZero(mask)
            
            if pixels > max_pixels and pixels > 100:  # Umbral mínimo
                max_pixels = pixels
                vest_color = color
                
        return vest_color
    
    def is_kicking(self, player_id, current_foot_pos):
        """Determina si el jugador está pateando basado en movimiento del pie"""
        if player_id not in self.players or 'prev_foot_pos' not in self.players[player_id]:
            return False
            
        prev_pos = self.players[player_id]['prev_foot_pos']
        if prev_pos is None or current_foot_pos is None:
            return False
            
        # Calcular distancia euclidiana
        distance = np.sqrt((current_foot_pos[0] - prev_pos[0])**2 + 
                          (current_foot_pos[1] - prev_pos[1])**2)
        
        return distance > self.kick_threshold
    
    def process_frame(self, frame):
        # Ejecutar detección y pose estimation
        results = self.model(frame, verbose=False)
        
        # Procesar cada detección
        for result in results:
            # Verificar si hay detecciones
            if not result.boxes:
                continue
                
            for box, keypoints in zip(result.boxes, result.keypoints):
                # Solo considerar personas (clase 0 en YOLO)
                if int(box.cls) == 0:
                    # Obtener bounding box
                    bbox = box.xyxy[0].cpu().numpy()
                    
                    # Detectar color del peto
                    vest_color = self.detect_vest_color(bbox, frame)
                    if vest_color is None:
                        continue
                    
                    # Asignar ID o emparejar con jugador existente
                    player_id = None
                    for pid, data in self.players.items():
                        if data['color'] == vest_color:
                            player_id = pid
                            break
                    
                    if player_id is None:
                        player_id = self.next_id
                        self.players[player_id] = {'color': vest_color, 'kick_count': 0, 'prev_foot_pos': None}
                        self.next_id += 1
                    
                    # Obtener posiciones clave del pie (keypoints 15, 16 en COCO pose)
                    keypoints_np = keypoints.xy[0].cpu().numpy()  # Forma: (17, 2)
                    
                    # Índices COCO para pies:
                    # 15 - left ankle, 16 - right ankle
                    left_foot = keypoints_np[15] if keypoints.conf[0][15] > 0.3 else None
                    right_foot = keypoints_np[16] if keypoints.conf[0][16] > 0.3 else None
                    
                    # Usar el pie que esté visible
                    current_foot_pos = left_foot if left_foot is not None else right_foot
                    
                    # Detectar patada
                    if self.is_kicking(player_id, current_foot_pos):
                        self.players[player_id]['kick_count'] += 1
                        print(f"¡Patada detectada! Jugador {vest_color} (ID {player_id}): {self.players[player_id]['kick_count']} patadas")
                    
                    # Actualizar posición anterior del pie
                    self.players[player_id]['prev_foot_pos'] = current_foot_pos
                    
                    # Dibujar información en el frame
                    self.draw_player_info(frame, player_id, bbox, keypoints_np, keypoints.conf[0].cpu().numpy())
        
        # Mostrar marcador
        self.draw_scoreboard(frame)
        return frame
    
    def draw_player_info(self, frame, player_id, bbox, keypoints_np, confidences):
        """Dibuja información del jugador en el frame"""
        color = (0, 0, 255) if self.players[player_id]['color'] == 'red' else (255, 0, 0)
        x1, y1, x2, y2 = map(int, bbox)
        
        # Dibujar bounding box
        cv2.rectangle(frame, (x1, y1), (x2, y2), color, 2)
        
        # Dibujar esqueleto completo
        self.draw_skeleton(frame, keypoints_np, confidences)
        
        # Dibujar ID y conteo de patadas
        cv2.putText(frame, f"ID {player_id}: {self.players[player_id]['kick_count']} patadas", 
                (x1, y1 - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.5, color, 2)
    
    def draw_scoreboard(self, frame):
        """Dibuja el marcador en la parte superior del frame"""
        red_kicks = sum(p['kick_count'] for p in self.players.values() if p['color'] == 'red')
        blue_kicks = sum(p['kick_count'] for p in self.players.values() if p['color'] == 'blue')
        
        cv2.putText(frame, f"ROJO: {red_kicks} patadas", (10, 30), 
                   cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2)
        cv2.putText(frame, f"AZUL: {blue_kicks} patadas", (10, 70), 
                   cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 0, 0), 2)
    
    def draw_skeleton(self, frame, keypoints_np, confidences):
        """Dibuja todos los landmarks y sus conexiones para formar un esqueleto"""
        # Conexiones para el formato COCO (17 puntos)
        connections = [
            # Cuerpo
            (5, 6), (5, 7), (6, 8), (7, 9), (8, 10),
            # Brazos
            (5, 11), (6, 12), (11, 13), (12, 14), (13, 15), (14, 16),
            # Piernas
            (11, 12), (11, 23), (12, 24), (23, 24), (23, 25), (24, 26), (25, 27), (26, 28)
        ]
        
        # Filtrar solo las conexiones válidas para COCO (0-16)
        valid_connections = [
            (start, end) for start, end in connections 
            if start < 17 and end < 17
        ]
        
        # Dibujar conexiones (huesos)
        for start, end in valid_connections:
            if (confidences[start] > 0.3 and confidences[end] > 0.3):
                start_point = tuple(map(int, keypoints_np[start]))
                end_point = tuple(map(int, keypoints_np[end]))
                cv2.line(frame, start_point, end_point, (0, 255, 255), 2)
        
        # Dibujar puntos clave (articulaciones)
        for i, point in enumerate(keypoints_np):
            if i < 17 and confidences[i] > 0.3:  # Solo los 17 puntos COCO
                center = tuple(map(int, point))
                cv2.circle(frame, center, 5, (0, 0, 255), -1)

    def save_results(self):
        """Guarda los resultados en un archivo JSON"""
        results = {
            "timestamp": datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            "players": []
        }
        
        for player_id, data in self.players.items():
            results["players"].append({
                "id": player_id,
                "color": data["color"],
                "kick_count": data["kick_count"]
            })
        
        # Guardar en archivo
        with open(self.output_filename, 'w') as f:
            json.dump(results, f, indent=4)
        
        print(f"Resultados guardados en {self.output_filename}")

# Uso del sistema
def main():
    kick_counter = TaekwondoKickCounter()
    
    # Para video en tiempo real
    # video_path = r"C:\Users\FRANCISCO\Downloads\cj_nikolas.mp4"
    # cap = cv2.VideoCapture(1)  # O usar un archivo de video
    
    try:
        cap = cv2.VideoCapture(0)
        
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break
            
            frame = cv2.flip(frame, 1)
            processed_frame = kick_counter.process_frame(frame)
            cv2.imshow('Taekwondo Kick Counter', processed_frame)
            
            if cv2.waitKey(1) & 0xFF == ord('q'):
                kick_counter.save_results()  # Guardar resultados al finalizar
                break

    finally:
        cap.release()
        cv2.destroyAllWindows()
        

if __name__ == "__main__":
    main()