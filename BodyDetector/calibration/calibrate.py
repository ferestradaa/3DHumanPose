import cv2
import numpy as np
import glob

# Definir el tamaño del patrón de ajedrez
pattern_size = (7, 7)  # Número de cuadros interiores (9x6)
square_size = 0.01    # Tamaño de cada cuadro en metros (1 cm)

# Crear un arreglo de puntos de referencia en 3D
objp = np.zeros((pattern_size[0] * pattern_size[1], 3), np.float32)
objp[:, :2] = np.mgrid[0:pattern_size[0], 0:pattern_size[1]].T.reshape(-1, 2) * square_size

# Arreglos para almacenar puntos 3D y 2D
objpoints = []  # Puntos 3D en el espacio del objeto
imgpoints = []  # Puntos 2D en la imagen

# Cargar las imágenes del patrón de ajedrez
images = glob.glob('rawChess/*.jpg')  # Cambia la ruta según tus imágenes

for img_path in images:
    img = cv2.imread(img_path)
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

    # Encontrar los puntos del patrón de ajedrez
    ret, corners = cv2.findChessboardCorners(gray, pattern_size, None)

    if ret:
        objpoints.append(objp)
        imgpoints.append(corners)

        # Dibujar y mostrar los puntos
        cv2.drawChessboardCorners(img, pattern_size, corners, ret)
        cv2.imshow('Chessboard', img)
        cv2.waitKey(500)

cv2.destroyAllWindows()

# Calibrar la cámara
ret, mtx, dist, rvecs, tvecs = cv2.calibrateCamera(objpoints, imgpoints, gray.shape[::-1], None, None)

# Guardar los parámetros de calibración
np.savez('camera_calibration_3d.npz', ret=ret, mtx=mtx, dist=dist, rvecs=rvecs, tvecs=tvecs)
print("\n")
print("Calibración completada.")
print("Matriz de la cámara:\n", mtx)
print("Coeficientes de distorsión:\n", dist)
#print("Matriz de rotacion:\n", rvecs)
#print("Matriz de traslacion: \n", tvecs)


