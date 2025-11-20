import os
import sys
import cv2
import numpy as np
from PyQt5.QtGui import *
from PyQt5.QtWidgets import *
from PyQt5.QtCore import *
from PyQt5.QtPrintSupport import *
import configparser
import status
import spine_points
import paint_interactive
import paint_wholespine
import previewwindow

class MainWindow(QMainWindow):

    def __init__(self, *args, **kwargs):
        super(MainWindow, self).__init__(*args, **kwargs)

        config = configparser.ConfigParser()
        config.read('config.txt')

        self.data_folder = config['data']['path']
        self.painter_size = int(config['graphics']['widget_size'])
        self.padding = int(config['internals']['padding'])
        self.image_type = str(config['data']['format'])
        self.files_txt = []
        self.files_c = []
        self.files_l = []

        self.scale_factor = 1.
        self.crop_scale_factor = 1.5

        self.brightness = 0
        self.contrast = 0

        self.status = status.status()

        self.layout = QVBoxLayout()
        container = QWidget()
        container.setLayout(self.layout)
        self.setCentralWidget(container)
        self.layout.setContentsMargins(10, 10, 10, 10)

        self.hbox = QHBoxLayout()
        self.layout.addLayout(self.hbox)

        self.image_box = QVBoxLayout()
        self.hbox.addLayout(self.image_box)

        self.imageap = paint_wholespine.paint_wholespine(painter_size=self.painter_size)
        self.image_box.addWidget(self.imageap)

        self.imagelat = paint_wholespine.paint_wholespine(painter_size=self.painter_size)
        self.image_box.addWidget(self.imagelat)

        fixedfont = QFontDatabase.systemFont(QFontDatabase.FixedFont)
        fixedfont.setPointSize(13) # Edited D.L (from 12 to 13)

        self.crop_box = QHBoxLayout()
        self.hbox.addLayout(self.crop_box)

        self.cropap = paint_interactive.paint_interactive()
        self.cropap.setFixedSize(self.painter_size, self.painter_size)
        self.crop_box.addWidget(self.cropap)

        self.croplat = paint_interactive.paint_interactive()
        self.croplat.setFixedSize(self.painter_size, self.painter_size)
        self.crop_box.addWidget(self.croplat)

        self.cropap.twin_widget = self.croplat
        self.croplat.twin_widget = self.cropap

        self.lower_box = QHBoxLayout()
        self.layout.addLayout(self.lower_box)

        self.right_box = QVBoxLayout()
        self.hbox.addLayout(self.right_box)

        self.next = QPushButton("Next", self)
        self.lower_box.addWidget(self.next)
        self.next.clicked.connect(self.next_pressed)
        self.next.setFont(fixedfont)

        self.previous = QPushButton("Previous", self)
        self.lower_box.addWidget(self.previous)
        self.previous.clicked.connect(self.previous_pressed)
        self.previous.setFont(fixedfont)

        self.next_spine = QPushButton("Next spine", self)
        self.lower_box.addWidget(self.next_spine)
        self.next_spine.clicked.connect(self.next_spine_pressed)
        self.next_spine.setFont(fixedfont)

        self.skip = QPushButton("Skip", self)
        self.lower_box.addWidget(self.skip)
        self.skip.clicked.connect(self.skip_pressed)
        self.skip.setFont(fixedfont)

        self.preview = QPushButton("Preview", self)
        self.right_box.addWidget(self.preview)
        self.preview.clicked.connect(self.preview_pressed)
        self.preview.setFont(fixedfont)

        self.zoom_fit = QPushButton("Zoom fit", self)
        self.right_box.addWidget(self.zoom_fit)
        self.zoom_fit.clicked.connect(self.zoom_fit_pressed)
        self.zoom_fit.setFont(fixedfont)

        self.zoom_plus = QPushButton("Zoom +", self)
        self.right_box.addWidget(self.zoom_plus)
        self.zoom_plus.clicked.connect(self.zoom_plus_pressed)
        self.zoom_plus.setFont(fixedfont)

        self.zoom_minus = QPushButton("Zoom -", self)
        self.right_box.addWidget(self.zoom_minus)
        self.zoom_minus.clicked.connect(self.zoom_minus_pressed)
        self.zoom_minus.setFont(fixedfont)

        self.autolevels = QPushButton("Auto levels", self)
        self.right_box.addWidget(self.autolevels)
        self.autolevels.clicked.connect(self.autolevels_pressed)
        self.autolevels.setFont(fixedfont)

        self.brightness_plus = QPushButton("Brightness +", self)
        self.right_box.addWidget(self.brightness_plus)
        self.brightness_plus.clicked.connect(self.brightness_plus_pressed)
        self.brightness_plus.setFont(fixedfont)

        self.brightness_minus = QPushButton("Brightness -", self)
        self.right_box.addWidget(self.brightness_minus)
        self.brightness_minus.clicked.connect(self.brightness_minus_pressed)
        self.brightness_minus.setFont(fixedfont)

        self.contrast_plus = QPushButton("Contrast +", self)
        self.right_box.addWidget(self.contrast_plus)
        self.contrast_plus.clicked.connect(self.contrast_plus_pressed)
        self.contrast_plus.setFont(fixedfont)

        self.contrast_minus = QPushButton("Contrast -", self)
        self.right_box.addWidget(self.contrast_minus)
        self.contrast_minus.clicked.connect(self.contrast_minus_pressed)
        self.contrast_minus.setFont(fixedfont)

        self.show_vertebra_below = QCheckBox("Show vertebra below", self)
        self.show_vertebra_below.setChecked(False)
        self.right_box.addWidget(self.show_vertebra_below)
        self.show_vertebra_below.stateChanged.connect(self.show_vertebra_below_changed)
        self.state_show_vertebra_below = False

        self.show_vertebra_above = QCheckBox("Show vertebra above", self)
        self.show_vertebra_above.setChecked(False)
        self.right_box.addWidget(self.show_vertebra_above)
        self.show_vertebra_above.stateChanged.connect(self.show_vertebra_above_changed)
        self.state_show_vertebra_above = False

        self.read_folder()
        self.counter = 0
        self.setFixedSize(int(float(self.painter_size * 2.5)), int(float(self.painter_size * 1.2)))

        self.show()
        self.next_spine_pressed()

    def preview_pressed(self):
        print("Preview\n")
        self.spine.render_image("preview_lines.png", self.data_folder + self.files_c[self.counter - 1], self.data_folder + self.files_l[self.counter - 1])
        self.spine.render_image_nolines("preview_nolines.png", self.data_folder + self.files_c[self.counter - 1], self.data_folder + self.files_l[self.counter - 1])
        pw = previewwindow.PreviewWindow(self)
        pw.exec_()
        return

    def show_vertebra_below_changed(self):
        if self.show_vertebra_below.isChecked() == True:
            self.state_show_vertebra_below = True
            self.cropap.show_vertebra_below = True
            self.croplat.show_vertebra_below = True
            self.update_images(new_vertebra = False)
            print("show vertebra below")
        else:
            self.state_show_vertebra_below = False
            self.cropap.show_vertebra_below = False
            self.croplat.show_vertebra_below = False
            self.update_images(new_vertebra = False)
            print("hide vertebra below")
        return

    def show_vertebra_above_changed(self):
        if self.show_vertebra_above.isChecked() == True:
            self.state_show_vertebra_above = True
            self.cropap.show_vertebra_above = True
            self.croplat.show_vertebra_above = True
            self.update_images(new_vertebra = False)
            print("show vertebra above")
        else:
            self.state_show_vertebra_above = False
            self.cropap.show_vertebra_above = False
            self.croplat.show_vertebra_above = False
            self.update_images(new_vertebra = False)
            print("hide vertebra above")
        return

    def brightness_plus_pressed(self):
        self.brightness += 10
        if self.brightness > 200:
            self.brightness = 200
        self.update_brightnesscontrast()
        return

    def brightness_minus_pressed(self):
        self.brightness -= 10
        if self.brightness < -200:
            self.brightness = -200
        self.update_brightnesscontrast()
        return

    def contrast_plus_pressed(self):
        self.contrast += 5
        if self.contrast > 100:
            self.contrast = 100
        self.update_brightnesscontrast()
        return

    def contrast_minus_pressed(self):
        self.contrast -= 5
        if self.contrast < -100:
            self.contrast = -100
        self.update_brightnesscontrast()
        return

    def autolevels_pressed(self):
        self.brightness = 0
        self.contrast = 0
        self.update_brightnesscontrast()
        return

    def zoom_fit_pressed(self):
        if self.status.get_vertebra()== 'S1':
            self.crop_scale_factor = 3.
        else:
            self.crop_scale_factor = 1.5
        print("Crop scale factor: {}\n".format(self.crop_scale_factor))
        self.update_images(new_vertebra = True)
        return

    def zoom_plus_pressed(self):
        self.crop_scale_factor -= 0.1
        print("Crop scale factor: {}\n".format(self.crop_scale_factor))
        self.update_images(new_vertebra = True)
        return

    def zoom_minus_pressed(self):
        self.crop_scale_factor += 0.1
        print("Crop scale factor: {}\n".format(self.crop_scale_factor))
        self.update_images(new_vertebra = True)
        return

    def update_brightnesscontrast(self):
        def apply_brightness_contrast(input_img, brightness = 0, contrast = 0):
            if brightness != 0:
                if brightness > 0:
                    shadow = brightness
                    highlight = 255
                else:
                    shadow = 0
                    highlight = 255 + brightness
                alpha_b = (highlight - shadow)/255
                gamma_b = shadow
                
                buf = cv2.addWeighted(input_img, alpha_b, input_img, 0, gamma_b)
            else:
                buf = input_img.copy()
            
            if contrast != 0:
                f = 131*(contrast + 127)/(127*(131-contrast))
                alpha_c = f
                gamma_c = 127*(1-f)
                
                buf = cv2.addWeighted(buf, alpha_c, buf, 0, gamma_c)
            return buf

        eq_c = cv2.imread("temp_c_nobc.png", cv2.IMREAD_GRAYSCALE)
        eq_l = cv2.imread("temp_l_nobc.png", cv2.IMREAD_GRAYSCALE)

        eq_c = apply_brightness_contrast(eq_c, self.brightness, self.contrast)
        eq_l = apply_brightness_contrast(eq_l, self.brightness, self.contrast)

        cv2.imwrite("temp_c.png", eq_c)
        cv2.imwrite("temp_l.png", eq_l)

        pix_cropap = QPixmap("temp_c.png")
        pix_croplat = QPixmap("temp_l.png")

        self.cropap.setPixmap(pix_cropap)
        self.croplat.setPixmap(pix_croplat) 

        self.cropap.draw_vertebra(0, self.status.get_vertebra(), self.status.get_region())
        self.croplat.draw_vertebra(1, self.status.get_vertebra(), self.status.get_region()) 

        return

    def read_folder(self):
        print("read folder!\n")
        for root, dirs, files in os.walk(self.data_folder):
            files.sort()
            for name_c in files:
                print(name_c)
                if name_c[-6:] == ('_C.' + self.image_type):
                    name_l = name_c[:-6] + '_L.' + self.image_type
                    name_s = name_c[:-6] + '_S.' + self.image_type
                    name_txt = name_c[:-6] + '.txt'
                    name_txt_edited = name_c[:-6] + '_edited.txt'
                    name_txt_skipped = name_c[:-6] + '_skipped.txt'

                    if ((os.path.isfile(self.data_folder + name_l)) or (os.path.isfile(self.data_folder + name_s))) and (os.path.isfile(self.data_folder + name_txt)) and (not(os.path.isfile(self.data_folder + name_txt_edited))) and (not(os.path.isfile(self.data_folder + name_txt_skipped))):
                        self.files_txt.append(name_txt)
                        self.files_c.append(name_c)
                        if os.path.isfile(self.data_folder + name_l):
                            self.files_l.append(name_l)
                        else:
                            self.files_l.append(name_s)
        return

    def get_images_size(self, fn_img_c, fn_img_l):
        img_c = cv2.imread(fn_img_c, cv2.IMREAD_COLOR)
        img_l = cv2.imread(fn_img_l, cv2.IMREAD_COLOR)
        size_x = img_l.shape[1]
        size_y = img_c.shape[1]
        size_z = img_l.shape[0]
        return size_x, size_y, size_z

    def update_images(self, new_vertebra = True, reset_zoom = False):

        def apply_brightness_contrast(input_img, brightness = 0, contrast = 0):
            if brightness != 0:
                if brightness > 0:
                    shadow = brightness
                    highlight = 255
                else:
                    shadow = 0
                    highlight = 255 + brightness
                alpha_b = (highlight - shadow)/255
                gamma_b = shadow
                
                buf = cv2.addWeighted(input_img, alpha_b, input_img, 0, gamma_b)
            else:
                buf = input_img.copy()
            
            if contrast != 0:
                f = 131*(contrast + 127)/(127*(131-contrast))
                alpha_c = f
                gamma_c = 127*(1-f)
                
                buf = cv2.addWeighted(buf, alpha_c, buf, 0, gamma_c)
            return buf

        if reset_zoom == True:
            if self.status.get_vertebra()== 'S1':
                self.crop_scale_factor = 3.
            else:
                self.crop_scale_factor = 1.5

        if new_vertebra == True:
            def remove_padding(px, py, pz):
                px -= self.padding
                py -= self.padding
                pz -= self.padding
                return px, py, pz

            size_x, size_y, size_z = self.get_images_size(self.data_folder + self.files_c[self.counter - 1], self.data_folder + self.files_l[self.counter - 1])
            

            min_x, max_x, min_y, max_y, min_z, max_z, factor = self.spine.get_crop_region(self.status.get_vertebra(), self.crop_scale_factor, size_x, size_y, size_z)
            self.crop_scale_factor = factor
            self.scale_factor = float(self.painter_size) / (float(max_x) - float(min_x)) 
            
            print("scale factor {}\n".format(self.scale_factor))
            self.cropap.scale_factor = self.scale_factor
            self.croplat.scale_factor = self.scale_factor

            cv_img_c = cv2.imread(self.data_folder + self.files_c[self.counter - 1], cv2.IMREAD_GRAYSCALE)
            cv_img_l = cv2.imread(self.data_folder + self.files_l[self.counter - 1], cv2.IMREAD_GRAYSCALE)

            cv_img_c = np.pad(cv_img_c, pad_width=self.padding, mode='constant', constant_values=0)
            cv_img_l = np.pad(cv_img_l, pad_width=self.padding, mode='constant', constant_values=0)

            cv_img_c_cropped = cv_img_c[min_z:max_z, min_y:max_y]
            cv_img_l_cropped = cv_img_l[min_z:max_z, min_x:max_x]
            resized_c = cv2.resize(cv_img_c_cropped, (self.painter_size, self.painter_size), interpolation = cv2.INTER_AREA)
            resized_l = cv2.resize(cv_img_l_cropped, (self.painter_size, self.painter_size), interpolation = cv2.INTER_AREA)

            eq_c = cv2.normalize(resized_c, None, alpha=0, beta=1, norm_type=cv2.NORM_MINMAX, dtype=cv2.CV_32F)
            eq_c = np.clip(eq_c, 0, 1)
            eq_c = (255*eq_c).astype(np.uint8)

            eq_l = cv2.normalize(resized_l, None, alpha=0, beta=1, norm_type=cv2.NORM_MINMAX, dtype=cv2.CV_32F)
            eq_l = np.clip(eq_l, 0, 1)
            eq_l = (255*eq_l).astype(np.uint8)

            cv2.imwrite("temp_c_nobc.png", eq_c)
            cv2.imwrite("temp_l_nobc.png", eq_l)

            eq_c = apply_brightness_contrast(eq_c, self.brightness, self.contrast)
            eq_l = apply_brightness_contrast(eq_l, self.brightness, self.contrast)

            cv2.imwrite("temp_c.png", eq_c)
            cv2.imwrite("temp_l.png", eq_l)

            self.cropap.minx = min_x 
            self.cropap.miny = min_y 
            self.cropap.minz = min_z 

            self.croplat.minx = min_x 
            self.croplat.miny = min_y 
            self.croplat.minz = min_z 

            if (self.status.get_vertebra() != 'Hips') and (self.status.get_vertebra() != 'S1'):
                v_id = self.spine.get_vertebra_id(self.status.get_vertebra())
                psa_x, psa_y, psa_z = self.spine.vertebrae[v_id].get_point_coords('Plat_Sup_Ant')
                psp_x, psp_y, psp_z = self.spine.vertebrae[v_id].get_point_coords('Plat_Sup_Post')
                psg_x, psg_y, psg_z = self.spine.vertebrae[v_id].get_point_coords('Plat_Sup_G')
                psd_x, psd_y, psd_z = self.spine.vertebrae[v_id].get_point_coords('Plat_Sup_D')
                pia_x, pia_y, pia_z = self.spine.vertebrae[v_id].get_point_coords('Plat_Inf_Ant')
                pip_x, pip_y, pip_z = self.spine.vertebrae[v_id].get_point_coords('Plat_Inf_Post')
                pig_x, pig_y, pig_z = self.spine.vertebrae[v_id].get_point_coords('Plat_Inf_G')
                pid_x, pid_y, pid_z = self.spine.vertebrae[v_id].get_point_coords('Plat_Inf_D')

                sp_x, sp_y, sp_z = self.spine.vertebrae[v_id].get_point_coords('Spinous_Process') # Edited D.L (added)


                psa_x, psa_y, psa_z = remove_padding(psa_x, psa_y, psa_z)
                psp_x, psp_y, psp_z = remove_padding(psp_x, psp_y, psp_z)
                psg_x, psg_y, psg_z = remove_padding(psg_x, psg_y, psg_z)
                psd_x, psd_y, psd_z = remove_padding(psd_x, psd_y, psd_z)
                pia_x, pia_y, pia_z = remove_padding(pia_x, pia_y, pia_z)
                pip_x, pip_y, pip_z = remove_padding(pip_x, pip_y, pip_z)
                pig_x, pig_y, pig_z = remove_padding(pig_x, pig_y, pig_z)
                pid_x, pid_y, pid_z = remove_padding(pid_x, pid_y, pid_z)

                sp_x, sp_y, sp_z = remove_padding(sp_x, sp_y, sp_z) # Edited D.L (added)

                self.imageap.set_size_image(size_y, size_z)
                self.imagelat.set_size_image(size_x, size_z)

                self.imageap.show_rectangle(psg_y, psg_z, psd_y, psd_z, pid_y, pid_z, pig_y, pig_z)
                self.imagelat.show_rectangle(psa_x, psa_z, psp_x, psp_z, pip_x, pip_z, pia_x, pia_z)

                self.imageap.show_point(sp_y, sp_z) # Edited D.L (added)
                self.imagelat.show_point(sp_x, sp_z) # Edited D.L (added)

            if self.status.get_vertebra() == 'S1':
                v_id = self.spine.get_vertebra_id(self.status.get_vertebra())
                psa_x, psa_y, psa_z = self.spine.vertebrae[v_id].get_point_coords('Plat_Sup_Ant')
                psp_x, psp_y, psp_z = self.spine.vertebrae[v_id].get_point_coords('Plat_Sup_Post')

                psa_x, psa_y, psa_z = remove_padding(psa_x, psa_y, psa_z)
                psp_x, psp_y, psp_z = remove_padding(psp_x, psp_y, psp_z)

                self.imageap.set_size_image(size_y, size_z)
                self.imagelat.set_size_image(size_x, size_z)

                self.imageap.show_line(psa_y, psa_z, psp_y, psp_z)
                self.imagelat.show_line(psa_x, psa_z, psp_x, psp_z)

            if self.status.get_vertebra() == 'Hips':
                v_id = self.spine.get_vertebra_id(self.status.get_vertebra())
                p1_x, p1_y, p1_z = self.spine.vertebrae[v_id].get_point_coords('Hip_G')
                p2_x, p2_y, p2_z = self.spine.vertebrae[v_id].get_point_coords('Hip_D')

                p1_x, p1_y, p1_z = remove_padding(p1_x, p1_y, p1_z)
                p2_x, p2_y, p2_z = remove_padding(p2_x, p2_y, p2_z)

                self.imageap.set_size_image(size_y, size_z)
                self.imagelat.set_size_image(size_x, size_z)

                self.imageap.show_line(p1_y, p1_z, p2_y, p2_z)
                self.imagelat.show_line(p1_x, p1_z, p2_x, p2_z)

        pix_cropap = QPixmap("temp_c.png")
        pix_croplat = QPixmap("temp_l.png")

        self.cropap.setPixmap(pix_cropap)
        self.croplat.setPixmap(pix_croplat)  

        self.cropap.draw_vertebra(0, self.status.get_vertebra(), self.status.get_region())
        self.croplat.draw_vertebra(1, self.status.get_vertebra(), self.status.get_region())

        return

    def next_pressed(self):
        new_vertebra = True
        new_spine = False
        reset_zoom = False
        last_region = False
        if self.counter != 0:
            new_vertebra, last_region = self.status.next()
            reset_zoom = new_vertebra
            self.previous.setEnabled(True)

        if last_region == True:
            self.next.setEnabled(False)
        
        print("Vertebra {}, region {}\n".format(self.status.get_vertebra(), self.status.get_region()))
        self.update_images(new_vertebra, reset_zoom)

        self.setWindowTitle(self.status.get_vertebra() + ' --- ' + str(self.counter) + ' in ' + str(len(self.files_txt)) + ' --- ' + self.files_txt[self.counter - 1])
        return

    def next_spine_pressed(self):
        if self.counter != 0:
            filename_txt_edited = self.data_folder + self.files_txt[self.counter - 1][:-4] + '_edited.txt'
            self.spine.write_to_file(filename_txt_edited)
            filename_render = self.data_folder + self.files_txt[self.counter - 1][:-4] + '_edited.' + self.image_type
            self.spine.render_image(filename_render, self.data_folder + self.files_c[self.counter - 1], self.data_folder + self.files_l[self.counter - 1])

        self.counter += 1
        self.status.current_region = 0
        self.status.current_vertebra = 0

        self.spine = spine_points.spine_points(self.data_folder + self.files_txt[self.counter - 1], self.padding)  
        self.cropap.spine = self.spine          
        self.croplat.spine = self.spine  

        self.imageap.set_image(self.data_folder + self.files_c[self.counter - 1])
        self.imagelat.set_image(self.data_folder + self.files_l[self.counter - 1])  

        self.previous.setEnabled(False)
        print("Vertebra {}, region {}\n".format(self.status.get_vertebra(), self.status.get_region()))
        self.update_images(True, True)

        self.setWindowTitle(self.status.get_vertebra() + ' --- ' + str(self.counter) + ' in ' + str(len(self.files_txt)) + ' --- ' + self.files_txt[self.counter - 1])

        self.previous.setEnabled(False)
        self.next.setEnabled(True)

        return


    def previous_pressed(self):
        new_vertebra = self.status.previous()
        reset_zoom = new_vertebra
        if (self.status.get_vertebra() == 'Hips'):
            self.previous.setEnabled(False)
        self.next.setEnabled(True)

        print("Vertebra {}, region {}\n".format(self.status.get_vertebra(), self.status.get_region()))
        self.update_images(new_vertebra, reset_zoom)
        self.setWindowTitle(self.status.get_vertebra() + ' --- ' +str(self.counter) + ' in ' + str(len(self.files_txt)) + ' --- ' + self.files_txt[self.counter - 1])
        return

    def skip_pressed(self):
        self.status.skip_spine()
            
        filename_txt_skipped = self.data_folder + self.files_txt[self.counter - 1][:-4] + '_skipped.txt'
        self.spine.write_skipped(filename_txt_skipped)
            
        self.counter += 1
        self.imageap.set_image(self.data_folder + self.files_c[self.counter - 1])
        self.imagelat.set_image(self.data_folder + self.files_l[self.counter - 1]) 
        
        self.spine = spine_points.spine_points(self.data_folder + self.files_txt[self.counter - 1], self.padding)  
        self.cropap.spine = self.spine          
        self.croplat.spine = self.spine          

        print("Vertebra {}, region {}\n".format(self.status.get_vertebra(), self.status.get_region()))
        self.update_images(True, True)
        self.setWindowTitle(self.status.get_vertebra() + ' --- ' +str(self.counter) + ' in ' + str(len(self.files_txt)) + ' --- ' + self.files_txt[self.counter - 1])

        self.previous.setEnabled(False)
        self.next.setEnabled(True)

        return

if __name__ == '__main__':
    app = QApplication(sys.argv)
    app.setApplicationName("EOS Fast Reconstruction")

    window = MainWindow()
    app.exec_()
