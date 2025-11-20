from PyQt5.QtGui import *
from PyQt5.QtWidgets import *
from PyQt5.QtCore import *
from PyQt5.QtPrintSupport import *
import numpy as np
import math

class paint_interactive(QLabel):
    def __init__(self, parent = None):
        QLabel.__init__(self, parent)
        self.spine = None
        self.minx = 0
        self.miny = 0
        self.minz = 0
        self.scale_factor = 1.
        self.points = np.zeros((13, 2), dtype = float)        # edited D.L (from 12 to 13)
        self.points_below = np.zeros((13, 2), dtype = float)  # edited D.L (from 12 to 13)
        self.points_above = np.zeros((13, 2), dtype = float)  # edited D.L (from 12 to 13)

        self.points_names = ['Plat_Sup_G', 'Plat_Sup_D', 'Plat_Sup_Ant', 'Plat_Sup_Post',
                       'Plat_Inf_G', 'Plat_Inf_D', 'Plat_Inf_Ant', 'Plat_Inf_Post',
                       'Centroid_G', 'Centroid_D', 'Spinous_Process',
                       'Hip_G', 'Hip_D'] # Edited D.L (added 'Spin..')
        self.left_clicked = False
        self.dot_x = 0
        self.dot_y = 0
        self.render_point = False
        self.render_vertebra = False
        self.vertebra_name = 'L5'
        self.view = 0
        self.region_name ='l_ep'
        self.point_rendered = 0
        self.twin_widget = None
        self.hide_lines = False
        self.render_region = False
        self.show_vertebra_below = False
        self.show_vertebra_above = False
        self.show_points_names = False

    ### Edited D.L
    def check_point_clicked(self, px, py):
        print("px: {}, py: {}\n".format(px, py))
        min_distance = 1.E8
        best = 0
        distance = 0
        if (self.vertebra_name != 'Hips') and (self.vertebra_name != 'S1'):
            for i in range(11): # (range(11) was range(10))
                distance = math.sqrt((px - self.points[i,0])*(px - self.points[i,0]) + (py - self.points[i,1])*(py - self.points[i,1]))
                if distance < min_distance:
                    min_distance = distance
                    best = i

## edited D.L (was range (10, 12))
        if self.vertebra_name == 'Hips':
            for i in range(11, 13):
                distance = math.sqrt((px - self.points[i,0])*(px - self.points[i,0]) + (py - self.points[i,1])*(py - self.points[i,1]))
                if distance < min_distance:
                    min_distance = distance
                    best = i

        if self.vertebra_name == 'S1':
            for i in range(2, 4):
                distance = math.sqrt((px - self.points[i,0])*(px - self.points[i,0]) + (py - self.points[i,1])*(py - self.points[i,1]))
                if distance < min_distance:
                    min_distance = distance
                    best = i

        return best, min_distance

    def check_point_in_region(self, npoint):
        if (self.vertebra_name == 'Hips') or (self.vertebra_name == 'S1'):
            return 1
        else:
            p_name = self.points_names[npoint]
            points_u_ep = ['Plat_Sup_G', 'Plat_Sup_D', 'Plat_Sup_Ant', 'Plat_Sup_Post']   
            points_l_ep = ['Plat_Inf_G', 'Plat_Inf_D', 'Plat_Inf_Ant', 'Plat_Inf_Post']   
            points_ped = ['Centroid_G', 'Centroid_D']
            points_proc = ['Spinous_Process'] # Edited D.L (added)

            if (self.region_name == 'u_ep') and (p_name in points_u_ep):
                return 1   
            if (self.region_name == 'l_ep') and (p_name in points_l_ep):
                return 1   
            if (self.region_name == 'ped') and (p_name in points_ped):
                return 1
            if (self.region_name == 'proc') and (p_name in points_proc): # Edited D.L (added)
                return 1
            return 0

    def mousePressEvent(self, event):
        if (event.button() == Qt.LeftButton) and (not(event.modifiers() & Qt.ControlModifier)):
            print("mouse clicked!\n")
            print(event.pos())
            closest, distance = self.check_point_clicked(event.pos().x(), event.pos().y())
            print("closest point: {}, distance: {}\n".format(self.points_names[closest], distance))
            self.left_clicked = True
            if distance < 6.:
                if self.check_point_in_region(closest):
                    self.dot_x = event.pos().x()
                    self.dot_y = event.pos().y()
                    self.render_point = True
                    self.point_rendered = closest
                    self.update_painter()

        if (event.button() == Qt.LeftButton) and (event.modifiers() & Qt.ControlModifier) and (self.vertebra_name != 'S1') and (self.vertebra_name != 'Hips'):
            self.render_region = True
            self.initial_point_x = event.pos().x()
            self.initial_point_y = event.pos().y()
            self.displacement_x = 0
            self.displacement_y = 0
            self.update_painter()

        if (event.button() == Qt.RightButton) and (not(event.modifiers() & Qt.ControlModifier)):
            self.hide_lines = True
            self.update_painter()

        if (event.button() == Qt.RightButton) and (event.modifiers() & Qt.ControlModifier):
            self.show_points_names = True
            self.update_painter()
        return

    def mouseMoveEvent(self, event):
        if self.render_point == True:
            self.dot_x = event.pos().x()
            self.dot_y = event.pos().y()
            self.update_painter()

        if (self.render_region == True) and (event.modifiers() & Qt.ControlModifier):
            self.displacement_x = event.pos().x() - self.initial_point_x
            self.displacement_y = event.pos().y() - self.initial_point_y 
            self.update_painter()
        elif self.render_region == True:
            self.render_region = False
            self.update_points_region()
            self.update_painter()
            self.twin_widget.update_painter()

    def update_painter(self):
        if self.view == 0:
            pix = QPixmap("temp_c.png")
        else:
            pix = QPixmap("temp_l.png")
        self.setPixmap(pix)
        painter = QPainter(self.pixmap())

        if (self.render_vertebra == True) and (self.hide_lines == False) and (self.render_region == False):
            v_id = self.spine.get_vertebra_id(self.vertebra_name)
            v = self.spine.vertebrae[v_id]

            if (self.vertebra_name == 'Hips'):
                self.points[self.get_point_id('Hip_G'), 0], self.points[self.get_point_id('Hip_G'), 1] = self.transform_point(self.view, v.get_point_coords('Hip_G'))
                self.points[self.get_point_id('Hip_D'), 0], self.points[self.get_point_id('Hip_D'), 1] = self.transform_point(self.view, v.get_point_coords('Hip_D'))
                painter.setPen(Qt.red)
                painter.drawLine(self.points[self.get_point_id('Hip_G'), 0], self.points[self.get_point_id('Hip_G'), 1], self.points[self.get_point_id('Hip_D'), 0], self.points[self.get_point_id('Hip_D'), 1])

            if (self.vertebra_name == 'S1'):
                self.points[self.get_point_id('Plat_Sup_Ant'), 0], self.points[self.get_point_id('Plat_Sup_Ant'), 1] = self.transform_point(self.view, v.get_point_coords('Plat_Sup_Ant'))
                self.points[self.get_point_id('Plat_Sup_Post'), 0], self.points[self.get_point_id('Plat_Sup_Post'), 1] = self.transform_point(self.view, v.get_point_coords('Plat_Sup_Post'))
                painter.setPen(Qt.red)
                painter.drawLine(self.points[self.get_point_id('Plat_Sup_Ant'), 0], self.points[self.get_point_id('Plat_Sup_Ant'), 1], self.points[self.get_point_id('Plat_Sup_Post'), 0], self.points[self.get_point_id('Plat_Sup_Post'), 1])
## edited D.L (was range 10)
            if (self.vertebra_name != 'Hips') and (self.vertebra_name != 'S1'):
                for i in range(11):
                    self.points[i, 0], self.points[i, 1] = self.transform_point(self.view, v.get_point_coords(self.points_names[i]))
            
                if self.region_name == 'u_ep':
                    painter.setPen(Qt.red)
                else:
                    painter.setPen(Qt.black)
                painter.drawLine(self.points[self.get_point_id('Plat_Sup_Ant'), 0], self.points[self.get_point_id('Plat_Sup_Ant'), 1], self.points[self.get_point_id('Plat_Sup_G'), 0], self.points[self.get_point_id('Plat_Sup_G'), 1])
                painter.drawLine(self.points[self.get_point_id('Plat_Sup_Ant'), 0], self.points[self.get_point_id('Plat_Sup_Ant'), 1], self.points[self.get_point_id('Plat_Sup_D'), 0], self.points[self.get_point_id('Plat_Sup_D'), 1])
                painter.drawLine(self.points[self.get_point_id('Plat_Sup_Post'), 0], self.points[self.get_point_id('Plat_Sup_Post'), 1], self.points[self.get_point_id('Plat_Sup_G'), 0], self.points[self.get_point_id('Plat_Sup_G'), 1])
                painter.drawLine(self.points[self.get_point_id('Plat_Sup_Post'), 0], self.points[self.get_point_id('Plat_Sup_Post'), 1], self.points[self.get_point_id('Plat_Sup_D'), 0], self.points[self.get_point_id('Plat_Sup_D'), 1])
                
                if self.region_name == 'l_ep':
                    painter.setPen(Qt.red)
                else:
                    painter.setPen(Qt.black)
                painter.drawLine(self.points[self.get_point_id('Plat_Inf_Ant'), 0], self.points[self.get_point_id('Plat_Inf_Ant'), 1], self.points[self.get_point_id('Plat_Inf_G'), 0], self.points[self.get_point_id('Plat_Inf_G'), 1])
                painter.drawLine(self.points[self.get_point_id('Plat_Inf_Ant'), 0], self.points[self.get_point_id('Plat_Inf_Ant'), 1], self.points[self.get_point_id('Plat_Inf_D'), 0], self.points[self.get_point_id('Plat_Inf_D'), 1])
                painter.drawLine(self.points[self.get_point_id('Plat_Inf_Post'), 0], self.points[self.get_point_id('Plat_Inf_Post'), 1], self.points[self.get_point_id('Plat_Inf_G'), 0], self.points[self.get_point_id('Plat_Inf_G'), 1])
                painter.drawLine(self.points[self.get_point_id('Plat_Inf_Post'), 0], self.points[self.get_point_id('Plat_Inf_Post'), 1], self.points[self.get_point_id('Plat_Inf_D'), 0], self.points[self.get_point_id('Plat_Inf_D'), 1])

                if self.region_name == 'ped':
                    painter.setPen(Qt.red)
                else:
                    painter.setPen(Qt.black)
                painter.drawLine(self.points[self.get_point_id('Centroid_G'), 0], self.points[self.get_point_id('Centroid_G'), 1], self.points[self.get_point_id('Centroid_D'), 0], self.points[self.get_point_id('Centroid_D'), 1])

                if self.region_name == 'proc':
                    pen = QPen(Qt.red)
                    pen.setWidth(5)
                    painter.setPen(pen)
                else:
                    pen = QPen(Qt.black)
                    pen.setWidth(5)
                    painter.setPen(pen)
                painter.drawPoint(self.points[self.get_point_id('Spinous_Process'), 0], self.points[self.get_point_id('Spinous_Process'), 1])

                if self.region_name == 'u_ep':
                    if self.view == 0:
                        painter.setPen(Qt.yellow)
                        painter.drawLine(self.points[self.get_point_id('Plat_Sup_G'), 0], self.points[self.get_point_id('Plat_Sup_G'), 1], self.points[self.get_point_id('Plat_Sup_D'), 0], self.points[self.get_point_id('Plat_Sup_D'), 1])
                    else:
                        painter.setPen(Qt.yellow)
                        painter.drawLine(self.points[self.get_point_id('Plat_Sup_Ant'), 0], self.points[self.get_point_id('Plat_Sup_Ant'), 1], self.points[self.get_point_id('Plat_Sup_Post'), 0], self.points[self.get_point_id('Plat_Sup_Post'), 1])
                elif self.region_name == 'l_ep':
                    if self.view == 0:
                        painter.setPen(Qt.yellow)
                        painter.drawLine(self.points[self.get_point_id('Plat_Inf_G'), 0], self.points[self.get_point_id('Plat_Inf_G'), 1], self.points[self.get_point_id('Plat_Inf_D'), 0], self.points[self.get_point_id('Plat_Inf_D'), 1])
                    else:
                        painter.setPen(Qt.yellow)
                        painter.drawLine(self.points[self.get_point_id('Plat_Inf_Ant'), 0], self.points[self.get_point_id('Plat_Inf_Ant'), 1], self.points[self.get_point_id('Plat_Inf_Post'), 0], self.points[self.get_point_id('Plat_Inf_Post'), 1])

        if (self.show_vertebra_below == True) and (self.hide_lines == False):
            if self.vertebra_name != 'Hips':
                v_id = self.spine.get_lower_vertebra_id(self.vertebra_name)
                v = self.spine.vertebrae[v_id]

                painter.setPen(Qt.darkBlue)
                if (v.name == 'Hips'):
                    self.points_below[self.get_point_id('Hip_G'), 0], self.points_below[self.get_point_id('Hip_G'), 1] = self.transform_point(self.view, v.get_point_coords('Hip_G'))
                    self.points_below[self.get_point_id('Hip_D'), 0], self.points_below[self.get_point_id('Hip_D'), 1] = self.transform_point(self.view, v.get_point_coords('Hip_D'))
                    painter.drawLine(self.points_below[self.get_point_id('Hip_G'), 0], self.points_below[self.get_point_id('Hip_G'), 1], self.points_below[self.get_point_id('Hip_D'), 0], self.points_below[self.get_point_id('Hip_D'), 1])

                if (v.name == 'S1'):
                    self.points_below[self.get_point_id('Plat_Sup_Ant'), 0], self.points_below[self.get_point_id('Plat_Sup_Ant'), 1] = self.transform_point(self.view, v.get_point_coords('Plat_Sup_Ant'))
                    self.points_below[self.get_point_id('Plat_Sup_Post'), 0], self.points_below[self.get_point_id('Plat_Sup_Post'), 1] = self.transform_point(self.view, v.get_point_coords('Plat_Sup_Post'))
                    painter.drawLine(self.points_below[self.get_point_id('Plat_Sup_Ant'), 0], self.points_below[self.get_point_id('Plat_Sup_Ant'), 1], self.points_below[self.get_point_id('Plat_Sup_Post'), 0], self.points_below[self.get_point_id('Plat_Sup_Post'), 1])

                if (v.name != 'Hips') and (v.name != 'S1'):
                    for i in range(11): # Edited D.L (range(10) to range(11))
                        self.points_below[i, 0], self.points_below[i, 1] = self.transform_point(self.view, v.get_point_coords(self.points_names[i]))
                
                    painter.drawLine(self.points_below[self.get_point_id('Plat_Sup_Ant'), 0], self.points_below[self.get_point_id('Plat_Sup_Ant'), 1], self.points_below[self.get_point_id('Plat_Sup_G'), 0], self.points_below[self.get_point_id('Plat_Sup_G'), 1])
                    painter.drawLine(self.points_below[self.get_point_id('Plat_Sup_Ant'), 0], self.points_below[self.get_point_id('Plat_Sup_Ant'), 1], self.points_below[self.get_point_id('Plat_Sup_D'), 0], self.points_below[self.get_point_id('Plat_Sup_D'), 1])
                    painter.drawLine(self.points_below[self.get_point_id('Plat_Sup_Post'), 0], self.points_below[self.get_point_id('Plat_Sup_Post'), 1], self.points_below[self.get_point_id('Plat_Sup_G'), 0], self.points_below[self.get_point_id('Plat_Sup_G'), 1])
                    painter.drawLine(self.points_below[self.get_point_id('Plat_Sup_Post'), 0], self.points_below[self.get_point_id('Plat_Sup_Post'), 1], self.points_below[self.get_point_id('Plat_Sup_D'), 0], self.points_below[self.get_point_id('Plat_Sup_D'), 1])

                    painter.drawLine(self.points_below[self.get_point_id('Plat_Inf_Ant'), 0], self.points_below[self.get_point_id('Plat_Inf_Ant'), 1], self.points_below[self.get_point_id('Plat_Inf_G'), 0], self.points_below[self.get_point_id('Plat_Inf_G'), 1])
                    painter.drawLine(self.points_below[self.get_point_id('Plat_Inf_Ant'), 0], self.points_below[self.get_point_id('Plat_Inf_Ant'), 1], self.points_below[self.get_point_id('Plat_Inf_D'), 0], self.points_below[self.get_point_id('Plat_Inf_D'), 1])
                    painter.drawLine(self.points_below[self.get_point_id('Plat_Inf_Post'), 0], self.points_below[self.get_point_id('Plat_Inf_Post'), 1], self.points_below[self.get_point_id('Plat_Inf_G'), 0], self.points_below[self.get_point_id('Plat_Inf_G'), 1])
                    painter.drawLine(self.points_below[self.get_point_id('Plat_Inf_Post'), 0], self.points_below[self.get_point_id('Plat_Inf_Post'), 1], self.points_below[self.get_point_id('Plat_Inf_D'), 0], self.points_below[self.get_point_id('Plat_Inf_D'), 1])

                    painter.drawLine(self.points_below[self.get_point_id('Centroid_G'), 0], self.points_below[self.get_point_id('Centroid_G'), 1], self.points_below[self.get_point_id('Centroid_D'), 0], self.points_below[self.get_point_id('Centroid_D'), 1])

                    painter.drawPoint(self.points_below[self.get_point_id('Spinous-Process'), 0], self.points_below[self.get_point_id('Spinous-Process'), 1])

        if (self.show_vertebra_above == True) and (self.hide_lines == False):
            if self.vertebra_name != 'T1':
                v_id = self.spine.get_upper_vertebra_id(self.vertebra_name)
                v = self.spine.vertebrae[v_id]

                painter.setPen(Qt.darkBlue)
                
                if v.name == 'S1':
                    self.points_above[self.get_point_id('Plat_Sup_Ant'), 0], self.points_above[self.get_point_id('Plat_Sup_Ant'), 1] = self.transform_point(self.view, v.get_point_coords('Plat_Sup_Ant'))
                    self.points_above[self.get_point_id('Plat_Sup_Post'), 0], self.points_above[self.get_point_id('Plat_Sup_Post'), 1] = self.transform_point(self.view, v.get_point_coords('Plat_Sup_Post'))
                    painter.drawLine(self.points_above[self.get_point_id('Plat_Sup_Ant'), 0], self.points_above[self.get_point_id('Plat_Sup_Ant'), 1], self.points_above[self.get_point_id('Plat_Sup_Post'), 0], self.points_above[self.get_point_id('Plat_Sup_Post'), 1])

                else:
                    for i in range(11): # Edited D.L (range(10) to range(11))
                        self.points_above[i, 0], self.points_above[i, 1] = self.transform_point(self.view, v.get_point_coords(self.points_names[i]))
                
                    painter.drawLine(self.points_above[self.get_point_id('Plat_Sup_Ant'), 0], self.points_above[self.get_point_id('Plat_Sup_Ant'), 1], self.points_above[self.get_point_id('Plat_Sup_G'), 0], self.points_above[self.get_point_id('Plat_Sup_G'), 1])
                    painter.drawLine(self.points_above[self.get_point_id('Plat_Sup_Ant'), 0], self.points_above[self.get_point_id('Plat_Sup_Ant'), 1], self.points_above[self.get_point_id('Plat_Sup_D'), 0], self.points_above[self.get_point_id('Plat_Sup_D'), 1])
                    painter.drawLine(self.points_above[self.get_point_id('Plat_Sup_Post'), 0], self.points_above[self.get_point_id('Plat_Sup_Post'), 1], self.points_above[self.get_point_id('Plat_Sup_G'), 0], self.points_above[self.get_point_id('Plat_Sup_G'), 1])
                    painter.drawLine(self.points_above[self.get_point_id('Plat_Sup_Post'), 0], self.points_above[self.get_point_id('Plat_Sup_Post'), 1], self.points_above[self.get_point_id('Plat_Sup_D'), 0], self.points_above[self.get_point_id('Plat_Sup_D'), 1])

                    painter.drawLine(self.points_above[self.get_point_id('Plat_Inf_Ant'), 0], self.points_above[self.get_point_id('Plat_Inf_Ant'), 1], self.points_above[self.get_point_id('Plat_Inf_G'), 0], self.points_above[self.get_point_id('Plat_Inf_G'), 1])
                    painter.drawLine(self.points_above[self.get_point_id('Plat_Inf_Ant'), 0], self.points_above[self.get_point_id('Plat_Inf_Ant'), 1], self.points_above[self.get_point_id('Plat_Inf_D'), 0], self.points_above[self.get_point_id('Plat_Inf_D'), 1])
                    painter.drawLine(self.points_above[self.get_point_id('Plat_Inf_Post'), 0], self.points_above[self.get_point_id('Plat_Inf_Post'), 1], self.points_above[self.get_point_id('Plat_Inf_G'), 0], self.points_above[self.get_point_id('Plat_Inf_G'), 1])
                    painter.drawLine(self.points_above[self.get_point_id('Plat_Inf_Post'), 0], self.points_above[self.get_point_id('Plat_Inf_Post'), 1], self.points_above[self.get_point_id('Plat_Inf_D'), 0], self.points_above[self.get_point_id('Plat_Inf_D'), 1])

                    painter.drawLine(self.points_above[self.get_point_id('Centroid_G'), 0], self.points_above[self.get_point_id('Centroid_G'), 1], self.points_above[self.get_point_id('Centroid_D'), 0], self.points_above[self.get_point_id('Centroid_D'), 1])

                    painter.drawPoint(self.points_below[self.get_point_id('Spinous-Process'), 0], self.points_below[self.get_point_id('Spinous-Process'), 1])

        if self.render_point == True:
            painter.setPen(Qt.green)
            diameter = 8
            painter.drawEllipse(QRect(self.dot_x - diameter / 2, self.dot_y - diameter /2, diameter, diameter))

        if self.render_region == True:
            painter.setPen(Qt.green)
            if self.region_name == 'u_ep':
                painter.drawLine(self.points[self.get_point_id('Plat_Sup_Ant'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Sup_Ant'), 1] + self.displacement_y, self.points[self.get_point_id('Plat_Sup_G'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Sup_G'), 1] + self.displacement_y)
                painter.drawLine(self.points[self.get_point_id('Plat_Sup_Ant'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Sup_Ant'), 1] + self.displacement_y, self.points[self.get_point_id('Plat_Sup_D'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Sup_D'), 1] + self.displacement_y)
                painter.drawLine(self.points[self.get_point_id('Plat_Sup_Post'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Sup_Post'), 1] + self.displacement_y, self.points[self.get_point_id('Plat_Sup_G'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Sup_G'), 1] + self.displacement_y)
                painter.drawLine(self.points[self.get_point_id('Plat_Sup_Post'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Sup_Post'), 1] + self.displacement_y, self.points[self.get_point_id('Plat_Sup_D'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Sup_D'), 1] + self.displacement_y)

            if self.region_name == 'l_ep':
                painter.drawLine(self.points[self.get_point_id('Plat_Inf_Ant'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Inf_Ant'), 1] + self.displacement_y, self.points[self.get_point_id('Plat_Inf_G'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Inf_G'), 1] + self.displacement_y)
                painter.drawLine(self.points[self.get_point_id('Plat_Inf_Ant'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Inf_Ant'), 1] + self.displacement_y, self.points[self.get_point_id('Plat_Inf_D'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Inf_D'), 1] + self.displacement_y)
                painter.drawLine(self.points[self.get_point_id('Plat_Inf_Post'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Inf_Post'), 1] + self.displacement_y, self.points[self.get_point_id('Plat_Inf_G'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Inf_G'), 1] + self.displacement_y)
                painter.drawLine(self.points[self.get_point_id('Plat_Inf_Post'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Inf_Post'), 1] + self.displacement_y, self.points[self.get_point_id('Plat_Inf_D'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Inf_D'), 1] + self.displacement_y)

            if self.region_name == 'ped':
                painter.drawLine(self.points[self.get_point_id('Centroid_G'), 0] + self.displacement_x, self.points[self.get_point_id('Centroid_G'), 1] + self.displacement_y, self.points[self.get_point_id('Centroid_D'), 0] + self.displacement_x, self.points[self.get_point_id('Centroid_D'), 1] + self.displacement_y)

            if self.region_name == 'proc':
                painter.drawPoint(self.points[self.get_point_id('Spinous-Process'), 0] + self.displacement_x, self.points[self.get_point_id('Spinous-Process'), 1] + self.displacement_y)

        if self.show_points_names == True:
            font = QFont()
            font.setPixelSize(14);
            painter.setPen(Qt.green)
            painter.setFont(font)
            if self.vertebra_name == 'Hips':
                painter.drawText(self.points[self.get_point_id('Hip_G'), 0], self.points[self.get_point_id('Hip_G'), 1], 'L')
                painter.drawText(self.points[self.get_point_id('Hip_D'), 0], self.points[self.get_point_id('Hip_D'), 1], 'R')

            if self.vertebra_name == 'S1':
                painter.drawText(self.points[self.get_point_id('Plat_Sup_Ant'), 0], self.points[self.get_point_id('Plat_Sup_Ant'), 1], 'A')
                painter.drawText(self.points[self.get_point_id('Plat_Sup_Post'), 0], self.points[self.get_point_id('Plat_Sup_Post'), 1], 'P')
                    
            if (self.vertebra_name != 'Hips') and (self.vertebra_name != 'S1'):
                if self.region_name == 'u_ep':
                    painter.drawText(self.points[self.get_point_id('Plat_Sup_G'), 0], self.points[self.get_point_id('Plat_Sup_G'), 1], 'L')
                    painter.drawText(self.points[self.get_point_id('Plat_Sup_D'), 0], self.points[self.get_point_id('Plat_Sup_D'), 1], 'R')
                    painter.drawText(self.points[self.get_point_id('Plat_Sup_Ant'), 0], self.points[self.get_point_id('Plat_Sup_Ant'), 1], 'A')
                    painter.drawText(self.points[self.get_point_id('Plat_Sup_Post'), 0], self.points[self.get_point_id('Plat_Sup_Post'), 1], 'P')
                    
                if self.region_name == 'l_ep':
                    painter.drawText(self.points[self.get_point_id('Plat_Inf_G'), 0], self.points[self.get_point_id('Plat_Inf_G'), 1], 'L')
                    painter.drawText(self.points[self.get_point_id('Plat_Inf_D'), 0], self.points[self.get_point_id('Plat_Inf_D'), 1], 'R')
                    painter.drawText(self.points[self.get_point_id('Plat_Inf_Ant'), 0], self.points[self.get_point_id('Plat_Inf_Ant'), 1], 'A')
                    painter.drawText(self.points[self.get_point_id('Plat_Inf_Post'), 0], self.points[self.get_point_id('Plat_Inf_Post'), 1], 'P')
                    
                if self.region_name == 'ped':
                    painter.drawText(self.points[self.get_point_id('Centroid_G'), 0], self.points[self.get_point_id('Centroid_G'), 1], 'L')
                    painter.drawText(self.points[self.get_point_id('Centroid_D'), 0], self.points[self.get_point_id('Centroid_D'), 1], 'R')

                if self.region_name == 'proc': # Edited D.L (added)
                    painter.drawText(self.points[self.get_point_id('Spinous_proess'), 0], self.points[self.get_point_id('Spinous_proess'), 1], 'P')

        painter.end()  
        
    def update_points_region(self):
        v_id = self.spine.get_vertebra_id(self.vertebra_name)
        v = self.spine.vertebrae[v_id]

        if self.region_name == 'u_ep':
            px_orig, py_orig = self.untransform_point(self.view, self.points[self.get_point_id('Plat_Sup_G'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Sup_G'), 1] + self.displacement_y)
            v.update_point_coords(self.view, 'Plat_Sup_G', px_orig, py_orig)
            px_orig, py_orig = self.untransform_point(self.view, self.points[self.get_point_id('Plat_Sup_D'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Sup_D'), 1] + self.displacement_y)
            v.update_point_coords(self.view, 'Plat_Sup_D', px_orig, py_orig)
            px_orig, py_orig = self.untransform_point(self.view, self.points[self.get_point_id('Plat_Sup_Ant'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Sup_Ant'), 1] + self.displacement_y)
            v.update_point_coords(self.view, 'Plat_Sup_Ant', px_orig, py_orig)
            px_orig, py_orig = self.untransform_point(self.view, self.points[self.get_point_id('Plat_Sup_Post'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Sup_Post'), 1] + self.displacement_y)
            v.update_point_coords(self.view, 'Plat_Sup_Post', px_orig, py_orig)            

        if self.region_name == 'l_ep':
            px_orig, py_orig = self.untransform_point(self.view, self.points[self.get_point_id('Plat_Inf_G'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Inf_G'), 1] + self.displacement_y)
            v.update_point_coords(self.view, 'Plat_Inf_G', px_orig, py_orig)
            px_orig, py_orig = self.untransform_point(self.view, self.points[self.get_point_id('Plat_Inf_D'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Inf_D'), 1] + self.displacement_y)
            v.update_point_coords(self.view, 'Plat_Inf_D', px_orig, py_orig)
            px_orig, py_orig = self.untransform_point(self.view, self.points[self.get_point_id('Plat_Inf_Ant'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Inf_Ant'), 1] + self.displacement_y)
            v.update_point_coords(self.view, 'Plat_Inf_Ant', px_orig, py_orig)
            px_orig, py_orig = self.untransform_point(self.view, self.points[self.get_point_id('Plat_Inf_Post'), 0] + self.displacement_x, self.points[self.get_point_id('Plat_Inf_Post'), 1] + self.displacement_y)
            v.update_point_coords(self.view, 'Plat_Inf_Post', px_orig, py_orig)
            
        if self.region_name == 'ped':
            px_orig, py_orig = self.untransform_point(self.view, self.points[self.get_point_id('Centroid_G'), 0] + self.displacement_x, self.points[self.get_point_id('Centroid_G'), 1] + self.displacement_y)
            v.update_point_coords(self.view, 'Centroid_G', px_orig, py_orig)
            px_orig, py_orig = self.untransform_point(self.view, self.points[self.get_point_id('Centroid_D'), 0] + self.displacement_x, self.points[self.get_point_id('Centroid_D'), 1] + self.displacement_y)
            v.update_point_coords(self.view, 'Centroid_D', px_orig, py_orig)

        if self.region_name == 'proc': # Edited D.L (added)
            px_orig, py_orig = self.untransform_point(self.view, self.points[self.get_point_id('Spinous_Process'), 0] + self.displacement_x, self.points[self.get_point_id('Spinous_Process'), 1] + self.displacement_y)
            v.update_point_coords(self.view, 'Spinous_Process', px_orig, py_orig)

        return

    def mouseReleaseEvent(self, event):
        if event.button() == Qt.LeftButton:
            print("mouse released!\n")

            if self.render_point == True:
                print("move point {}!\n".format(self.point_rendered))
                self.points[self.point_rendered, 0] = event.pos().x()
                self.points[self.point_rendered, 1] = event.pos().y()

                px_orig, py_orig = self.untransform_point(self.view, self.points[self.point_rendered, 0], self.points[self.point_rendered, 1])
                v_id = self.spine.get_vertebra_id(self.vertebra_name)
                v = self.spine.vertebrae[v_id]
                v.update_point_coords(self.view, self.points_names[self.point_rendered], px_orig, py_orig)
                self.twin_widget.update_painter()

                self.spine.vertebrae_modified[v_id] = 1

            self.left_clicked = False
            self.render_point = False
            self.update_painter()

        if event.button() == Qt.RightButton:
            self.hide_lines = False
            self.show_points_names = False
            self.update_painter()

        if self.render_region == True:
            self.displacement_x = event.pos().x() - self.initial_point_x
            self.displacement_y = event.pos().y() - self.initial_point_y 
            self.render_region = False
            self.update_points_region()
            self.update_painter()
            self.twin_widget.update_painter()
            v_id = self.spine.get_vertebra_id(self.vertebra_name)
            self.spine.vertebrae_modified[v_id] = 1
        
        return

    def transform_point(self, view, point):
        px = int((float(point[0]) - float(self.minx)) * self.scale_factor)
        py = int((float(point[1]) - float(self.miny)) * self.scale_factor)
        pz = int((float(point[2]) - float(self.minz)) * self.scale_factor)
        if view == 0:
            return py, pz
        else:
            return px, pz

    def untransform_point(self, view, pxl, pyl):
        if view == 0:
            px = float(pxl) / self.scale_factor + float(self.miny)
        else:
            px = float(pxl) / self.scale_factor + float(self.minx)
        py = float(pyl) / self.scale_factor + float(self.minz)
        return int(px), int(py)

    def get_point_id(self, pname):
        return self.points_names.index(pname)

    def draw_vertebra(self, view, vname, rname):
        self.render_vertebra = True
        self.vertebra_name = vname
        self.region_name = rname
        self.view = view
        self.update_painter()
        return
          


