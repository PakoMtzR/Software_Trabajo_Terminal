import cv2

def mostrar_camara(dispositivo=0):
    """
    Función para mostrar el feed de video de una cámara conectada.
    
    Parámetros:
        dispositivo (int): Número del dispositivo de cámara (0 por defecto para la cámara principal)
    """
    # Crear un objeto VideoCapture
    cap = cv2.VideoCapture(dispositivo)
    
    # Verificar si la cámara se abrió correctamente
    if not cap.isOpened():
        print("Error: No se pudo abrir la cámara")
        return
    
    print("Cámara abierta correctamente. Presiona 'q' para salir.")
    
    while True:
        # Capturar frame por frame
        ret, frame = cap.read()
        
        # Si el frame se leyó correctamente, ret es True
        if not ret:
            print("Error: No se pudo capturar el frame")
            break
        
        # Mostrar el frame en una vent5ana
        cv2.imshow('Cámara', frame)
        
        # Salir del bucle si se presiona 'q'
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break
    
    # Liberar la cámara y cerrar todas las ventanas
    cap.release()
    cv2.destroyAllWindows()

if __name__ == "__main__":
    # Llamar a la función (0 para cámara principal, puedes cambiar a 1, 2, etc. para otras cámaras)
    mostrar_camara(0)
