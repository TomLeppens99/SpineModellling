from PyQt5.QtGui import *
from PyQt5.QtWidgets import *
from PyQt5.QtCore import *
from PyQt5.QtPrintSupport import *
import numpy as np
import math
import os
 
class paint_interactive(QLabel):
    def __init__(self, parent = None):
        QLabel.__init__(self, parent)
        self.spine = None
        self.minx = 0
        self.miny = 0
        self.minz = 0
        self.scale_factor = 1.
        self.points = np.zeros((12, 2), dtype = float)
        self.points_below = np.zeros((12, 2), dtype = float)
        self.points_above = np.zeros((12, 2), dtype = float)

        self.points_names = ['Plat_Sup_G', 'Plat_Sup_D', 'Plat_Sup_Ant', 'Plat_Sup_Post',
                       'Plat_Inf_G', 'Plat_Inf_D', 'Plat_Inf_Ant', 'Plat_Inf_Post',
                       'Centroid_G', 'Centroid_D',
                       'Hip_G', 'Hip_D']
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
        self.main_window = None  # Reference to main window for hover updates
        self.hovered_point = -1  # Track which point is being hovered over
        
        # Enable mouse tracking for hover events
        self.setMouseTracking(True)
    
    def clear(self):
        """Clear the widget display"""
        super().clear()  # Clear the QLabel pixmap
        self.spine = None
        self.render_vertebra = False
        self.render_point = False
    
    def draw_line_int(self, painter, x1, y1, x2, y2):
        """Helper function to draw lines with integer coordinates"""
        painter.drawLine(int(x1), int(y1), int(x2), int(y2))
 
    def check_point_clicked(self, px, py):
        print("px: {}, py: {}\n".format(px, py))
        min_distance = 1.E8
        best = 0
        distance = 0
        if (self.vertebra_name != 'Hips') and (self.vertebra_name != 'S1'):
            for i in range(10):
                distance = math.sqrt((px - self.points[i,0])*(px - self.points[i,0]) + (py - self.points[i,1])*(py - self.points[i,1]))
                if distance < min_distance:
                    min_distance = distance
                    best = i
 
        if self.vertebra_name == 'Hips':
            for i in range(10, 12):
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
 
            if (self.region_name == 'u_ep') and (p_name in points_u_ep):
                return 1  
            if (self.region_name == 'l_ep') and (p_name in points_l_ep):
                return 1  
            if (self.region_name == 'ped') and (p_name in points_ped):
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
                    # Update status label to show the selected point
                    if self.main_window:
                        self.main_window.update_status_label(self.points_names[closest])
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
        # Check for hover over points (even when not dragging)
        if not self.render_point and not self.render_region:
            closest, distance = self.check_point_clicked(event.pos().x(), event.pos().y())
            if distance < 15.:  # Larger hover detection radius
                if self.check_point_in_region(closest):
                    if self.hovered_point != closest:
                        self.hovered_point = closest
                        # Update hover label in main window
                        if self.main_window:
                            self.main_window.update_hover_label(self.points_names[closest])
                        self.update_painter()
                else:
                    if self.hovered_point != -1:
                        self.hovered_point = -1
                        if self.main_window:
                            self.main_window.update_hover_label(None)
                        self.update_painter()
            else:
                if self.hovered_point != -1:
                    self.hovered_point = -1
                    if self.main_window:
                        self.main_window.update_hover_label(None)
                    self.update_painter()
        
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
            pix = QPixmap(os.path.join(self.main_window.temp_dir, "temp_c.png"))
        else:
            pix = QPixmap(os.path.join(self.main_window.temp_dir, "temp_l.png"))
        self.setPixmap(pix)
        painter = QPainter(self.pixmap())
 
        if (self.render_vertebra == True) and (self.hide_lines == False) and (self.render_region == False):
            v_id = self.spine.get_vertebra_id(self.vertebra_name)
            v = self.spine.vertebrae[v_id]
 
            if (self.vertebra_name == 'Hips'):
                self.points[self.get_point_id('Hip_G'), 0], self.points[self.get_point_id('Hip_G'), 1] = self.transform_point(self.view, v.get_point_coords('Hip_G'))
                self.points[self.get_point_id('Hip_D'), 0], self.points[self.get_point_id('Hip_D'), 1] = self.transform_point(self.view, v.get_point_coords('Hip_D'))
                painter.setPen(Qt.red)
                painter.drawLine(int(self.points[self.get_point_id('Hip_G'), 0]), int(self.points[self.get_point_id('Hip_G'), 1]), int(self.points[self.get_point_id('Hip_D'), 0]), int(self.points[self.get_point_id('Hip_D'), 1]))
 
            if (self.vertebra_name == 'S1'):
                self.points[self.get_point_id('Plat_Sup_Ant'), 0], self.points[self.get_point_id('Plat_Sup_Ant'), 1] = self.transform_point(self.view, v.get_point_coords('Plat_Sup_Ant'))
                self.points[self.get_point_id('Plat_Sup_Post'), 0], self.points[self.get_point_id('Plat_Sup_Post'), 1] = self.transform_point(self.view, v.get_point_coords('Plat_Sup_Post'))
                painter.setPen(Qt.red)
                painter.drawLine(int(self.points[self.get_point_id('Plat_Sup_Ant'), 0]), int(self.points[self.get_point_id('Plat_Sup_Ant'), 1]), int(self.points[self.get_point_id('Plat_Sup_Post'), 0]), int(self.points[self.get_point_id('Plat_Sup_Post'), 1]))
 
            if (self.vertebra_name != 'Hips') and (self.vertebra_name != 'S1'):
                for i in range(10):
                    self.points[i, 0], self.points[i, 1] = self.transform_point(self.view, v.get_point_coords(self.points_names[i]))
           
                if self.region_name == 'u_ep':
                    painter.setPen(Qt.red)
                else:
                    painter.setPen(Qt.black)
                painter.drawLine(int(self.points[self.get_point_id('Plat_Sup_Ant'), 0]), int(self.points[self.get_point_id('Plat_Sup_Ant'), 1]), int(self.points[self.get_point_id('Plat_Sup_G'), 0]), int(self.points[self.get_point_id('Plat_Sup_G'), 1]))
                painter.drawLine(int(self.points[self.get_point_id('Plat_Sup_Ant'), 0]), int(self.points[self.get_point_id('Plat_Sup_Ant'), 1]), int(self.points[self.get_point_id('Plat_Sup_D'), 0]), int(self.points[self.get_point_id('Plat_Sup_D'), 1]))
                painter.drawLine(int(self.points[self.get_point_id('Plat_Sup_Post'), 0]), int(self.points[self.get_point_id('Plat_Sup_Post'), 1]), int(self.points[self.get_point_id('Plat_Sup_G'), 0]), int(self.points[self.get_point_id('Plat_Sup_G'), 1]))
                painter.drawLine(int(self.points[self.get_point_id('Plat_Sup_Post'), 0]), int(self.points[self.get_point_id('Plat_Sup_Post'), 1]), int(self.points[self.get_point_id('Plat_Sup_D'), 0]), int(self.points[self.get_point_id('Plat_Sup_D'), 1]))
               
                if self.region_name == 'l_ep':
                    painter.setPen(Qt.red)
                else:
                    painter.setPen(Qt.black)
                painter.drawLine(int(self.points[self.get_point_id('Plat_Inf_Ant'), 0]), int(self.points[self.get_point_id('Plat_Inf_Ant'), 1]), int(self.points[self.get_point_id('Plat_Inf_G'), 0]), int(self.points[self.get_point_id('Plat_Inf_G'), 1]))
                painter.drawLine(int(self.points[self.get_point_id('Plat_Inf_Ant'), 0]), int(self.points[self.get_point_id('Plat_Inf_Ant'), 1]), int(self.points[self.get_point_id('Plat_Inf_D'), 0]), int(self.points[self.get_point_id('Plat_Inf_D'), 1]))
                painter.drawLine(int(self.points[self.get_point_id('Plat_Inf_Post'), 0]), int(self.points[self.get_point_id('Plat_Inf_Post'), 1]), int(self.points[self.get_point_id('Plat_Inf_G'), 0]), int(self.points[self.get_point_id('Plat_Inf_G'), 1]))
                painter.drawLine(int(self.points[self.get_point_id('Plat_Inf_Post'), 0]), int(self.points[self.get_point_id('Plat_Inf_Post'), 1]), int(self.points[self.get_point_id('Plat_Inf_D'), 0]), int(self.points[self.get_point_id('Plat_Inf_D'), 1]))
 
                if self.region_name == 'ped':
                    painter.setPen(Qt.red)
                else:
                    painter.setPen(Qt.black)
                painter.drawLine(int(self.points[self.get_point_id('Centroid_G'), 0]), int(self.points[self.get_point_id('Centroid_G'), 1]), int(self.points[self.get_point_id('Centroid_D'), 0]), int(self.points[self.get_point_id('Centroid_D'), 1]))
               
                if self.region_name == 'u_ep':
                    if self.view == 0:
                        painter.setPen(Qt.yellow)
                        painter.drawLine(int(self.points[self.get_point_id('Plat_Sup_G'), 0]), int(self.points[self.get_point_id('Plat_Sup_G'), 1]), int(self.points[self.get_point_id('Plat_Sup_D'), 0]), int(self.points[self.get_point_id('Plat_Sup_D'), 1]))
                    else:
                        painter.setPen(Qt.yellow)
                        painter.drawLine(int(self.points[self.get_point_id('Plat_Sup_Ant'), 0]), int(self.points[self.get_point_id('Plat_Sup_Ant'), 1]), int(self.points[self.get_point_id('Plat_Sup_Post'), 0]), int(self.points[self.get_point_id('Plat_Sup_Post'), 1]))
                elif self.region_name == 'l_ep':
                    if self.view == 0:
                        painter.setPen(Qt.yellow)
                        painter.drawLine(int(self.points[self.get_point_id('Plat_Inf_G'), 0]), int(self.points[self.get_point_id('Plat_Inf_G'), 1]), int(self.points[self.get_point_id('Plat_Inf_D'), 0]), int(self.points[self.get_point_id('Plat_Inf_D'), 1]))
                    else:
                        painter.setPen(Qt.yellow)
                        painter.drawLine(int(self.points[self.get_point_id('Plat_Inf_Ant'), 0]), int(self.points[self.get_point_id('Plat_Inf_Ant'), 1]), int(self.points[self.get_point_id('Plat_Inf_Post'), 0]), int(self.points[self.get_point_id('Plat_Inf_Post'), 1]))
 
        if (self.show_vertebra_below == True) and (self.hide_lines == False):
            if self.vertebra_name != 'Hips':
                v_id = self.spine.get_lower_vertebra_id(self.vertebra_name)
                v = self.spine.vertebrae[v_id]
 
                painter.setPen(Qt.darkBlue)
                if (v.name == 'Hips'):
                    self.points_below[self.get_point_id('Hip_G'), 0], self.points_below[self.get_point_id('Hip_G'), 1] = self.transform_point(self.view, v.get_point_coords('Hip_G'))
                    self.points_below[self.get_point_id('Hip_D'), 0], self.points_below[self.get_point_id('Hip_D'), 1] = self.transform_point(self.view, v.get_point_coords('Hip_D'))
                    painter.drawLine(int(self.points_below[self.get_point_id('Hip_G'), 0]), int(self.points_below[self.get_point_id('Hip_G'), 1]), int(self.points_below[self.get_point_id('Hip_D'), 0]), int(self.points_below[self.get_point_id('Hip_D'), 1]))
 
                if (v.name == 'S1'):
                    self.points_below[self.get_point_id('Plat_Sup_Ant'), 0], self.points_below[self.get_point_id('Plat_Sup_Ant'), 1] = self.transform_point(self.view, v.get_point_coords('Plat_Sup_Ant'))
                    self.points_below[self.get_point_id('Plat_Sup_Post'), 0], self.points_below[self.get_point_id('Plat_Sup_Post'), 1] = self.transform_point(self.view, v.get_point_coords('Plat_Sup_Post'))
                    painter.drawLine(int(self.points_below[self.get_point_id('Plat_Sup_Ant'), 0]), int(self.points_below[self.get_point_id('Plat_Sup_Ant'), 1]), int(self.points_below[self.get_point_id('Plat_Sup_Post'), 0]), int(self.points_below[self.get_point_id('Plat_Sup_Post'), 1]))
 
                if (v.name != 'Hips') and (v.name != 'S1'):
                    for i in range(10):
                        self.points_below[i, 0], self.points_below[i, 1] = self.transform_point(self.view, v.get_point_coords(self.points_names[i]))
               
                    painter.drawLine(int(self.points_below[self.get_point_id('Plat_Sup_Ant'), 0]), int(self.points_below[self.get_point_id('Plat_Sup_Ant'), 1]), int(self.points_below[self.get_point_id('Plat_Sup_G'), 0]), int(self.points_below[self.get_point_id('Plat_Sup_G'), 1]))
                    painter.drawLine(int(self.points_below[self.get_point_id('Plat_Sup_Ant'), 0]), int(self.points_below[self.get_point_id('Plat_Sup_Ant'), 1]), int(self.points_below[self.get_point_id('Plat_Sup_D'), 0]), int(self.points_below[self.get_point_id('Plat_Sup_D'), 1]))
                    painter.drawLine(int(self.points_below[self.get_point_id('Plat_Sup_Post'), 0]), int(self.points_below[self.get_point_id('Plat_Sup_Post'), 1]), int(self.points_below[self.get_point_id('Plat_Sup_G'), 0]), int(self.points_below[self.get_point_id('Plat_Sup_G'), 1]))
                    painter.drawLine(int(self.points_below[self.get_point_id('Plat_Sup_Post'), 0]), int(self.points_below[self.get_point_id('Plat_Sup_Post'), 1]), int(self.points_below[self.get_point_id('Plat_Sup_D'), 0]), int(self.points_below[self.get_point_id('Plat_Sup_D'), 1]))
 
                    painter.drawLine(int(self.points_below[self.get_point_id('Plat_Inf_Ant'), 0]), int(self.points_below[self.get_point_id('Plat_Inf_Ant'), 1]), int(self.points_below[self.get_point_id('Plat_Inf_G'), 0]), int(self.points_below[self.get_point_id('Plat_Inf_G'), 1]))
                    painter.drawLine(int(self.points_below[self.get_point_id('Plat_Inf_Ant'), 0]), int(self.points_below[self.get_point_id('Plat_Inf_Ant'), 1]), int(self.points_below[self.get_point_id('Plat_Inf_D'), 0]), int(self.points_below[self.get_point_id('Plat_Inf_D'), 1]))
                    painter.drawLine(int(self.points_below[self.get_point_id('Plat_Inf_Post'), 0]), int(self.points_below[self.get_point_id('Plat_Inf_Post'), 1]), int(self.points_below[self.get_point_id('Plat_Inf_G'), 0]), int(self.points_below[self.get_point_id('Plat_Inf_G'), 1]))
                    painter.drawLine(int(self.points_below[self.get_point_id('Plat_Inf_Post'), 0]), int(self.points_below[self.get_point_id('Plat_Inf_Post'), 1]), int(self.points_below[self.get_point_id('Plat_Inf_D'), 0]), int(self.points_below[self.get_point_id('Plat_Inf_D'), 1]))
 
                    painter.drawLine(int(self.points_below[self.get_point_id('Centroid_G'), 0]), int(self.points_below[self.get_point_id('Centroid_G'), 1]), int(self.points_below[self.get_point_id('Centroid_D'), 0]), int(self.points_below[self.get_point_id('Centroid_D'), 1]))
                   
        if (self.show_vertebra_above == True) and (self.hide_lines == False):
            if self.vertebra_name != 'T1':
                v_id = self.spine.get_upper_vertebra_id(self.vertebra_name)
                v = self.spine.vertebrae[v_id]
 
                painter.setPen(Qt.darkBlue)
               
                if v.name == 'S1':
                    self.points_above[self.get_point_id('Plat_Sup_Ant'), 0], self.points_above[self.get_point_id('Plat_Sup_Ant'), 1] = self.transform_point(self.view, v.get_point_coords('Plat_Sup_Ant'))
                    self.points_above[self.get_point_id('Plat_Sup_Post'), 0], self.points_above[self.get_point_id('Plat_Sup_Post'), 1] = self.transform_point(self.view, v.get_point_coords('Plat_Sup_Post'))
                    painter.drawLine(int(self.points_above[self.get_point_id('Plat_Sup_Ant'), 0]), int(self.points_above[self.get_point_id('Plat_Sup_Ant'), 1]), int(self.points_above[self.get_point_id('Plat_Sup_Post'), 0]), int(self.points_above[self.get_point_id('Plat_Sup_Post'), 1]))
 
                else:
                    for i in range(10):
                        self.points_above[i, 0], self.points_above[i, 1] = self.transform_point(self.view, v.get_point_coords(self.points_names[i]))
               
                    painter.drawLine(int(self.points_above[self.get_point_id('Plat_Sup_Ant'), 0]), int(self.points_above[self.get_point_id('Plat_Sup_Ant'), 1]), int(self.points_above[self.get_point_id('Plat_Sup_G'), 0]), int(self.points_above[self.get_point_id('Plat_Sup_G'), 1]))
                    painter.drawLine(int(self.points_above[self.get_point_id('Plat_Sup_Ant'), 0]), int(self.points_above[self.get_point_id('Plat_Sup_Ant'), 1]), int(self.points_above[self.get_point_id('Plat_Sup_D'), 0]), int(self.points_above[self.get_point_id('Plat_Sup_D'), 1]))
                    painter.drawLine(int(self.points_above[self.get_point_id('Plat_Sup_Post'), 0]), int(self.points_above[self.get_point_id('Plat_Sup_Post'), 1]), int(self.points_above[self.get_point_id('Plat_Sup_G'), 0]), int(self.points_above[self.get_point_id('Plat_Sup_G'), 1]))
                    painter.drawLine(int(self.points_above[self.get_point_id('Plat_Sup_Post'), 0]), int(self.points_above[self.get_point_id('Plat_Sup_Post'), 1]), int(self.points_above[self.get_point_id('Plat_Sup_D'), 0]), int(self.points_above[self.get_point_id('Plat_Sup_D'), 1]))
 
                    painter.drawLine(int(self.points_above[self.get_point_id('Plat_Inf_Ant'), 0]), int(self.points_above[self.get_point_id('Plat_Inf_Ant'), 1]), int(self.points_above[self.get_point_id('Plat_Inf_G'), 0]), int(self.points_above[self.get_point_id('Plat_Inf_G'), 1]))
                    painter.drawLine(int(self.points_above[self.get_point_id('Plat_Inf_Ant'), 0]), int(self.points_above[self.get_point_id('Plat_Inf_Ant'), 1]), int(self.points_above[self.get_point_id('Plat_Inf_D'), 0]), int(self.points_above[self.get_point_id('Plat_Inf_D'), 1]))
                    painter.drawLine(int(self.points_above[self.get_point_id('Plat_Inf_Post'), 0]), int(self.points_above[self.get_point_id('Plat_Inf_Post'), 1]), int(self.points_above[self.get_point_id('Plat_Inf_G'), 0]), int(self.points_above[self.get_point_id('Plat_Inf_G'), 1]))
                    painter.drawLine(int(self.points_above[self.get_point_id('Plat_Inf_Post'), 0]), int(self.points_above[self.get_point_id('Plat_Inf_Post'), 1]), int(self.points_above[self.get_point_id('Plat_Inf_D'), 0]), int(self.points_above[self.get_point_id('Plat_Inf_D'), 1]))
 
                    painter.drawLine(int(self.points_above[self.get_point_id('Centroid_G'), 0]), int(self.points_above[self.get_point_id('Centroid_G'), 1]), int(self.points_above[self.get_point_id('Centroid_D'), 0]), int(self.points_above[self.get_point_id('Centroid_D'), 1]))
                   
 
        if self.render_point == True:
            painter.setPen(Qt.green)
            diameter = 8
            painter.drawEllipse(QRect(int(self.dot_x - diameter / 2), int(self.dot_y - diameter / 2), diameter, diameter))
 
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
           
        if self.show_points_names == True:
            font = QFont()
            font.setPixelSize(14);
            painter.setPen(Qt.green)
            painter.setFont(font)
            if self.vertebra_name == 'Hips':
                painter.drawText(int(self.points[self.get_point_id('Hip_G'), 0]), int(self.points[self.get_point_id('Hip_G'), 1]), 'L')
                painter.drawText(int(self.points[self.get_point_id('Hip_D'), 0]), int(self.points[self.get_point_id('Hip_D'), 1]), 'R')
 
            if self.vertebra_name == 'S1':
                painter.drawText(int(self.points[self.get_point_id('Plat_Sup_Ant'), 0]), int(self.points[self.get_point_id('Plat_Sup_Ant'), 1]), 'A')
                painter.drawText(int(self.points[self.get_point_id('Plat_Sup_Post'), 0]), int(self.points[self.get_point_id('Plat_Sup_Post'), 1]), 'P')
                   
            if (self.vertebra_name != 'Hips') and (self.vertebra_name != 'S1'):
                if self.region_name == 'u_ep':
                    painter.drawText(int(self.points[self.get_point_id('Plat_Sup_G'), 0]), int(self.points[self.get_point_id('Plat_Sup_G'), 1]), 'L')
                    painter.drawText(int(self.points[self.get_point_id('Plat_Sup_D'), 0]), int(self.points[self.get_point_id('Plat_Sup_D'), 1]), 'R')
                    painter.drawText(int(self.points[self.get_point_id('Plat_Sup_Ant'), 0]), int(self.points[self.get_point_id('Plat_Sup_Ant'), 1]), 'A')
                    painter.drawText(int(self.points[self.get_point_id('Plat_Sup_Post'), 0]), int(self.points[self.get_point_id('Plat_Sup_Post'), 1]), 'P')
                   
                if self.region_name == 'l_ep':
                    painter.drawText(int(self.points[self.get_point_id('Plat_Inf_G'), 0]), int(self.points[self.get_point_id('Plat_Inf_G'), 1]), 'L')
                    painter.drawText(int(self.points[self.get_point_id('Plat_Inf_D'), 0]), int(self.points[self.get_point_id('Plat_Inf_D'), 1]), 'R')
                    painter.drawText(int(self.points[self.get_point_id('Plat_Inf_Ant'), 0]), int(self.points[self.get_point_id('Plat_Inf_Ant'), 1]), 'A')
                    painter.drawText(int(self.points[self.get_point_id('Plat_Inf_Post'), 0]), int(self.points[self.get_point_id('Plat_Inf_Post'), 1]), 'P')
                   
                if self.region_name == 'ped':
                    painter.drawText(int(self.points[self.get_point_id('Centroid_G'), 0]), int(self.points[self.get_point_id('Centroid_G'), 1]), 'L')
                    painter.drawText(int(self.points[self.get_point_id('Centroid_D'), 0]), int(self.points[self.get_point_id('Centroid_D'), 1]), 'R')
       
        # Draw anatomical points as interactive circles
        painter.setPen(Qt.blue)
        painter.setBrush(Qt.blue)
        point_diameter = 6
        
        # First draw hovered point with highlight (if any)
        if self.hovered_point >= 0 and not self.render_point:
            # Draw a larger highlight circle
            painter.setPen(QPen(Qt.yellow, 3))
            painter.setBrush(Qt.NoBrush)
            highlight_diameter = 12
            painter.drawEllipse(QRect(int(self.points[self.hovered_point, 0] - highlight_diameter/2),
                                    int(self.points[self.hovered_point, 1] - highlight_diameter/2),
                                    highlight_diameter, highlight_diameter))
       
        if self.vertebra_name == 'Hips':
            # Draw Hip points
            for i in [self.get_point_id('Hip_G'), self.get_point_id('Hip_D')]:
                # Highlight point being dragged
                if self.render_point and i == self.point_rendered:
                    painter.setPen(Qt.red)
                    painter.setBrush(Qt.red)
                else:
                    painter.setPen(Qt.blue)
                    painter.setBrush(Qt.blue)
                painter.drawEllipse(QRect(int(self.points[i, 0] - point_diameter/2),
                                        int(self.points[i, 1] - point_diameter/2),
                                        point_diameter, point_diameter))
       
        elif self.vertebra_name == 'S1':
            # Draw S1 points
            for i in [self.get_point_id('Plat_Sup_Ant'), self.get_point_id('Plat_Sup_Post')]:
                # Highlight point being dragged
                if self.render_point and i == self.point_rendered:
                    painter.setPen(Qt.red)
                    painter.setBrush(Qt.red)
                else:
                    painter.setPen(Qt.blue)
                    painter.setBrush(Qt.blue)
                painter.drawEllipse(QRect(int(self.points[i, 0] - point_diameter/2),
                                        int(self.points[i, 1] - point_diameter/2),
                                        point_diameter, point_diameter))
       
        else:
            # Draw vertebra points (T1-L5)
            for i, point_name in enumerate(self.points_names):
                if i < len(self.points):  # Safety check
                    # Highlight point being dragged
                    if self.render_point and i == self.point_rendered:
                        painter.setPen(Qt.red)
                        painter.setBrush(Qt.red)
                    else:
                        painter.setPen(Qt.blue)
                        painter.setBrush(Qt.blue)
                    painter.drawEllipse(QRect(int(self.points[i, 0] - point_diameter/2),
                                            int(self.points[i, 1] - point_diameter/2),
                                            point_diameter, point_diameter))
                   
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
            # Reset status label to show all points when released
            if self.main_window:
                self.main_window.update_status_label(None)
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
         