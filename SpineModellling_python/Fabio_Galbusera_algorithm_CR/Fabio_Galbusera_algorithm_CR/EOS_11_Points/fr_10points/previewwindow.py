from PyQt5.QtGui import *
from PyQt5.QtWidgets import *
from PyQt5.QtCore import *
from PyQt5.QtPrintSupport import *
import configparser

class PreviewWindow(QDialog):
    def __init__(self, parent):
        super(PreviewWindow, self).__init__(parent)

        config = configparser.ConfigParser()
        config.read('config.txt')

        self.painter_size = int(float(config['graphics']['widget_size']) * 1.5)

        self.layout = QVBoxLayout()
        self.setLayout(self.layout)
        self.layout.setContentsMargins(10, 10, 10, 10)

        self.hide_lines = QCheckBox("Hide annotations", self)
        self.hide_lines.setChecked(False)
        self.layout.addWidget(self.hide_lines)
        self.hide_lines.stateChanged.connect(self.hide_lines_changed)
        self.state_hide_lines = False

        self.image = QLabel()
        self.layout.addWidget(self.image)

        pix = QPixmap("preview_lines.png")
        pix = pix.scaled(self.painter_size, self.painter_size, Qt.KeepAspectRatio)
        self.image.setPixmap(pix)
        self.show()

    def hide_lines_changed(self):
        if self.hide_lines.isChecked() == True:
            self.state_hide_lines = True
            print("hide_annotations")
            pix = QPixmap("preview_nolines.png")
            pix = pix.scaled(self.painter_size, self.painter_size, Qt.KeepAspectRatio)
            self.image.setPixmap(pix)
        else:
            self.state_hide_lines = False
            print("show annotations")
            pix = QPixmap("preview_lines.png")
            pix = pix.scaled(self.painter_size, self.painter_size, Qt.KeepAspectRatio)
            self.image.setPixmap(pix)
        return