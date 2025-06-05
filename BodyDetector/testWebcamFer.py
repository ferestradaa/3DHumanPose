import BodyDetector
import cv2
import time 

def init_video(): #function to read from video
    cap = cv2.VideoCapture('fer.mp4')
    pose3D = BodyDetector.pose3d()

    while cap.isOpened(): 
        ret, frame = cap.read()
        if not ret: 
            break
        pose3D.setImage(frame)
        image = pose3D.getImageLandMarks()
        world = pose3D.getWordLandMark()
        #print('\n',world)
        #v2.imshow("Output", frame)
        if cv2.waitKey(1) & 0xFF == ord('q'): 
            break 

    cap.release()
    cv2.destroyAllWindows()

def init_image(): #function to read image for debuggin
    img = cv2.imread('img.jpeg')
    pose3D_ = BodyDetector.pose3d()
    pose3D_.setImage(cv2.resize(img, (500, 600)))
    #pose3D = BodyDetector.pose3d(img)
    image_pts = pose3D_.getImageLandMarks()
    world_pts = pose3D_.getWordLandMark()
    #print('\n', image_pts)

    cv2.waitKey(0)
    cv2.destroyAllWindows()

if __name__ == '__main__':
    #init_video() #change for the use you want
    init_image()
