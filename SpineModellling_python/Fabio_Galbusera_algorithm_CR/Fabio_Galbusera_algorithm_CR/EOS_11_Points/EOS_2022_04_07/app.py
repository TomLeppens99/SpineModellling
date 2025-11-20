import network_4_stacks
#import network
import preprocessing
import postprocessing
import os
import matplotlib.pyplot as plt
import torch
import dsntnn
import glob
from calculate_angles import *
from view_images import *
import time
import pandas as pd
import cv2 as cv
 

# from PyQt5.QtWidgets import (QMainWindow, QTextEdit, QAction, QFileDialog, QApplication, QPushButton, QWidget, QInputDialog, QHBoxLayout, QVBoxLayout, QProgressBar, QDialog, QLabel)
from PyQt5.QtGui import QIcon, QFontDatabase
from PyQt5 import QtCore
from PyQt5.QtCore import QThread, pyqtSignal
import sys
from pathlib import Path



class Main(QMainWindow):

    def __init__(self):
        super().__init__()



        self.initUI()
        
        

    def initUI(self):

        self.layout = QHBoxLayout()
        container = QWidget()
        container.setLayout(self.layout)
        self.setCentralWidget(container)
        self.layout.setContentsMargins(10, 10, 10, 10)

        self.right_box = QVBoxLayout()
        self.layout.addLayout(self.right_box)

        fixedfont = QFontDatabase.systemFont(QFontDatabase.FixedFont)
        fixedfont.setPointSize(12)

        self.choosefolder = QPushButton("Choose Folder", self)
        self.right_box.addWidget(self.choosefolder)
        self.choosefolder.clicked.connect(self.showFolder)
        self.choosefolder.setFont(fixedfont)

        self.choosefile = QPushButton("Choose Image", self)
        self.right_box.addWidget(self.choosefile)
        self.choosefile.clicked.connect(self.showFile)
        self.choosefile.setFont(fixedfont)

        self.description = QLabel()
        self.description.setAlignment(QtCore.Qt.AlignCenter)
        self.description.setText('''Choose Folder: processes an entire folder with paired EOS images and save a txt file with name 'exam_code.txt' with the coordinates and a excel file with the angles automatically calculated\n
            Choose Image: it works only after that the folder has been processed and the text files generated. When you choose an image (lateral or anteroposterior) it prompts a window with the 2 images with the points plotted 
            and a box with angles values\n
            Important notes:\n 
            1 - the images must be in pairs and named as exam_code_C.(jpg, jpeg, tif, png) for anteroposterior images and exam_code_S or exam_code_L.(jpg, jpeg, tif, png) for lateral images.\n
            2 - When the process is over you must close the window with the progress bar to make another operation''')
        
        

        self.right_box.addWidget(self.description)

        self.exitButton = QPushButton("Close", self)
        self.right_box.addWidget(self.exitButton, alignment=QtCore.Qt.AlignRight | QtCore.Qt.AlignBottom)
        self.exitButton.clicked.connect(QtCore.QCoreApplication.instance().quit)
        self.exitButton.setFont(fixedfont)

        #self.right_box.addWidget(Actions())       

        self.setGeometry(300, 300, 550, 450)
        self.setWindowTitle('Process EOS images')
        self.show()

    def showFile(self):
        self.choosefolder.setEnabled(False)
        self.choosefile.setEnabled(False)
        home_dir = str(Path.home())
        self.fname = QFileDialog.getOpenFileName(self, 'Open file', home_dir,"Image files (*.jpg *.gif *.jpeg *.tif *.png)")

        if not self.fname:
            self.choosefolder.setEnabled(True)
            self.choosefile.setEnabled(True)
            return
        

        self.view_images()

    def showFolder(self):
        self.choosefolder.setEnabled(False)
        self.choosefile.setEnabled(False)
        self.dirname = QFileDialog.getExistingDirectory(None, 'Select project folder:', 'C:\\', QFileDialog.ShowDirsOnly)
        if not self.dirname:
            self.choosefolder.setEnabled(True)
            self.choosefile.setEnabled(True)
            return
        model = init_model()

        self.progress = Actions(model, self.dirname)

        self.progress.onProgressStart()

        self.choosefolder.setEnabled(True)
        self.choosefile.setEnabled(True)
           


    def view_images(self):
        
        file_extension = os.path.splitext(os.path.basename(self.fname[0]))[1]
        x = re.search(file_extension, self.fname[0])

        with open(self.fname[0][:x.start()][:-2] + '.txt', 'r') as f:
            file = f.readlines()
        #predicted angles
        L1_L5, L1_S1, T4_T12, SS, PI, PT = angles_from_file(file)
        
        c_sup, c_inf = coordinates(file)
        
        m_sup, m_inf = coefficients(c_sup, c_inf)
        vertebrae = ['T1', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'T8', 'T9', 'T10', 'T11', 'T12', 'L1', 'L2', 'L3', 'L4', 'L5', 'S1']
        
        cobb, v = Cobb(m_sup, m_inf, vertebrae)
    
        coords_ap, coords_lat = read(file)
        
        img_ap = cv.imread(self.fname[0][:x.start()][:-2] + '_C' + file_extension, cv.IMREAD_COLOR)
        img_lat = cv.imread(self.fname[0][:x.start()][:-2] + '_S' + file_extension, cv.IMREAD_COLOR)

        if img_lat == None:
        	img_lat = cv.imread(self.fname[0][:x.start()][:-2] + '_L' + file_extension, cv.IMREAD_COLOR)

        img_tot = np.concatenate((img_ap, img_lat), axis = 1)

        text = 'Cobb ({}) = {:.2f}\nL1-L5 = {:.2f}\nL1-S1 = {:.2f}\nT4-T12 = {:.2f}\nSS = {:.2f}\nPI = {:.2f}\nPT = {:.2f}'.format(v, cobb, L1_L5, L1_S1, T4_T12, SS, PI, PT)

        props = dict(boxstyle = 'round', facecolor = 'wheat', alpha = 0.5)

        fig = plt.figure(frameon=False)
        fig.set_size_inches(10, 15)        
        ax = plt.Axes(fig, [0., 0., 1., 1.])
        ax.set_axis_off()
        fig.add_axes(ax)        
        ax.imshow(img_tot, aspect='auto')
        ax.scatter(coords_ap[:, 0], coords_ap[:, 1], marker='.', color = 'y')
        ax.scatter(coords_lat[:, 0] + img_ap.shape[1], coords_lat[:, 1], marker='.', color = 'y')
        ax.text(0.05, 0.95, text, transform = ax.transAxes, fontsize = 14, verticalalignment = 'top', bbox = props)
        plt.show()

        self.choosefolder.setEnabled(True)
        self.choosefile.setEnabled(True)
        


class External(QThread):
    """
    Runs a counter thread.
    """
    countChanged = pyqtSignal(float)
    

    def __init__(self, model, dirname):
        super().__init__()
        
        self.dirname = dirname
        self.model = model

    def run(self):
        k = 0
        #length = len(os.listdir(self.dirname)) / 2
        #length = len(glob.glob(self.dirname,"*.jpg")) / 2
        length = len([f for f in os.listdir(self.dirname) if (f.endswith('.tif') or f.endswith('.jpg') or f.endswith('.jpeg') or f.endswith('.png')) and os.path.isfile(os.path.join(self.dirname, f))]) / 2
        
        for i in os.listdir(self.dirname):            

            if os.path.splitext(os.path.basename(i))[0][-1] == 'C':
                #print(length)
                k += 1
                
                image, size_ap, size_lat = preprocessing.ReadImages(self.dirname, i).preprocess()

                image = image.unsqueeze(dim = 0)

                #image = image.to(self.device)

                coords, _ = self.model(image)
                
                coords = coords[-1]
                
                #self.progressbar.timerEvent(k, length)
                #landmarks_true_retransformed = dsntnn.normalized_to_pixel_coordinates(target_var, (input_var.shape[2], input_var.shape[3]))        
                landmarks_pred_retransformed = dsntnn.normalized_to_pixel_coordinates(coords, (image.shape[2], image.shape[3]))

                landmarks_pred = landmarks_pred_retransformed.squeeze()

                ind_antero = list(range(1, len(landmarks_pred), 2))
                ind_lateral = list(range(0, len(landmarks_pred), 2))
                
                landmarks_pred_ap = landmarks_pred[ind_antero, :]
                landmarks_pred_lat = landmarks_pred[ind_lateral, :]
                

                landmarks_ap = postprocessing.resize_coordinates(coordinates = landmarks_pred_ap.cpu(), size = size_ap, lat = False)
                landmarks_lat = postprocessing.resize_coordinates(coordinates = landmarks_pred_lat.cpu(), size = size_lat, lat = True)
                
                labels = ['T1,Plat_Sup_G',
							'T1,Plat_Sup_D',
							'T1,Plat_Sup_Ant',
							'T1,Plat_Sup_Post',
							'T1,Plat_Inf_G',
							'T1,Plat_Inf_D',
							'T1,Plat_Inf_Ant',
							'T1,Plat_Inf_Post',
							'T1,Centroid_G',
							'T1,Centroid_D',
							'T2,Plat_Sup_G',
							'T2,Plat_Sup_D',
							'T2,Plat_Sup_Ant',
							'T2,Plat_Sup_Post',
							'T2,Plat_Inf_G',
							'T2,Plat_Inf_D',
							'T2,Plat_Inf_Ant',
							'T2,Plat_Inf_Post',
							'T2,Centroid_G',
							'T2,Centroid_D',
							'T3,Plat_Sup_G',
							'T3,Plat_Sup_D',
							'T3,Plat_Sup_Ant',
							'T3,Plat_Sup_Post',
							'T3,Plat_Inf_G',
							'T3,Plat_Inf_D',
							'T3,Plat_Inf_Ant',
							'T3,Plat_Inf_Post',
							'T3,Centroid_G',
							'T3,Centroid_D',
							'T4,Plat_Sup_G',
							'T4,Plat_Sup_D',
							'T4,Plat_Sup_Ant',
							'T4,Plat_Sup_Post',
							'T4,Plat_Inf_G',
							'T4,Plat_Inf_D',
							'T4,Plat_Inf_Ant',
							'T4,Plat_Inf_Post',
							'T4,Centroid_G',
							'T4,Centroid_D',
							'T5,Plat_Sup_G',
							'T5,Plat_Sup_D',
							'T5,Plat_Sup_Ant',
							'T5,Plat_Sup_Post',
							'T5,Plat_Inf_G',
							'T5,Plat_Inf_D',
							'T5,Plat_Inf_Ant',
							'T5,Plat_Inf_Post',
							'T5,Centroid_G',
							'T5,Centroid_D',
							'T6,Plat_Sup_G',
							'T6,Plat_Sup_D',
							'T6,Plat_Sup_Ant',
							'T6,Plat_Sup_Post',
							'T6,Plat_Inf_G',
							'T6,Plat_Inf_D',
							'T6,Plat_Inf_Ant',
							'T6,Plat_Inf_Post',
							'T6,Centroid_G',
							'T6,Centroid_D',
							'T7,Plat_Sup_G',
							'T7,Plat_Sup_D',
							'T7,Plat_Sup_Ant',
							'T7,Plat_Sup_Post',
							'T7,Plat_Inf_G',
							'T7,Plat_Inf_D',
							'T7,Plat_Inf_Ant',
							'T7,Plat_Inf_Post',
							'T7,Centroid_G',
							'T7,Centroid_D',
							'T8,Plat_Sup_G',
							'T8,Plat_Sup_D',
							'T8,Plat_Sup_Ant',
							'T8,Plat_Sup_Post',
							'T8,Plat_Inf_G',
							'T8,Plat_Inf_D',
							'T8,Plat_Inf_Ant',
							'T8,Plat_Inf_Post',
							'T8,Centroid_G',
							'T8,Centroid_D',
							'T9,Plat_Sup_G',
							'T9,Plat_Sup_D',
							'T9,Plat_Sup_Ant',
							'T9,Plat_Sup_Post',
							'T9,Plat_Inf_G',
							'T9,Plat_Inf_D',
							'T9,Plat_Inf_Ant',
							'T9,Plat_Inf_Post',
							'T9,Centroid_G',
							'T9,Centroid_D',
							'T10,Plat_Sup_G',
							'T10,Plat_Sup_D',
							'T10,Plat_Sup_Ant',
							'T10,Plat_Sup_Post',
							'T10,Plat_Inf_G',
							'T10,Plat_Inf_D',
							'T10,Plat_Inf_Ant',
							'T10,Plat_Inf_Post',
							'T10,Centroid_G',
							'T10,Centroid_D',
							'T11,Plat_Sup_G',
							'T11,Plat_Sup_D',
							'T11,Plat_Sup_Ant',
							'T11,Plat_Sup_Post',
							'T11,Plat_Inf_G',
							'T11,Plat_Inf_D',
							'T11,Plat_Inf_Ant',
							'T11,Plat_Inf_Post',
							'T11,Centroid_G',
							'T11,Centroid_D',
							'T12,Plat_Sup_G',
							'T12,Plat_Sup_D',
							'T12,Plat_Sup_Ant',
							'T12,Plat_Sup_Post',
							'T12,Plat_Inf_G',
							'T12,Plat_Inf_D',
							'T12,Plat_Inf_Ant',
							'T12,Plat_Inf_Post',
							'T12,Centroid_G',
							'T12,Centroid_D',
							'L1,Plat_Sup_G',
							'L1,Plat_Sup_D',
							'L1,Plat_Sup_Ant',
							'L1,Plat_Sup_Post',
							'L1,Plat_Inf_G',
							'L1,Plat_Inf_D',
							'L1,Plat_Inf_Ant',
							'L1,Plat_Inf_Post',
							'L1,Centroid_G',
							'L1,Centroid_D',
							'L2,Plat_Sup_G',
							'L2,Plat_Sup_D',
							'L2,Plat_Sup_Ant',
							'L2,Plat_Sup_Post',
							'L2,Plat_Inf_G',
							'L2,Plat_Inf_D',
							'L2,Plat_Inf_Ant',
							'L2,Plat_Inf_Post',
							'L2,Centroid_G',
							'L2,Centroid_D',
							'L3,Plat_Sup_G',
							'L3,Plat_Sup_D',
							'L3,Plat_Sup_Ant',
							'L3,Plat_Sup_Post',
							'L3,Plat_Inf_G',
							'L3,Plat_Inf_D',
							'L3,Plat_Inf_Ant',
							'L3,Plat_Inf_Post',
							'L3,Centroid_G',
							'L3,Centroid_D',
							'L4,Plat_Sup_G',
							'L4,Plat_Sup_D',
							'L4,Plat_Sup_Ant',
							'L4,Plat_Sup_Post',
							'L4,Plat_Inf_G',
							'L4,Plat_Inf_D',
							'L4,Plat_Inf_Ant',
							'L4,Plat_Inf_Post',
							'L4,Centroid_G',
							'L4,Centroid_D',
							'L5,Plat_Sup_G',
							'L5,Plat_Sup_D',
							'L5,Plat_Sup_Ant',
							'L5,Plat_Sup_Post',
							'L5,Plat_Inf_G',
							'L5,Plat_Inf_D',
							'L5,Plat_Inf_Ant',
							'L5,Plat_Inf_Post',
							'L5,Centroid_G',
							'L5,Centroid_D',
							'S1,Plat_Sup_Ant',
							'S1,Plat_Sup_Post',
							'Hips,Hip_G',
							'Hips,Hip_D']

                out = open(self.dirname + '/'+ os.path.splitext(os.path.basename(i))[0][:-2] + '.txt', 'w')
                out.write('X: anteroposterior coordinate (lat image); Y: laterolateral coordinate (ap image); Z: craniocaudal coordinate (shared)')
                #line without bug
                #out.write('X: anteroposterior coordinate (lat image); Y: laterolateral coordinate (ap image); Z: craniocaudal coordinate (shared)\n')
                for j in range(landmarks_lat.shape[0]):
                    if j % 2 == 0:
                        out.write('{},{},{},{}\n'.format(labels[j], float(landmarks_lat[j, 0]), float(landmarks_ap[j, 0]), float((landmarks_lat[j, 1] + landmarks_ap[j, 1]) / 2)))
                    else:
                        out.write('{},{},{},{}\n'.format(labels[j], float(landmarks_lat[j, 0]), float(landmarks_ap[j, 0]), float((landmarks_lat[j, 1] + landmarks_ap[j, 1]) / 2)))
                out.close()

                count = k/length*100
                time.sleep(1)
                self.countChanged.emit(float(count))

        create_dataframe(self.dirname)

class Actions(QDialog):
    """
    Simple dialog that consists of a Progress Bar and a Button.
    Clicking on the button results in the start of a timer and
    updates the progress bar.
    """
    def __init__(self, model, dirname):
        super().__init__()
        
        self.initUI()
        self.model = model
        self.dirname = dirname
        
        
    def initUI(self):

        self.setWindowModality(QtCore.Qt.ApplicationModal)



        self.setWindowTitle('Progress Bar')

        self.right_box = QVBoxLayout()

        self.progress = QProgressBar(self)
        self.progress.setGeometry(0, 0, 300, 25)
        self.n = 100
        value = 0
        self.progress.setValue(value * self.n)
        self.progress.setMaximum(100 * self.n)
        # displaying the decimal value 
        self.progress.setFormat("%.02f %%" % value)
        self.right_box.addWidget(self.progress)

        self.label = QLabel(self)
        self.label.setGeometry(0, 0, 300, 25)
        self.label.setAlignment(QtCore.Qt.AlignCenter)
        self.label.setText('Processing...')
        self.right_box.addWidget(self.label)
        
        self.show()

        #self.button.clicked.connect(self.onProgressStart)
        

    def onProgressStart(self):
        self.calc = External(self.model, self.dirname)
        self.calc.countChanged.connect(self.onCountChanged)
        self.calc.start()

    def onCountChanged(self, value):
        self.progress.setValue(value * self.n)
        self.progress.setFormat("%.02f %%" % value)
        if value == 100.00:
            self.label.setText('Completed!!!')
            return True



def init_model():
    #self.device = torch.device("cuda:0" if torch.cuda.is_available() else "cpu")
    #self.device = torch.device("cpu")
    '''model = network.CoordRegressionNetwork(n_locations = 348, in_channels = 128)
    model.load_state_dict(torch.load(resource_path('25_nov_StackedHourGlass_2_stacks_dsnt_1200_imgs.pth'), map_location = {'cuda:0': 'cpu'}))'''
    #Hourglass with 4 stacks
    model = network_4_stacks.CoordRegressionNetwork()
    model.load_state_dict(torch.load(resource_path('3_dic_StackedHourGlass_2_stacks_no_dsnt_1200_imgs.pth'), map_location = {'cuda:0': 'cpu'}))
    #model = model.to(self.device)
    model.eval()
    return model

def create_dataframe(dirname):
        angles = []
        for txt in glob.glob(dirname +"/*.txt"):
            name = os.path.splitext(os.path.basename(txt))[0]
            with open(txt, 'r') as f_pred:
                file_pred = f_pred.readlines()    
    
    
    
            #predicted angles
            L1_L5, L1_S1, T4_T12, SS, PI, PT = angles_from_file(file_pred)
            
            c_sup, c_inf = coordinates(file_pred)
            
            m_sup, m_inf = coefficients(c_sup, c_inf)

            vertebrae = ['T1', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'T8', 'T9', 'T10', 'T11', 'T12', 'L1', 'L2', 'L3', 'L4', 'L5', 'S1']
            cobb, v = Cobb(m_sup, m_inf, vertebrae)
            
            
            
            angles.append([name, cobb, v, L1_L5, L1_S1, T4_T12, SS, PI, PT])
    

        data = pd.DataFrame(angles, columns=['examination_ID', 'Cobb_angle', 'vertbrae Cobb curve', 'L1-L5', 'L1_S1', 'T4_T12', 'SS', 'PI', 'PT'])

        data.to_csv(dirname + '/' + 'dataframe_angles_predicted.csv', index = False)     

def resource_path(relative_path):
    """ Get absolute path to resource, works for dev and for PyInstaller """
    try:
        # PyInstaller creates a temp folder and stores path in _MEIPASS
        base_path = sys._MEIPASS
    except Exception:
        base_path = os.path.abspath(".")

    return os.path.join(base_path, relative_path)     
                
    


def main():
    
    app = QApplication(sys.argv)
    window = Main()
    #window.show()

    sys.exit(app.exec_())



if __name__ == '__main__':
    main()