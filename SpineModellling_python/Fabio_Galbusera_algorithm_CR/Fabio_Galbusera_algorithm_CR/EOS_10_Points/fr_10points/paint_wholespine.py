from PyQt5.QtGui import *
from PyQt5.QtWidgets import *
from PyQt5.QtCore import *
from PyQt5.QtPrintSupport import *
import numpy as np
import math

class paint_wholespine(QLabel):
    def __init__(self, parent = None, painter_size = 500):
        QLabel.__init__(self, parent)
        self.draw_rectangle = False
        self.draw_line = False
        self.rectangle = [0., 0., 0., 0., 0., 0., 0., 0.]
        self.line = [0., 0., 0., 0.]
        self.painter_size = painter_size
        self.scale_factor = 1.

    def clear(self):
        """Clear the widget display"""
        super().clear()  # Clear the QLabel pixmap
        self.draw_rectangle = False
        self.draw_line = False

    def set_image(self, img):
        self.img = img
        return

    def set_size_image(self, sx, sy):
        self.size_x = sx
        self.size_y = sy
        self.scale_factor = float(self.painter_size) / 2. / float(self.size_y)
        return

    def show_rectangle(self, p1x, p1y, p2x, p2y, p3x, p3y, p4x, p4y):
        self.rectangle = [p1x, p1y, p2x, p2y, p3x, p3y, p4x, p4y]
        #print("rectangle {} {}, {} {}, {} {}, {} {}\n".format(p1x, p1y, p2x, p2y, p3x, p3y, p4x, p4y))
        self.draw_rectangle = True
        self.draw_line = False
        self.update_painter()
        return

    def show_line(self, p1x, p1y, p2x, p2y):
        self.line = [p1x, p1y, p2x, p2y]
        self.draw_rectangle = False
        self.draw_line = True
        self.update_painter()
        return

    def hide_rectangle(self):
        self.draw_rectangle = False
        return

    def update_painter(self):
        pix = QPixmap(self.img)
        pix = pix.scaled(int(float(self.painter_size) / 2.), int(float(self.painter_size) / 2.), Qt.KeepAspectRatio)
        self.setPixmap(pix)

        painter = QPainter(self.pixmap())
        
        if self.draw_rectangle == True:
            painter.setPen(Qt.red)
            p1x = int(self.rectangle[0] * self.scale_factor)
            p1y = int(self.rectangle[1] * self.scale_factor)
            p2x = int(self.rectangle[2] * self.scale_factor)
            p2y = int(self.rectangle[3] * self.scale_factor)
            p3x = int(self.rectangle[4] * self.scale_factor)
            p3y = int(self.rectangle[5] * self.scale_factor)
            p4x = int(self.rectangle[6] * self.scale_factor)
            p4y = int(self.rectangle[7] * self.scale_factor)
            #print("transformed rectangle {} {}, {} {}, {} {}, {} {}\n".format(p1x, p1y, p2x, p2y, p3x, p3y, p4x, p4y))

            painter.drawLine(p1x, p1y, p2x, p2y)    
            painter.drawLine(p2x, p2y, p3x, p3y)    
            painter.drawLine(p3x, p3y, p4x, p4y)    
            painter.drawLine(p4x, p4y, p1x, p1y)    

        if self.draw_line == True:
            painter.setPen(Qt.red)
            p1x = int(self.line[0] * self.scale_factor)
            p1y = int(self.line[1] * self.scale_factor)
            p2x = int(self.line[2] * self.scale_factor)
            p2y = int(self.line[3] * self.scale_factor)
            
            painter.drawLine(p1x, p1y, p2x, p2y)    
            
        painter.end()  
        
          


