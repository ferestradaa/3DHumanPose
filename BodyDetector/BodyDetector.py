import cv2
import numpy as np 
import mediapipe as mp 
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D

class pose3d(): 
    def __init__(self): 
        self.img = None
        self.mp_pose = mp.solutions.pose
        self.mp_drawing = mp.solutions.drawing_utils
        self.pose = self.mp_pose.Pose()

        plt.ion()
        self.fig_3dpose = plt.figure()
        self.fig_3dCamera = plt.figure()
        self.ax = self.fig_3dpose.add_subplot(111,projection='3d')
        self.ax2 = self.fig_3dCamera.add_subplot(111,projection='3d')
        plt.show(block=False) 

        self.head = [[8, 6], [6, 3], [3, 7], [7, 9], [9, 10], [10,8]] #representa la conexion necesaria para unir la forma de distintas partes del cuerpo
        self.body = [[12, 11], [11,23], [23,24], [24,12]]
        self.l_leg = [[24, 26], [26, 28], [28,30], [30,32],[32,28]]
        self.r_leg = [[23, 25], [25, 27], [27, 31], [31, 29], [29, 27]]
        self.l_arm = [[12, 14], [14, 16], [16,18], [18, 20], [20, 16]]
        self.r_arm = [[11, 13], [13, 15], [15, 19], [19,17], [17,15]]
        self.full_body = (self.head, self.body, self.l_arm, self.r_arm, self.l_leg, self.r_leg)

        self.calibration_data = np.load('calibration/camera_calibration_3d.npz')
        self.mtx = self.calibration_data['mtx']
        self.dist = self.calibration_data['dist']

        self.transformation_mtx_init = np.eye(4)

    def setImage(self, img): 
        self.img = img
        self.frame_height, self.frame_width, _= self.img.shape

    def getImageLandMarks(self): 
        results = self.pose.process(self.img)
        
        if results.pose_landmarks: 
            self.image_detected_landmakrs = {}
            self.img_rgb = cv2.cvtColor(self.img, cv2.COLOR_BGR2RGB)
            self.mp_drawing.draw_landmarks(
            self.img_rgb,
            results.pose_landmarks, 
            self.mp_pose.POSE_CONNECTIONS
            )

            for i, landmark in enumerate(results.pose_landmarks.landmark): 
                landmark_name = self.mp_pose.PoseLandmark(i).name
                self.image_detected_landmakrs[landmark_name] = (landmark.x*self.frame_width, landmark.y*self.frame_height)

            self.img_landmarks_arr = np.array(list(self.image_detected_landmakrs.values())) #convert landmarks into numpy array
            cv2.imshow("detected landmarks", self.img_rgb)
            return self.image_detected_landmakrs 
        return None
    
        
    def getWordLandMark(self): 
        results = self.pose.process(self.img)

        if results.pose_landmarks: 
            self.world_detected_landmarks = {}
            for i, landmark in enumerate(results.pose_world_landmarks.landmark): 
                landmark_name = self.mp_pose.PoseLandmark(i).name
                self.world_detected_landmarks[landmark_name] = (landmark.x, landmark.y, landmark.z)
            #self.visualize3Dpose(self.world_detected_landmarks)

            self.world_landmarks_arr = np.array(list(self.world_detected_landmarks.values()))
            self.getGlobalPose()
            return self.world_detected_landmarks
        return None
    
    def getGlobalPose(self):
        success, rvec, tvec = cv2.solvePnP(self.world_landmarks_arr, self.img_landmarks_arr, self.mtx, self.dist, flags=cv2.SOLVEPNP_SQPNP)
        self.transformation_mtx_init[0:3,3] = tvec.squeeze()
        world_points_hom = np.concatenate((self.world_landmarks_arr, np.ones((33,1))), axis = 1)
        global_points = world_points_hom.dot(np.linalg.inv(self.transformation_mtx_init).T)
        self.plotCamera_3Dpose(global_points)

        print('\n', global_points) 
        
    def plotCamera_3Dpose(self, global_points): 
        self.ax2.clear()
        x, y, z = global_points[:, 0], global_points[:,1], global_points[:,2]

        for part in self.full_body: 
            for item in part: 
                self.ax2.plot([x[item[0]], x[item[1]]], [y[item[0]], y[item[1]]], [z[item[0]], z[item[1]]])

        self.ax2.set_xlim(-5, 5) #limitis for xyz axis
        self.ax2.set_ylim(0, 2)  
        self.ax2.set_zlim(-5, 5) 
        self.ax2.set_xlabel('X')
        self.ax2.set_ylabel('Y')
        self.ax2.set_zlabel('Z')
        self.ax2.view_init(elev=-65, azim=-90)
  
        plt.draw()
        plt.pause(0.02)


    def visualize3Dpose(self, detected_landmarks): 
        self.ax.clear()
       
        x = [coordinate[0] for coordinate in detected_landmarks.values()]
        y = [coordinate[1] for coordinate in detected_landmarks.values()]
        z = [coordinate[2] for coordinate in detected_landmarks.values()]
                
        for part in self.full_body: 
            for item in part: 
                self.ax.plot([x[item[0]], x[item[1]]], [y[item[0]], y[item[1]]], [z[item[0]], z[item[1]]])

        self.ax.set_xlim(-1, 1) #limitis for xyz axis
        self.ax.set_ylim(-1, 1)  
        self.ax.set_zlim(-1, 1) 
        self.ax.set_xlabel('X')
        self.ax.set_ylabel('Y')
        self.ax.set_zlabel('Z')
        self.ax.view_init(elev=-65, azim=-90)
  
        plt.draw()
        plt.pause(0.02)
