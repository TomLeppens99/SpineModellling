"""
3D Registration Viewer for EOS Images
Displays frontal and lateral X-ray images in 3D space with proper coordinate system
"""
import sys
import os
import numpy as np
from PyQt5.QtWidgets import (QMainWindow, QWidget, QVBoxLayout, QHBoxLayout, 
                             QPushButton, QLabel, QComboBox, QSlider, QCheckBox,
                             QListWidget, QListWidgetItem, QGroupBox)
from PyQt5.QtCore import Qt
from PyQt5.QtGui import QFont
import matplotlib
matplotlib.use('Qt5Agg')
from matplotlib.backends.backend_qt5agg import FigureCanvasQTAgg as FigureCanvas
from matplotlib.backends.backend_qt5agg import NavigationToolbar2QT as NavigationToolbar
from matplotlib.figure import Figure
from mpl_toolkits.mplot3d import Axes3D
import matplotlib.pyplot as plt
from matplotlib import patches
import cv2


class Registration3DViewer(QMainWindow):
    """
    3D Registration viewer for EOS images
    Shows frontal (AP) and lateral (Sagittal) images in 3D coordinate space
    """
    
    def __init__(self, parent=None):
        super().__init__(parent)
        self.setWindowTitle("3D Registration Viewer")
        
        # Data storage
        self.current_patient = None
        self.frontal_image = None
        self.lateral_image = None
        self.frontal_path = None
        self.lateral_path = None
        
        # Image positioning (in mm, typical EOS setup)
        self.frontal_distance = 1500  # Distance from origin (mm)
        self.lateral_distance = 1500  # Distance from origin (mm)
        self.image_separation = 900   # Distance between imaging planes (mm)
        
        self.init_ui()
        self.load_patients()
        
    def init_ui(self):
        """Initialize the user interface"""
        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        
        main_layout = QHBoxLayout(central_widget)
        
        # Left panel - Controls
        left_panel = QWidget()
        left_layout = QVBoxLayout(left_panel)
        left_layout.setContentsMargins(10, 10, 10, 10)
        left_panel.setMaximumWidth(300)
        
        # Patient selection
        patient_group = QGroupBox("Patient Selection")
        patient_layout = QVBoxLayout()
        
        self.patient_label = QLabel("Select Patient:")
        patient_layout.addWidget(self.patient_label)
        
        self.patient_combo = QComboBox()
        self.patient_combo.currentIndexChanged.connect(self.on_patient_changed)
        patient_layout.addWidget(self.patient_combo)
        
        patient_group.setLayout(patient_layout)
        left_layout.addWidget(patient_group)
        
        # View controls
        view_group = QGroupBox("View Controls")
        view_layout = QVBoxLayout()
        
        self.show_frontal_cb = QCheckBox("Show Frontal (AP)")
        self.show_frontal_cb.setChecked(True)
        self.show_frontal_cb.stateChanged.connect(self.update_3d_view)
        view_layout.addWidget(self.show_frontal_cb)
        
        self.show_lateral_cb = QCheckBox("Show Lateral (Sagittal)")
        self.show_lateral_cb.setChecked(True)
        self.show_lateral_cb.stateChanged.connect(self.update_3d_view)
        view_layout.addWidget(self.show_lateral_cb)
        
        self.show_axes_cb = QCheckBox("Show Coordinate Axes")
        self.show_axes_cb.setChecked(True)
        self.show_axes_cb.stateChanged.connect(self.update_3d_view)
        view_layout.addWidget(self.show_axes_cb)
        
        self.show_grid_cb = QCheckBox("Show Grid")
        self.show_grid_cb.setChecked(False)
        self.show_grid_cb.stateChanged.connect(self.update_3d_view)
        view_layout.addWidget(self.show_grid_cb)
        
        view_group.setLayout(view_layout)
        left_layout.addWidget(view_group)
        
        # Geometry controls
        geometry_group = QGroupBox("Geometry Settings")
        geometry_layout = QVBoxLayout()
        
        # Image separation slider
        sep_label = QLabel("Image Separation (mm):")
        geometry_layout.addWidget(sep_label)
        
        self.separation_slider = QSlider(Qt.Horizontal)
        self.separation_slider.setMinimum(500)
        self.separation_slider.setMaximum(1500)
        self.separation_slider.setValue(self.image_separation)
        self.separation_slider.setTickPosition(QSlider.TicksBelow)
        self.separation_slider.setTickInterval(100)
        self.separation_slider.valueChanged.connect(self.on_separation_changed)
        geometry_layout.addWidget(self.separation_slider)
        
        self.separation_value_label = QLabel(f"{self.image_separation} mm")
        geometry_layout.addWidget(self.separation_value_label)
        
        # Distance slider
        dist_label = QLabel("Image Distance (mm):")
        geometry_layout.addWidget(dist_label)
        
        self.distance_slider = QSlider(Qt.Horizontal)
        self.distance_slider.setMinimum(1000)
        self.distance_slider.setMaximum(2000)
        self.distance_slider.setValue(self.frontal_distance)
        self.distance_slider.setTickPosition(QSlider.TicksBelow)
        self.distance_slider.setTickInterval(100)
        self.distance_slider.valueChanged.connect(self.on_distance_changed)
        geometry_layout.addWidget(self.distance_slider)
        
        self.distance_value_label = QLabel(f"{self.frontal_distance} mm")
        geometry_layout.addWidget(self.distance_value_label)
        
        geometry_group.setLayout(geometry_layout)
        left_layout.addWidget(geometry_group)
        
        # Action buttons
        button_layout = QVBoxLayout()
        
        self.reset_view_btn = QPushButton("Reset View")
        self.reset_view_btn.clicked.connect(self.reset_view)
        button_layout.addWidget(self.reset_view_btn)
        
        self.export_btn = QPushButton("Export View")
        self.export_btn.clicked.connect(self.export_view)
        button_layout.addWidget(self.export_btn)
        
        left_layout.addLayout(button_layout)
        left_layout.addStretch()
        
        # Info label
        self.info_label = QLabel("Load a patient to begin")
        self.info_label.setWordWrap(True)
        self.info_label.setStyleSheet("color: #888; font-size: 10px;")
        left_layout.addWidget(self.info_label)
        
        main_layout.addWidget(left_panel)
        
        # Right panel - 3D visualization
        right_panel = QWidget()
        right_layout = QVBoxLayout(right_panel)
        right_layout.setContentsMargins(0, 0, 0, 0)
        
        # Create matplotlib figure
        self.figure = Figure(figsize=(10, 8), facecolor='#2b2b2b')
        self.canvas = FigureCanvas(self.figure)
        self.ax = self.figure.add_subplot(111, projection='3d', facecolor='#1e1e1e')
        
        # Add toolbar
        self.toolbar = NavigationToolbar(self.canvas, self)
        right_layout.addWidget(self.toolbar)
        right_layout.addWidget(self.canvas)
        
        main_layout.addWidget(right_panel, stretch=1)
        
        # Initialize empty 3D view
        self.setup_3d_axes()
        self.canvas.draw()
        
    def setup_3d_axes(self):
        """Setup the 3D axes with proper styling"""
        self.ax.clear()
        
        # Set labels and title
        self.ax.set_xlabel('X (mm)', color='white', fontsize=10)
        self.ax.set_ylabel('Y (mm)', color='white', fontsize=10)
        self.ax.set_zlabel('Z (mm)', color='white', fontsize=10)
        self.ax.set_title('EOS 3D Registration Space', color='white', fontsize=12, pad=20)
        
        # Set tick colors
        self.ax.tick_params(colors='white', labelsize=8)
        
        # Set pane colors
        self.ax.xaxis.pane.fill = False
        self.ax.yaxis.pane.fill = False
        self.ax.zaxis.pane.fill = False
        
        # Set grid
        self.ax.grid(self.show_grid_cb.isChecked(), color='#444', linewidth=0.5)
        
        # Set equal aspect ratio
        max_range = 1000
        self.ax.set_xlim([-max_range, max_range])
        self.ax.set_ylim([-max_range, max_range])
        self.ax.set_zlim([-max_range, max_range])
        
    def load_patients(self):
        """Load available patients from Patients directory"""
        script_dir = os.path.dirname(os.path.abspath(__file__))
        base_dir = os.path.join(script_dir, '..', '..', '..', '..')
        patients_dir = os.path.abspath(os.path.join(base_dir, 'Patients'))
        
        if not os.path.exists(patients_dir):
            print(f"⚠️ Patients directory not found: {patients_dir}")
            return
        
        # Find all patients with EOS images
        for patient_id in sorted(os.listdir(patients_dir)):
            patient_path = os.path.join(patients_dir, patient_id)
            eos_path = os.path.join(patient_path, 'EOS')
            
            if os.path.isdir(patient_path) and os.path.exists(eos_path):
                # Check for valid image files
                has_images = any(
                    f.endswith(('_C.jpg', '_C.jpeg', '_C.tif', '_C.png', '_S.jpg', '_S.jpeg', '_S.tif', '_S.png'))
                    for f in os.listdir(eos_path)
                )
                if has_images:
                    self.patient_combo.addItem(patient_id, eos_path)
        
        if self.patient_combo.count() > 0:
            self.on_patient_changed(0)
            
    def on_patient_changed(self, index):
        """Handle patient selection change"""
        if index < 0:
            return
            
        self.current_patient = self.patient_combo.currentText()
        eos_path = self.patient_combo.currentData()
        
        if not eos_path:
            return
            
        # Load images
        self.load_patient_images(eos_path)
        self.update_3d_view()
        
    def load_patient_images(self, eos_path):
        """Load frontal and lateral images for the patient"""
        self.frontal_image = None
        self.lateral_image = None
        self.frontal_path = None
        self.lateral_path = None
        
        # Find frontal (AP) image
        for ext in ['_C.jpg', '_C.jpeg', '_C.tif', '_C.png']:
            potential_files = [f for f in os.listdir(eos_path) if f.endswith(ext)]
            if potential_files:
                self.frontal_path = os.path.join(eos_path, potential_files[0])
                self.frontal_image = cv2.imread(self.frontal_path)
                if self.frontal_image is not None:
                    self.frontal_image = cv2.cvtColor(self.frontal_image, cv2.COLOR_BGR2RGB)
                break
        
        # Find lateral (Sagittal) image
        for ext in ['_S.jpg', '_S.jpeg', '_S.tif', '_S.png', '_L.jpg', '_L.jpeg', '_L.tif', '_L.png']:
            potential_files = [f for f in os.listdir(eos_path) if f.endswith(ext)]
            if potential_files:
                self.lateral_path = os.path.join(eos_path, potential_files[0])
                self.lateral_image = cv2.imread(self.lateral_path)
                if self.lateral_image is not None:
                    self.lateral_image = cv2.cvtColor(self.lateral_image, cv2.COLOR_BGR2RGB)
                break
        
        # Update info
        info_text = f"Patient: {self.current_patient}\n"
        if self.frontal_image is not None:
            h, w = self.frontal_image.shape[:2]
            info_text += f"Frontal: {w}×{h}px\n"
        else:
            info_text += "Frontal: Not found\n"
            
        if self.lateral_image is not None:
            h, w = self.lateral_image.shape[:2]
            info_text += f"Lateral: {w}×{h}px\n"
        else:
            info_text += "Lateral: Not found\n"
            
        self.info_label.setText(info_text)
        
    def update_3d_view(self):
        """Update the 3D visualization"""
        self.setup_3d_axes()
        
        # Draw coordinate axes if enabled
        if self.show_axes_cb.isChecked():
            axis_length = 500
            self.ax.quiver(0, 0, 0, axis_length, 0, 0, color='r', arrow_length_ratio=0.1, linewidth=2, label='X')
            self.ax.quiver(0, 0, 0, 0, axis_length, 0, color='g', arrow_length_ratio=0.1, linewidth=2, label='Y')
            self.ax.quiver(0, 0, 0, 0, 0, axis_length, color='b', arrow_length_ratio=0.1, linewidth=2, label='Z')
        
        # Plot frontal image if available and enabled
        if self.show_frontal_cb.isChecked() and self.frontal_image is not None:
            self.plot_image_plane(self.frontal_image, 'frontal')
        
        # Plot lateral image if available and enabled
        if self.show_lateral_cb.isChecked() and self.lateral_image is not None:
            self.plot_image_plane(self.lateral_image, 'lateral')
        
        # Update canvas
        self.canvas.draw()
        
    def plot_image_plane(self, image, plane_type):
        """
        Plot an image as a plane in 3D space
        
        Args:
            image: Image array (H, W, 3)
            plane_type: 'frontal' or 'lateral'
        """
        h, w = image.shape[:2]
        
        # Scale factor: assume typical EOS image height represents ~1800mm
        pixel_to_mm = 1800 / h
        
        width_mm = w * pixel_to_mm
        height_mm = h * pixel_to_mm
        
        if plane_type == 'frontal':
            # Frontal image in YZ plane (perpendicular to X axis)
            # Origin at top-left corner
            x_pos = -self.image_separation / 2
            
            # Create mesh grid
            y = np.linspace(0, width_mm, w)
            z = np.linspace(0, -height_mm, h)  # Negative because image coordinate system has Y down
            Y, Z = np.meshgrid(y, z)
            X = np.full_like(Y, x_pos)
            
            # Plot surface with image texture
            self.ax.plot_surface(X, Y, Z, facecolors=image/255.0, 
                               shade=False, rstride=1, cstride=1, alpha=0.8)
            
            # Draw frame
            self.draw_image_frame(x_pos, 0, 0, 0, width_mm, -height_mm, 'frontal')
            
        elif plane_type == 'lateral':
            # Lateral image in XZ plane (perpendicular to Y axis)
            # Origin at top-left corner
            y_pos = self.image_separation / 2
            
            # Create mesh grid
            x = np.linspace(0, width_mm, w)
            z = np.linspace(0, -height_mm, h)  # Negative because image coordinate system has Y down
            X, Z = np.meshgrid(x, z)
            Y = np.full_like(X, y_pos)
            
            # Plot surface with image texture
            self.ax.plot_surface(X, Y, Z, facecolors=image/255.0,
                               shade=False, rstride=1, cstride=1, alpha=0.8)
            
            # Draw frame
            self.draw_image_frame(0, y_pos, 0, width_mm, 0, -height_mm, 'lateral')
    
    def draw_image_frame(self, x0, y0, z0, dx, dy, dz, plane_type):
        """Draw a frame around the image plane"""
        color = 'cyan' if plane_type == 'frontal' else 'magenta'
        
        # Define corners
        corners = [
            [x0, y0, z0],
            [x0 + dx, y0 + dy, z0],
            [x0 + dx, y0 + dy, z0 + dz],
            [x0, y0, z0 + dz],
            [x0, y0, z0]  # Close the frame
        ]
        
        corners = np.array(corners)
        self.ax.plot(corners[:, 0], corners[:, 1], corners[:, 2], 
                    color=color, linewidth=2, label=f'{plane_type.capitalize()} Frame')
    
    def on_separation_changed(self, value):
        """Handle image separation slider change"""
        self.image_separation = value
        self.separation_value_label.setText(f"{value} mm")
        self.update_3d_view()
        
    def on_distance_changed(self, value):
        """Handle distance slider change"""
        self.frontal_distance = value
        self.lateral_distance = value
        self.distance_value_label.setText(f"{value} mm")
        self.update_3d_view()
        
    def reset_view(self):
        """Reset the 3D view to default"""
        self.ax.view_init(elev=20, azim=45)
        self.canvas.draw()
        
    def export_view(self):
        """Export the current 3D view as an image"""
        from PyQt5.QtWidgets import QFileDialog
        
        script_dir = os.path.dirname(os.path.abspath(__file__))
        base_dir = os.path.join(script_dir, '..', '..', '..', '..')
        default_dir = os.path.abspath(os.path.join(base_dir, 'Patients'))
        
        filename, _ = QFileDialog.getSaveFileName(
            self, "Export 3D View", default_dir,
            "PNG Image (*.png);;JPEG Image (*.jpg);;All Files (*)"
        )
        
        if filename:
            self.figure.savefig(filename, dpi=300, facecolor='#2b2b2b', edgecolor='none')
            print(f"✅ Exported 3D view to: {filename}")


def main():
    """Standalone test function"""
    from PyQt5.QtWidgets import QApplication
    
    app = QApplication(sys.argv)
    app.setStyle('Fusion')
    
    viewer = Registration3DViewer()
    viewer.showMaximized()
    
    sys.exit(app.exec_())


if __name__ == '__main__':
    main()
