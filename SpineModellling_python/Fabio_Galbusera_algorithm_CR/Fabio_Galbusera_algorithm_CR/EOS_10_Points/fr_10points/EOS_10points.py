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
        # Use absolute path for config.txt to allow running from other directories
        config_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'config.txt')
        config.read(config_path)
 
        self.data_folder = config['data']['path']
        self.painter_size = int(config['graphics']['widget_size'])
        
        # Increase painter size for better visibility (multiply by 1.5x)
        # This makes images larger relative to UI elements
        self.painter_size = int(self.painter_size * 1.5)
        
        # Create dedicated temp directory for image processing
        self.temp_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'temp')
        os.makedirs(self.temp_dir, exist_ok=True)
        
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
        
        # Track fullscreen state
        self.is_fullscreen = False
        
        # Set modern color scheme
        self.setStyleSheet("""
            QMainWindow {
                background-color: #2b2b2b;
            }
            QWidget {
                background-color: #2b2b2b;
                color: #e0e0e0;
            }
        """)
 
        self.layout = QVBoxLayout()
        container = QWidget()
        container.setLayout(self.layout)
        self.setCentralWidget(container)
        self.layout.setContentsMargins(10, 10, 10, 10)  # Reduced from 15
        self.layout.setSpacing(8)  # Reduced from 15

        # Add patient selector at the top
        patient_selector_box = QHBoxLayout()
        patient_selector_box.setSpacing(10)
        self.layout.addLayout(patient_selector_box)
        
        patient_label = QLabel("Select Patient:")
        patient_label.setStyleSheet("""
            QLabel {
                color: #e0e0e0;
                font-size: 13px;
                font-weight: bold;
                padding: 5px;
            }
        """)
        patient_selector_box.addWidget(patient_label)
        
        self.patient_combo = QComboBox()
        self.patient_combo.setStyleSheet("""
            QComboBox {
                background: #3a3a3a;
                color: #e0e0e0;
                border: 2px solid #606060;
                border-radius: 5px;
                padding: 8px;
                font-size: 12px;
                min-width: 200px;
            }
            QComboBox:hover {
                border: 2px solid #4a90e2;
            }
            QComboBox::drop-down {
                border: none;
                width: 30px;
            }
            QComboBox::down-arrow {
                image: none;
                border-left: 5px solid transparent;
                border-right: 5px solid transparent;
                border-top: 6px solid #e0e0e0;
                margin-right: 10px;
            }
            QComboBox QAbstractItemView {
                background: #3a3a3a;
                color: #e0e0e0;
                selection-background-color: #4a90e2;
                border: 2px solid #606060;
            }
        """)
        self.patient_combo.currentIndexChanged.connect(self.change_patient)
        patient_selector_box.addWidget(self.patient_combo)
        
        patient_selector_box.addStretch()
        
        # Load available patients
        self.load_patient_list()
        print(f"DEBUG: Patient combo has {self.patient_combo.count()} items after load_patient_list()")
 
        self.hbox = QHBoxLayout()
        self.layout.addLayout(self.hbox)
 
        self.image_box = QVBoxLayout()
        self.hbox.addLayout(self.image_box)
 
        self.imageap = paint_wholespine.paint_wholespine(painter_size=self.painter_size)
        self.image_box.addWidget(self.imageap)
 
        self.imagelat = paint_wholespine.paint_wholespine(painter_size=self.painter_size)
        self.image_box.addWidget(self.imagelat)
 
        fixedfont = QFontDatabase.systemFont(QFontDatabase.FixedFont)
        fixedfont.setPointSize(12)        
 
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
        
        # Set reference to main window for hover updates (still needed for hover highlighting)
        self.cropap.main_window = self
        self.croplat.main_window = self

        # Add status info display with modern styling (reduced size)
        self.info_box = QVBoxLayout()
        self.info_box.setSpacing(0)  # Reduced from 5 for minimal spacing
        self.layout.addLayout(self.info_box)
        
        # Create status label with smaller size (25% reduction from 16pt to 12pt)
        status_font = QFont("Segoe UI", 11, QFont.Bold)  # Reduced from 12 to 11
        
        self.status_label = QLabel("Vertebra: -- | Region: --")
        self.status_label.setFont(status_font)
        self.status_label.setStyleSheet("""
            QLabel {
                background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                    stop:0 #4a90e2, stop:1 #357abd);
                color: white;
                padding: 6px;
                border-radius: 5px;
                border: 2px solid #2c5aa0;
            }
        """)
        self.status_label.setAlignment(Qt.AlignCenter)
        self.info_box.addWidget(self.status_label)
        
        # Hover label removed to save space - hover highlighting still works on the image

        self.lower_box = QHBoxLayout()
        self.lower_box.setSpacing(8)  # Reduced from 10
        self.layout.addLayout(self.lower_box)
        
        self.right_box = QVBoxLayout()
        self.right_box.setSpacing(6)  # Reduced from 8
        self.hbox.addLayout(self.right_box)
        
        # Define modern button style
        navigation_button_style = """
            QPushButton {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #5c9ced, stop:1 #4a7ac9);
                color: white;
                border: 2px solid #3d6ba8;
                border-radius: 8px;
                padding: 12px 20px;
                font-size: 13px;
                font-weight: bold;
                min-width: 100px;
            }
            QPushButton:hover {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #6aabff, stop:1 #5c9ced);
                border: 2px solid #5c9ced;
            }
            QPushButton:pressed {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #4a7ac9, stop:1 #3d6ba8);
            }
            QPushButton:disabled {
                background: #505050;
                color: #808080;
                border: 2px solid #404040;
            }
        """
        
        action_button_style = """
            QPushButton {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #6fdc8c, stop:1 #5bc777);
                color: white;
                border: 2px solid #4ab665;
                border-radius: 8px;
                padding: 12px 20px;
                font-size: 13px;
                font-weight: bold;
                min-width: 100px;
            }
            QPushButton:hover {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #80eb9d, stop:1 #6fdc8c);
                border: 2px solid #5bc777;
            }
            QPushButton:pressed {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #5bc777, stop:1 #4ab665);
            }
        """
        
        skip_button_style = """
            QPushButton {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #f4a742, stop:1 #e09635);
                color: white;
                border: 2px solid #c98530;
                border-radius: 8px;
                padding: 12px 20px;
                font-size: 13px;
                font-weight: bold;
                min-width: 100px;
            }
            QPushButton:hover {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #ffb857, stop:1 #f4a742);
                border: 2px solid #e09635;
            }
            QPushButton:pressed {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #e09635, stop:1 #c98530);
            }
        """
 
        self.previous = QPushButton("◀ Previous", self)
        self.lower_box.addWidget(self.previous)
        self.previous.clicked.connect(self.previous_pressed)
        self.previous.setStyleSheet(navigation_button_style)
 
        self.next = QPushButton("▶ Next", self)
        self.lower_box.addWidget(self.next)
        self.next.clicked.connect(self.next_pressed)
        self.next.setStyleSheet(navigation_button_style)
 
        self.next_spine = QPushButton("⏭ Next Spine", self)
        self.lower_box.addWidget(self.next_spine)
        self.next_spine.clicked.connect(self.next_spine_pressed)
        self.next_spine.setStyleSheet(action_button_style)
 
        self.skip = QPushButton("⏩ Skip", self)
        self.lower_box.addWidget(self.skip)
        self.skip.clicked.connect(self.skip_pressed)
        self.skip.setStyleSheet(skip_button_style)
        
        # Sidebar button style
        sidebar_button_style = """
            QPushButton {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #505050, stop:1 #3a3a3a);
                color: #e0e0e0;
                border: 1px solid #606060;
                border-radius: 6px;
                padding: 10px;
                font-size: 12px;
                text-align: left;
                min-width: 120px;
            }
            QPushButton:hover {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #606060, stop:1 #4a4a4a);
                border: 1px solid #707070;
            }
            QPushButton:pressed {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #3a3a3a, stop:1 #2a2a2a);
            }
        """
        
        special_button_style = """
            QPushButton {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #9b59b6, stop:1 #8e44ad);
                color: white;
                border: 1px solid #7d3c98;
                border-radius: 6px;
                padding: 10px;
                font-size: 12px;
                font-weight: bold;
                text-align: left;
                min-width: 120px;
            }
            QPushButton:hover {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #ae6bc7, stop:1 #9b59b6);
                border: 1px solid #8e44ad;
            }
            QPushButton:pressed {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #8e44ad, stop:1 #7d3c98);
            }
        """
        
        save_button_style = """
            QPushButton {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #27ae60, stop:1 #229954);
                color: white;
                border: 2px solid #1e8449;
                border-radius: 6px;
                padding: 10px;
                font-size: 13px;
                font-weight: bold;
                text-align: left;
                min-width: 120px;
            }
            QPushButton:hover {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #2ecc71, stop:1 #27ae60);
                border: 2px solid #229954;
            }
            QPushButton:pressed {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #229954, stop:1 #1e8449);
            }
        """
 
        self.preview = QPushButton("🔍 Preview", self)
        self.right_box.addWidget(self.preview)
        self.preview.clicked.connect(self.preview_pressed)
        self.preview.setStyleSheet(special_button_style)
 
        self.zoom_fit = QPushButton("🔲 Zoom Fit", self)
        self.right_box.addWidget(self.zoom_fit)
        self.zoom_fit.clicked.connect(self.zoom_fit_pressed)
        self.zoom_fit.setStyleSheet(sidebar_button_style)
 
        # Add zoom slider label
        zoom_label = QLabel("🔍 Zoom", self)
        zoom_label.setStyleSheet("""
            QLabel {
                color: #e0e0e0;
                font-size: 11px;
                padding: 0px;
                margin: 0px;
            }
        """)
        self.right_box.addWidget(zoom_label)
        
        # Create zoom slider (inverted so left = zoom out, right = zoom in)
        self.zoom_slider = QSlider(Qt.Horizontal, self)
        self.zoom_slider.setMinimum(5)  # 0.5x zoom (more zoomed out)
        self.zoom_slider.setMaximum(80)  # 8.0x zoom (more zoomed in)
        self.zoom_slider.setValue(15)  # Default 1.5x (crop_scale_factor = 1.5)
        self.zoom_slider.setTickPosition(QSlider.TicksBelow)
        self.zoom_slider.setTickInterval(10)
        self.zoom_slider.valueChanged.connect(self.zoom_slider_changed)
        self.zoom_slider.setStyleSheet("""
            QSlider::groove:horizontal {
                border: 1px solid #606060;
                height: 6px;
                background: #3a3a3a;
                margin: 0px 0;
                border-radius: 3px;
            }
            QSlider::handle:horizontal {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #606060, stop:1 #4a4a4a);
                border: 1px solid #707070;
                width: 16px;
                margin: -4px 0;
                border-radius: 8px;
            }
            QSlider::handle:horizontal:hover {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #707070, stop:1 #5a5a5a);
            }
            QSlider::sub-page:horizontal {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #4a90e2, stop:1 #357abd);
                border: 1px solid #2e6da4;
                height: 6px;
                border-radius: 3px;
            }
        """)
        self.right_box.addWidget(self.zoom_slider)
 
        self.autolevels = QPushButton("✨ Auto Levels", self)
        self.right_box.addWidget(self.autolevels)
        self.autolevels.clicked.connect(self.autolevels_pressed)
        self.autolevels.setStyleSheet(sidebar_button_style)
 
        self.brightness_plus = QPushButton("☀️ Brightness +", self)
        self.right_box.addWidget(self.brightness_plus)
        self.brightness_plus.clicked.connect(self.brightness_plus_pressed)
        self.brightness_plus.setStyleSheet(sidebar_button_style)
 
        self.brightness_minus = QPushButton("🌙 Brightness −", self)
        self.right_box.addWidget(self.brightness_minus)
        self.brightness_minus.clicked.connect(self.brightness_minus_pressed)
        self.brightness_minus.setStyleSheet(sidebar_button_style)
 
        self.contrast_plus = QPushButton("◐ Contrast +", self)
        self.right_box.addWidget(self.contrast_plus)
        self.contrast_plus.clicked.connect(self.contrast_plus_pressed)
        self.contrast_plus.setStyleSheet(sidebar_button_style)
 
        self.contrast_minus = QPushButton("◑ Contrast −", self)
        self.right_box.addWidget(self.contrast_minus)
        self.contrast_minus.clicked.connect(self.contrast_minus_pressed)
        self.contrast_minus.setStyleSheet(sidebar_button_style)
        
        # Add Image Enhancement button
        enhance_button_style = """
            QPushButton {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #9b59b6, stop:1 #8e44ad);
                color: white;
                border: 2px solid #7d3c98;
                border-radius: 6px;
                padding: 10px;
                font-size: 12px;
                font-weight: bold;
                text-align: left;
                min-width: 120px;
            }
            QPushButton:hover {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #ae6bc7, stop:1 #9b59b6);
                border: 2px solid #8e44ad;
            }
            QPushButton:pressed {
                background: qlineargradient(x1:0, y1:0, x2:0, y2:1,
                    stop:0 #8e44ad, stop:1 #7d3c98);
            }
        """
        
        self.enhance_button = QPushButton("✨ Enhance Image", self)
        self.right_box.addWidget(self.enhance_button)
        self.enhance_button.clicked.connect(self.enhance_image_pressed)
        self.enhance_button.setStyleSheet(enhance_button_style)
        self.image_enhanced = False  # Track enhancement state
        
        # Additional enhancement options
        self.gamma_button = QPushButton("🔆 Gamma Correction", self)
        self.right_box.addWidget(self.gamma_button)
        self.gamma_button.clicked.connect(self.gamma_correction_pressed)
        self.gamma_button.setStyleSheet(sidebar_button_style)
        self.gamma_enabled = False
        
        self.edge_button = QPushButton("🔲 Edge Enhancement", self)
        self.right_box.addWidget(self.edge_button)
        self.edge_button.clicked.connect(self.edge_enhancement_pressed)
        self.edge_button.setStyleSheet(sidebar_button_style)
        self.edge_enabled = False
        
        self.histogram_button = QPushButton("📊 Histogram Eq", self)
        self.right_box.addWidget(self.histogram_button)
        self.histogram_button.clicked.connect(self.histogram_eq_pressed)
        self.histogram_button.setStyleSheet(sidebar_button_style)
        self.histogram_enabled = False
        
        self.invert_button = QPushButton("⚫⚪ Invert Colors", self)
        self.right_box.addWidget(self.invert_button)
        self.invert_button.clicked.connect(self.invert_colors_pressed)
        self.invert_button.setStyleSheet(sidebar_button_style)
        self.invert_enabled = False
        
        self.denoise_button = QPushButton("🔇 Denoise", self)
        self.right_box.addWidget(self.denoise_button)
        self.denoise_button.clicked.connect(self.denoise_pressed)
        self.denoise_button.setStyleSheet(sidebar_button_style)
        self.denoise_enabled = False
 
        # Add Save button
        self.save_button = QPushButton("💾 Save", self)
        self.right_box.addWidget(self.save_button)
        self.save_button.clicked.connect(self.save_pressed)
        self.save_button.setStyleSheet(save_button_style)
        
        # Add separator
        separator = QFrame()
        separator.setFrameShape(QFrame.HLine)
        separator.setStyleSheet("background-color: #505050;")
        self.right_box.addWidget(separator)
        
        # Checkbox style
        checkbox_style = """
            QCheckBox {
                color: #e0e0e0;
                font-size: 11px;
                spacing: 8px;
            }
            QCheckBox::indicator {
                width: 18px;
                height: 18px;
                border-radius: 4px;
                border: 2px solid #606060;
                background-color: #3a3a3a;
            }
            QCheckBox::indicator:hover {
                border: 2px solid #4a90e2;
                background-color: #404040;
            }
            QCheckBox::indicator:checked {
                background-color: #4a90e2;
                border: 2px solid #357abd;
                image: url(data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIiIGhlaWdodD0iMTIiIHZpZXdCb3g9IjAgMCAxMiAxMiIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cGF0aCBkPSJNMTAgMkw0LjUgNy41IDIgNSIgc3Ryb2tlPSJ3aGl0ZSIgc3Ryb2tlLXdpZHRoPSIyIiBmaWxsPSJub25lIi8+PC9zdmc+);
            }
            QCheckBox::indicator:checked:hover {
                background-color: #5c9ced;
                border: 2px solid #4a7ac9;
            }
        """
 
        self.show_vertebra_below = QCheckBox("📍 Show Vertebra Below", self)
        self.show_vertebra_below.setChecked(False)
        self.right_box.addWidget(self.show_vertebra_below)
        self.show_vertebra_below.stateChanged.connect(self.show_vertebra_below_changed)
        self.show_vertebra_below.setStyleSheet(checkbox_style)
        self.state_show_vertebra_below = False
 
        self.show_vertebra_above = QCheckBox("📍 Show Vertebra Above", self)
        self.show_vertebra_above.setChecked(False)
        self.right_box.addWidget(self.show_vertebra_above)
        self.show_vertebra_above.stateChanged.connect(self.show_vertebra_above_changed)
        self.show_vertebra_above.setStyleSheet(checkbox_style)
        self.state_show_vertebra_above = False
 
        self.read_folder()
        self.counter = 0
        
        # Set window to fullscreen (maximized) instead of fixed size
        # Remove fixed size to allow window to be resizable
        # self.setFixedSize(int(float(self.painter_size * 2.5)), int(float(self.painter_size * 1.2)))
        
        # Start maximized/fullscreen
        # self.showMaximized()  # This makes it fill the screen but keeps taskbar visible
        # Alternative: self.showFullScreen()  # This makes it truly fullscreen (hides taskbar)
        
        # Optionally set minimum size to prevent it from being too small
        self.setMinimumSize(1200, 800)
 
        # self.show()  # Commented out since showMaximized() already shows the window
        self.next_spine_pressed()
        
        # Add keyboard shortcut for fullscreen toggle (F11)
        self.fullscreen_shortcut = QShortcut(QKeySequence("F11"), self)
        self.fullscreen_shortcut.activated.connect(self.toggle_fullscreen)
        
        # Add keyboard shortcut for exit fullscreen (Escape)
        self.escape_shortcut = QShortcut(QKeySequence("Escape"), self)
        self.escape_shortcut.activated.connect(self.exit_fullscreen)
    
    def load_patient_list(self):
        """Load available patients from Patients directory into dropdown"""
        # Get base directory (navigate up from fr_10points to SpineModellling_python)
        # Path: fr_10points -> EOS_10_Points -> Fabio_Galbusera_algorithm_CR -> Fabio_Galbusera_algorithm_CR -> SpineModellling_python
        script_dir = os.path.dirname(os.path.abspath(__file__))
        base_dir = os.path.join(script_dir, '..', '..', '..', '..')
        patients_dir = os.path.abspath(os.path.join(base_dir, 'Patients'))
        
        if not os.path.exists(patients_dir):
            print(f"⚠️ Patients directory not found: {patients_dir}")
            return
        
        # Store current selection to restore if possible
        current_patient = None
        if hasattr(self, 'data_folder') and self.data_folder:
            # Extract patient ID from current data_folder path
            parts = self.data_folder.rstrip('/\\').split(os.sep)
            if 'EOS' in parts:
                eos_idx = parts.index('EOS')
                if eos_idx > 0:
                    current_patient = parts[eos_idx - 1]
        
        # Scan for patient directories that have EOS subfolder
        self.patient_combo.blockSignals(True)  # Prevent triggering change_patient during population
        self.patient_combo.clear()
        
        patient_list = []
        for patient_id in sorted(os.listdir(patients_dir)):
            patient_path = os.path.join(patients_dir, patient_id)
            eos_path = os.path.join(patient_path, 'EOS')
            
            if os.path.isdir(patient_path) and os.path.exists(eos_path):
                # Check if EOS folder has valid image pairs (anteroposterior images)
                has_valid_data = any(
                    f.endswith('_C.jpg') or f.endswith('_C.jpeg') or f.endswith('_C.tif') or f.endswith('_C.png')
                    for f in os.listdir(eos_path)
                )
                if has_valid_data:
                    patient_list.append((patient_id, eos_path))
                    self.patient_combo.addItem(patient_id, eos_path)
        
        # Restore previous selection or select first patient
        if current_patient and current_patient in [p[0] for p in patient_list]:
            index = [p[0] for p in patient_list].index(current_patient)
            self.patient_combo.setCurrentIndex(index)
        elif self.patient_combo.count() > 0:
            self.patient_combo.setCurrentIndex(0)
        
        self.patient_combo.blockSignals(False)
        
        if patient_list:
            print(f"✅ Found {len(patient_list)} patients with EOS data: {[p[0] for p in patient_list]}")
            # If we restored a previous selection or selected first patient, trigger change_patient
            if self.patient_combo.count() > 0:
                # Manually call change_patient for the initially selected patient
                # This ensures the data folder is updated to match the dropdown selection
                current_index = self.patient_combo.currentIndex()
                if current_index >= 0:
                    eos_path = self.patient_combo.itemData(current_index)
                    if eos_path:
                        self.data_folder = os.path.abspath(eos_path) + os.sep
                        print(f"📂 Initial patient: {self.patient_combo.currentText()}")
                        print(f"   Data folder: {self.data_folder}")
        else:
            print(f"⚠️ No patients with valid EOS data found in: {patients_dir}")
    
    def change_patient(self, index):
        """Handle patient selection change from dropdown"""
        if index < 0:
            return
        
        # Get the selected patient's EOS path
        eos_path = self.patient_combo.itemData(index)
        if not eos_path or not os.path.exists(eos_path):
            print(f"⚠️ Invalid EOS path for selected patient")
            return
        
        # Update data_folder and reload
        self.data_folder = os.path.abspath(eos_path) + os.sep
        print(f"\n📂 Switching to patient: {self.patient_combo.currentText()}")
        print(f"   Data folder: {self.data_folder}")
        
        # Clear existing data
        self.files_txt = []
        self.files_c = []
        self.files_l = []
        self.counter = 0
        
        # Reload folder and reset to first case
        self.read_folder()
        
        if self.files_txt:
            # Start with first case
            self.counter = 0
            self.next_spine_pressed()
        else:
            patient_name = self.patient_combo.currentText()
            print(f"⚠️ No valid cases found for patient {patient_name}")
            
            # Clear the display
            self.cropap.clear()
            self.croplat.clear()
            self.imageap.clear()
            self.imagelat.clear()
            self.setWindowTitle("Landmark Editor - No Cases Loaded")
            
            # Show user-friendly message
            from PyQt5.QtWidgets import QMessageBox
            msg = QMessageBox()
            msg.setIcon(QMessageBox.Information)
            msg.setWindowTitle("No Cases to Edit")
            msg.setText(f"No editable landmark data found for patient {patient_name}.")
            msg.setInformativeText(
                "The editor requires processed landmark files (.txt) that haven't been edited or skipped yet.\n\n"
                "Please use the Landmark Detection tab to process this patient's images first."
            )
            msg.setStandardButtons(QMessageBox.Ok)
            msg.exec_()
    
    def toggle_fullscreen(self):
        """Toggle between fullscreen and maximized window"""
        if self.is_fullscreen:
            self.showMaximized()
            self.is_fullscreen = False
            print("Exited fullscreen mode (F11 to toggle)")
        else:
            self.showFullScreen()
            self.is_fullscreen = True
            print("Entered fullscreen mode (F11 or ESC to exit)")
    
    def exit_fullscreen(self):
        """Exit fullscreen mode when Escape is pressed"""
        if self.is_fullscreen:
            self.showMaximized()
            self.is_fullscreen = False
            print("Exited fullscreen mode")
    
    def update_status_label(self, selected_point_name=None):
        """Update the status label to show current vertebra, region, and selected point
        
        Args:
            selected_point_name: The name of the currently selected point (e.g., 'Plat_Inf_Ant')
                               If None, shows all available points for the region
        """
        vertebra = self.status.get_vertebra()
        region = self.status.get_region()
        
        # Create readable region names with emojis
        region_names = {
            'u_ep': '⬆️ Upper Endplate',
            'l_ep': '⬇️ Lower Endplate',
            'ped': '🔘 Pedicles'
        }
        region_display = region_names.get(region, region)
        
        # Map point names to readable names
        point_name_map = {
            'Plat_Sup_Ant': 'Anterior',
            'Plat_Sup_Post': 'Posterior',
            'Plat_Sup_G': 'Left',
            'Plat_Sup_D': 'Right',
            'Plat_Inf_Ant': 'Anterior',
            'Plat_Inf_Post': 'Posterior',
            'Plat_Inf_G': 'Left',
            'Plat_Inf_D': 'Right',
            'Centroid_G': 'Left Centroid',
            'Centroid_D': 'Right Centroid',
            'Hip_G': 'Left Hip',
            'Hip_D': 'Right Hip'
        }
        
        # Determine what point info to show
        if selected_point_name:
            # Show only the selected point
            point_readable = point_name_map.get(selected_point_name, selected_point_name)
            point_info = f"({point_readable})"
        else:
            # Show all available points for the region
            point_details = {
                'u_ep': '(Anterior, Posterior, Left, Right)',
                'l_ep': '(Anterior, Posterior, Left, Right)',
                'ped': '(Left Centroid, Right Centroid)'
            }
            point_info = point_details.get(region, '')
        
        self.status_label.setText(f"🦴 Vertebra: {vertebra}  |  {region_display} {point_info}")
        
        # Change gradient colors based on region for better visibility
        region_gradients = {
            'u_ep': """
                QLabel {
                    background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                        stop:0 #e74c3c, stop:1 #c0392b);
                    color: white;
                    padding: 6px;
                    border-radius: 5px;
                    border: 2px solid #a93226;
                }
            """,
            'l_ep': """
                QLabel {
                    background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                        stop:0 #3498db, stop:1 #2980b9);
                    color: white;
                    padding: 6px;
                    border-radius: 5px;
                    border: 2px solid #21618c;
                }
            """,
            'ped': """
                QLabel {
                    background: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                        stop:0 #2ecc71, stop:1 #27ae60);
                    color: white;
                    padding: 6px;
                    border-radius: 5px;
                    border: 2px solid #1e8449;
                }
            """
        }
        gradient = region_gradients.get(region, region_gradients['l_ep'])
        self.status_label.setStyleSheet(gradient)
    
    def update_hover_label(self, marker_name):
        """Hover label removed to save space - this method kept for compatibility"""
        # The yellow highlight ring still shows on hover in the paint_interactive widget
        # No label update needed anymore
        pass

    def preview_pressed(self):
        print("Preview\n")
        self.spine.render_image("preview_lines.png", self.data_folder + self.files_c[self.counter - 1], self.data_folder + self.files_l[self.counter - 1])
        self.spine.render_image_nolines("preview_nolines.png", self.data_folder + self.files_c[self.counter - 1], self.data_folder + self.files_l[self.counter - 1])
        pw = previewwindow.PreviewWindow(self)
        pw.exec_()
        return
 
    def save_pressed(self):
        if self.counter != 0:
            cur_txt = self.files_txt[self.counter - 1]
            fn_txt = self.data_folder + self._edited_txt_name(cur_txt)
            self.spine.write_to_file(fn_txt)
            print(f"Saved to: {fn_txt}")
     
             # Also save the rendered image (single _edited suffix)
            render_base = self._edited_img_base(cur_txt)
            fn_render = self.data_folder + render_base + '.' + self.image_type
            self.spine.render_image(fn_render,
                self.data_folder + self.files_c[self.counter - 1],
                self.data_folder + self.files_l[self.counter - 1])
            print(f"Rendered image saved to: {fn_render}")
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
    
    def enhance_image_pressed(self):
        """
        Toggle advanced image enhancement for EOS radiographic images.
        Uses research-based techniques specifically designed for medical X-ray imaging:
        - CLAHE (Contrast Limited Adaptive Histogram Equalization)
        - Bilateral filtering for noise reduction while preserving edges
        - Unsharp masking for edge enhancement
        """
        self.image_enhanced = not self.image_enhanced
        
        if self.image_enhanced:
            print("Image Enhancement: ENABLED")
            print("Applying CLAHE + Bilateral Filter + Edge Enhancement...")
            self.enhance_button.setText("✨ Enhanced (ON)")
        else:
            print("Image Enhancement: DISABLED")
            print("Reverting to standard processing...")
            self.enhance_button.setText("✨ Enhance Image")
        
        # Reprocess images with or without enhancement
        self.update_images(new_vertebra=True, reset_zoom=False)
        return
    
    def gamma_correction_pressed(self):
        """Toggle gamma correction for mid-tone brightness adjustment"""
        self.gamma_enabled = not self.gamma_enabled
        
        if self.gamma_enabled:
            print("Gamma Correction: ENABLED")
            self.gamma_button.setText("🔆 Gamma (ON)")
        else:
            print("Gamma Correction: DISABLED")
            self.gamma_button.setText("🔆 Gamma Correction")
        
        self.update_images(new_vertebra=True, reset_zoom=False)
        return
    
    def edge_enhancement_pressed(self):
        """Toggle aggressive edge enhancement"""
        self.edge_enabled = not self.edge_enabled
        
        if self.edge_enabled:
            print("Edge Enhancement: ENABLED")
            self.edge_button.setText("🔲 Edge (ON)")
        else:
            print("Edge Enhancement: DISABLED")
            self.edge_button.setText("🔲 Edge Enhancement")
        
        self.update_images(new_vertebra=True, reset_zoom=False)
        return
    
    def histogram_eq_pressed(self):
        """Toggle histogram equalization for global contrast"""
        self.histogram_enabled = not self.histogram_enabled
        
        if self.histogram_enabled:
            print("Histogram Equalization: ENABLED")
            self.histogram_button.setText("📊 Histogram (ON)")
        else:
            print("Histogram Equalization: DISABLED")
            self.histogram_button.setText("📊 Histogram Eq")
        
        self.update_images(new_vertebra=True, reset_zoom=False)
        return
    
    def invert_colors_pressed(self):
        """Toggle color inversion"""
        self.invert_enabled = not self.invert_enabled
        
        if self.invert_enabled:
            print("Invert Colors: ENABLED")
            self.invert_button.setText("⚫⚪ Invert (ON)")
        else:
            print("Invert Colors: DISABLED")
            self.invert_button.setText("⚫⚪ Invert Colors")
        
        self.update_images(new_vertebra=True, reset_zoom=False)
        return
    
    def denoise_pressed(self):
        """Toggle additional denoising"""
        self.denoise_enabled = not self.denoise_enabled
        
        if self.denoise_enabled:
            print("Denoise: ENABLED")
            self.denoise_button.setText("🔇 Denoise (ON)")
        else:
            print("Denoise: DISABLED")
            self.denoise_button.setText("🔇 Denoise")
        
        self.update_images(new_vertebra=True, reset_zoom=False)
        return
 
    def autolevels_pressed(self):
        self.brightness = 0
        self.contrast = 0
        self.update_brightnesscontrast()
        return
 
    def zoom_fit_pressed(self):
        if self.status.get_vertebra()== 'S1':
            self.crop_scale_factor = 3.
            self.zoom_slider.setValue(30)  # Update slider to 3.0
        else:
            self.crop_scale_factor = 1.5
            self.zoom_slider.setValue(15)  # Update slider to 1.5
        print("Crop scale factor: {}\n".format(self.crop_scale_factor))
        self.update_images(new_vertebra = True)
        return
 
    def zoom_slider_changed(self, value):
        # Convert slider value (5-50) to crop_scale_factor (0.5-5.0)
        self.crop_scale_factor = value / 10.0
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
 
        eq_c = cv2.imread(os.path.join(self.temp_dir, "temp_c_nobc.png"), cv2.IMREAD_GRAYSCALE)
        eq_l = cv2.imread(os.path.join(self.temp_dir, "temp_l_nobc.png"), cv2.IMREAD_GRAYSCALE)
 
        eq_c = apply_brightness_contrast(eq_c, self.brightness, self.contrast)
        eq_l = apply_brightness_contrast(eq_l, self.brightness, self.contrast)
 
        cv2.imwrite(os.path.join(self.temp_dir, "temp_c.png"), eq_c)
        cv2.imwrite(os.path.join(self.temp_dir, "temp_l.png"), eq_l)
 
        pix_cropap = QPixmap(os.path.join(self.temp_dir, "temp_c.png"))
        pix_croplat = QPixmap(os.path.join(self.temp_dir, "temp_l.png"))
 
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
                # Detect AP image
                if name_c.endswith('_C.' + self.image_type):
                    # Construct associated filenames
                    name_l = name_c[:-6] + '_L.' + self.image_type
                    name_s = name_c[:-6] + '_S.' + self.image_type
                    name_txt = name_c[:-6] + '.txt'
                    name_txt_edited = name_c[:-6] + '_edited.txt'
                    name_txt_skipped = name_c[:-6] + '_skipped.txt'

                    # Skip if the case was explicitly marked as skipped
                    if os.path.isfile(self.data_folder + name_txt_skipped):
                        continue

                    # Prefer edited file if present, else use the original .txt
                    if os.path.isfile(self.data_folder + name_txt_edited):
                        use_txt = name_txt_edited
                    elif os.path.isfile(self.data_folder + name_txt):
                        use_txt = name_txt
                    else:
                        continue  # no text file found, skip this case

                    # Check that the corresponding lateral image exists
                    if os.path.isfile(self.data_folder + name_l):
                        use_lat = name_l
                    elif os.path.isfile(self.data_folder + name_s):
                        use_lat = name_s
                    else:
                        continue  # no lateral image found, skip this case

                    # Store valid case
                    self.files_txt.append(use_txt)
                    self.files_c.append(name_c)
                    self.files_l.append(use_lat)

        # Notify if no valid cases found
        if not self.files_txt:
            print("⚠️  No valid EOS cases found in:", self.data_folder)
        else:
            print(f"✅ Found {len(self.files_txt)} valid cases.")
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
        
        def enhance_medical_image(input_img):
            """
            Advanced enhancement for EOS radiographic images using research-based techniques.
            
            Techniques applied:
            1. CLAHE (Contrast Limited Adaptive Histogram Equalization)
               - Enhances local contrast without amplifying noise
               - Clip limit prevents over-amplification
               - Tile grid size optimized for vertebral structures
            
            2. Bilateral Filter
               - Reduces noise while preserving edges
               - Important for maintaining anatomical boundaries
               
            3. Unsharp Masking
               - Enhances edges and fine details
               - Critical for identifying vertebral endplates and landmarks
            
            References:
            - Pizer et al. "Adaptive Histogram Equalization and Its Variations" (1987)
            - Zuiderveld "Contrast Limited Adaptive Histogram Equalization" (1994)
            - Tomasi & Manduchi "Bilateral Filtering for Gray and Color Images" (1998)
            """
            # Step 1: CLAHE - Contrast Limited Adaptive Histogram Equalization
            # clipLimit: Threshold for contrast limiting (2.0-4.0 optimal for X-rays)
            # tileGridSize: Size of grid for histogram equalization (8x8 good for vertebrae)
            clahe = cv2.createCLAHE(clipLimit=3.0, tileGridSize=(8, 8))
            enhanced = clahe.apply(input_img)
            
            # Step 2: Bilateral Filter - Noise reduction while preserving edges
            # d=9: Diameter of pixel neighborhood
            # sigmaColor=75: Filter sigma in color space (higher = more colors mixed)
            # sigmaSpace=75: Filter sigma in coordinate space (higher = farther pixels influence)
            enhanced = cv2.bilateralFilter(enhanced, d=9, sigmaColor=75, sigmaSpace=75)
            
            # Step 3: Unsharp Masking - Edge enhancement
            # Create a Gaussian blurred version
            gaussian = cv2.GaussianBlur(enhanced, (0, 0), 2.0)
            # Unsharp mask formula: original + amount * (original - blurred)
            enhanced = cv2.addWeighted(enhanced, 1.5, gaussian, -0.5, 0)
            
            # Step 4: Ensure values are in valid range
            enhanced = np.clip(enhanced, 0, 255).astype(np.uint8)
            
            return enhanced
        
        def apply_gamma_correction(input_img, gamma=1.5):
            """
            Apply gamma correction to adjust mid-tone brightness.
            Gamma > 1: Brightens mid-tones (good for dark images)
            Gamma < 1: Darkens mid-tones
            """
            # Build lookup table for gamma correction
            inv_gamma = 1.0 / gamma
            table = np.array([((i / 255.0) ** inv_gamma) * 255 for i in range(256)]).astype(np.uint8)
            return cv2.LUT(input_img, table)
        
        def apply_edge_enhancement(input_img):
            """
            Apply aggressive edge enhancement using Laplacian filter.
            """
            # Apply Laplacian edge detection
            laplacian = cv2.Laplacian(input_img, cv2.CV_64F)
            laplacian = np.uint8(np.absolute(laplacian))
            # Add edges back to original image
            enhanced = cv2.addWeighted(input_img, 1.0, laplacian, 0.5, 0)
            return np.clip(enhanced, 0, 255).astype(np.uint8)
        
        def apply_histogram_equalization(input_img):
            """
            Apply global histogram equalization for contrast enhancement.
            Simple and fast method.
            """
            return cv2.equalizeHist(input_img)
        
        def apply_invert(input_img):
            """
            Invert image colors (black becomes white, white becomes black).
            Sometimes useful for viewing bone structures.
            """
            return cv2.bitwise_not(input_img)
        
        def apply_denoise(input_img):
            """
            Apply Non-local Means Denoising for noise reduction.
            Preserves details better than simple Gaussian blur.
            """
            return cv2.fastNlMeansDenoising(input_img, None, h=10, templateWindowSize=7, searchWindowSize=21)
 
        if reset_zoom == True:
            if self.status.get_vertebra()== 'S1':
                self.crop_scale_factor = 3.
                self.zoom_slider.setValue(30)  # Update slider to match
            else:
                self.crop_scale_factor = 1.5
                self.zoom_slider.setValue(15)  # Update slider to match
 
        if new_vertebra == True:
            def remove_padding(px, py, pz):
                px -= self.padding
                py -= self.padding
                pz -= self.padding
                return px, py, pz
 
            size_x, size_y, size_z = self.get_images_size(self.data_folder + self.files_c[self.counter - 1], self.data_folder + self.files_l[self.counter - 1])
           
 
            min_x, max_x, min_y, max_y, min_z, max_z, factor = self.spine.get_crop_region(self.status.get_vertebra(), self.crop_scale_factor, size_x, size_y, size_z)
            self.crop_scale_factor = factor
            # Update slider to match the crop_scale_factor that may have been adjusted
            self.zoom_slider.blockSignals(True)  # Prevent triggering valueChanged
            self.zoom_slider.setValue(int(self.crop_scale_factor * 10))
            self.zoom_slider.blockSignals(False)
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
            
            # Apply advanced enhancement if enabled
            if self.image_enhanced:
                eq_c = enhance_medical_image(eq_c)
                eq_l = enhance_medical_image(eq_l)
            
            # Apply additional enhancements
            if self.histogram_enabled:
                eq_c = apply_histogram_equalization(eq_c)
                eq_l = apply_histogram_equalization(eq_l)
            
            if self.gamma_enabled:
                eq_c = apply_gamma_correction(eq_c, gamma=1.5)
                eq_l = apply_gamma_correction(eq_l, gamma=1.5)
            
            if self.edge_enabled:
                eq_c = apply_edge_enhancement(eq_c)
                eq_l = apply_edge_enhancement(eq_l)
            
            if self.denoise_enabled:
                eq_c = apply_denoise(eq_c)
                eq_l = apply_denoise(eq_l)
            
            if self.invert_enabled:
                eq_c = apply_invert(eq_c)
                eq_l = apply_invert(eq_l)
 
            cv2.imwrite(os.path.join(self.temp_dir, "temp_c_nobc.png"), eq_c)
            cv2.imwrite(os.path.join(self.temp_dir, "temp_l_nobc.png"), eq_l)
 
            eq_c = apply_brightness_contrast(eq_c, self.brightness, self.contrast)
            eq_l = apply_brightness_contrast(eq_l, self.brightness, self.contrast)
 
            cv2.imwrite(os.path.join(self.temp_dir, "temp_c.png"), eq_c)
            cv2.imwrite(os.path.join(self.temp_dir, "temp_l.png"), eq_l)
 
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
 
                psa_x, psa_y, psa_z = remove_padding(psa_x, psa_y, psa_z)
                psp_x, psp_y, psp_z = remove_padding(psp_x, psp_y, psp_z)
                psg_x, psg_y, psg_z = remove_padding(psg_x, psg_y, psg_z)
                psd_x, psd_y, psd_z = remove_padding(psd_x, psd_y, psd_z)
                pia_x, pia_y, pia_z = remove_padding(pia_x, pia_y, pia_z)
                pip_x, pip_y, pip_z = remove_padding(pip_x, pip_y, pip_z)
                pig_x, pig_y, pig_z = remove_padding(pig_x, pig_y, pig_z)
                pid_x, pid_y, pid_z = remove_padding(pid_x, pid_y, pid_z)
 
                self.imageap.set_size_image(size_y, size_z)
                self.imagelat.set_size_image(size_x, size_z)
 
                self.imageap.show_rectangle(psg_y, psg_z, psd_y, psd_z, pid_y, pid_z, pig_y, pig_z)
                self.imagelat.show_rectangle(psa_x, psa_z, psp_x, psp_z, pip_x, pip_z, pia_x, pia_z)
 
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
 
        pix_cropap = QPixmap(os.path.join(self.temp_dir, "temp_c.png"))
        pix_croplat = QPixmap(os.path.join(self.temp_dir, "temp_l.png"))
 
        self.cropap.setPixmap(pix_cropap)
        self.croplat.setPixmap(pix_croplat)  
 
        self.cropap.draw_vertebra(0, self.status.get_vertebra(), self.status.get_region())
        self.croplat.draw_vertebra(1, self.status.get_vertebra(), self.status.get_region())
 
        return
 
    def next_pressed(self):
        new_vertebra = True
        new_spine = False
        reset_zoom = False  # Keep zoom level when navigating
        last_region = False
        if self.counter != 0:
            new_vertebra, last_region = self.status.next()
            # Don't reset zoom when navigating between vertebrae
            # This allows the zoom level to persist
            self.previous.setEnabled(True)

        if last_region == True:
            self.next.setEnabled(False)
       
        print("Vertebra {}, region {}\n".format(self.status.get_vertebra(), self.status.get_region()))
        self.update_images(new_vertebra, reset_zoom)
        
        # Update status label
        self.update_status_label()

        self.setWindowTitle(self.status.get_vertebra() + ' --- ' + str(self.counter) + ' in ' + str(len(self.files_txt)) + ' --- ' + self.files_txt[self.counter - 1])
        return
    
    def _edited_txt_name(self, txt_name: str) -> str:
        # 'ASD_501.txt' -> 'ASD_501_edited.txt'
        # 'ASD_501_edited.txt' -> 'ASD_501_edited.txt' (unchanged)
        return txt_name if txt_name.endswith('_edited.txt') else txt_name[:-4] + '_edited.txt'

    def _edited_img_base(self, txt_name: str) -> str:
        # Returns basename (no extension) for the rendered image
        base = txt_name[:-4] if txt_name.endswith('.txt') else txt_name
        return base if base.endswith('_edited') else base + '_edited'

    def next_spine_pressed(self):
    # Save current case (if any), overwriting single-edited name
        if 0 < self.counter <= len(self.files_txt):
            cur_txt = self.files_txt[self.counter - 1]
            fn_txt = self.data_folder + self._edited_txt_name(cur_txt)
            self.spine.write_to_file(fn_txt)

            render_base = self._edited_img_base(cur_txt)
            fn_render = self.data_folder + render_base + '.' + self.image_type
            self.spine.render_image(
                fn_render,
                self.data_folder + self.files_c[self.counter - 1],
                self.data_folder + self.files_l[self.counter - 1]
            )

        # No more cases?
        if self.counter >= len(self.files_txt):
            QMessageBox.information(self, "Done", "No more spines to process.")
            return
        
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
        
        # Update status label
        self.update_status_label()

        return 
    def previous_pressed(self):
        new_vertebra = self.status.previous()
        reset_zoom = False  # Keep zoom level when navigating
        if (self.status.get_vertebra() == 'Hips'):
            self.previous.setEnabled(False)
        self.next.setEnabled(True)
 
        print("Vertebra {}, region {}\n".format(self.status.get_vertebra(), self.status.get_region()))
        self.update_images(new_vertebra, reset_zoom)
        self.setWindowTitle(self.status.get_vertebra() + ' --- ' +str(self.counter) + ' in ' + str(len(self.files_txt)) + ' --- ' + self.files_txt[self.counter - 1])
        
        # Update status label
        self.update_status_label()
        
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
        
        # Update status label
        self.update_status_label()

        return
    
    def closeEvent(self, event):
        """Auto-save when window is closed"""
        if self.counter != 0:
            try:
                cur_txt = self.files_txt[self.counter - 1]
                fn_txt = self.data_folder + self._edited_txt_name(cur_txt)
                self.spine.write_to_file(fn_txt)
                print(f"Auto-saved to: {fn_txt}")
            except Exception as e:
                print("Error during auto-save: {}".format(e))
       
        event.accept()  # Allow the window to close
 
if __name__ == '__main__':
    app = QApplication(sys.argv)
    app.setApplicationName("EOS Fast Reconstruction")
 
    window = MainWindow()
    window.showMaximized()
    app.exec_()