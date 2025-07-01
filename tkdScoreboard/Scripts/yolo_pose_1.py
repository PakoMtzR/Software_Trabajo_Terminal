from ultralytics import YOLO
import cv2

# Cargar el modelo preentrenado para pose estimation
model = YOLO('yolov8n-pose.pt')  # versión nano (ligera)
# También disponibles: yolov8s-pose.pt, yolov8m-pose.pt, yolov8l-pose.pt, yolov8x-pose.pt

# Fuente de video (0 para cámara web, o ruta a archivo de video)
# video_path = 0  # o 'ruta/a/tu/video.mp4'
video_path = r"C:\Users\FRANCISCO\Downloads\cj_nikolas.mp4"

cap = cv2.VideoCapture(video_path)

while cap.isOpened():
    success, frame = cap.read()
    
    if not success:
        break
    
    # frame = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
    frame = cv2.flip(frame, 1)

    # Ejecutar inference
    results = model(frame)
    
    # Visualizar resultados
    annotated_frame = results[0].plot()
    
    cv2.imshow("YOLOv8 Pose Estimation", annotated_frame)
    
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()