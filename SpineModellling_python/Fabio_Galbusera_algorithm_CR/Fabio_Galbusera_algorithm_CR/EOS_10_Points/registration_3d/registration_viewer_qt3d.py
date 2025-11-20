"""
3D Registration Viewer for EOS Images using PyQt3D
Displays frontal and lateral X-ray images in 3D space with proper coordinate system
"""
import sys
import os
import numpy as np
from PyQt5.QtWidgets import (QMainWindow, QWidget, QVBoxLayout, QHBoxLayout, 
                             QPushButton, QLabel, QComboBox, QSlider, QCheckBox,
                             QGroupBox, QFileDialog)
from PyQt5.QtCore import Qt, QUrl
from PyQt5.QtGui import QFont, QVector3D, QQuaternion, QColor
from PyQt5.Qt3DCore import QEntity, QTransform
from PyQt5.Qt3DExtras import (Qt3DWindow, QFirstPersonCameraController, 
                               QPhongMaterial, QCuboidMesh, QSphereMesh,
                               QCylinderMesh, QOrbitCameraController)
from PyQt5.Qt3DRender import (QCamera, QTexture2D, QTextureImage, 
                               QMaterial, QEffect, QTechnique, QRenderPass,
                               QParameter, QMesh, QGeometryRenderer)
import cv2


class Registration3DViewer(QMainWindow):
    """
    3D Registration viewer for EOS images using PyQt3D
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
        
        geometry_group.setLayout(geometry_layout)
        left_layout.addWidget(geometry_group)
        
        # Action buttons
        button_layout = QVBoxLayout()
        
        self.reset_view_btn = QPushButton("Reset View")
        self.reset_view_btn.clicked.connect(self.reset_view)
        button_layout.addWidget(self.reset_view_btn)
        
        left_layout.addLayout(button_layout)
        left_layout.addStretch()
        
        # Info label
        self.info_label = QLabel("Load a patient to begin")
        self.info_label.setWordWrap(True)
        self.info_label.setStyleSheet("color: #888; font-size: 10px;")
        left_layout.addWidget(self.info_label)
        
        main_layout.addWidget(left_panel)
        
        # Right panel - 3D visualization using Qt3D
        self.view_3d = Qt3DWindow()
        self.view_3d.defaultFrameGraph().setClearColor(QColor(43, 43, 43))
        
        # Create container for Qt3DWindow
        container = QWidget.createWindowContainer(self.view_3d)
        main_layout.addWidget(container, stretch=1)
        
        # Setup 3D scene
        self.setup_3d_scene()
        
    def setup_3d_scene(self):
        """Setup the Qt3D scene"""
        # Root entity
        self.root_entity = QEntity()
        
        # Camera
        self.camera = self.view_3d.camera()
        self.camera.lens().setPerspectiveProjection(45.0, 16.0/9.0, 0.1, 10000.0)
        self.camera.setPosition(QVector3D(2000, 1000, 2000))
        self.camera.setViewCenter(QVector3D(0, 0, 0))
        
        # Camera controller
        self.cam_controller = QOrbitCameraController(self.root_entity)
        self.cam_controller.setLinearSpeed(500.0)
        self.cam_controller.setLookSpeed(180.0)
        self.cam_controller.setCamera(self.camera)
        
        # Create coordinate axes
        self.create_axes()
        
        # Set root entity
        self.view_3d.setRootEntity(self.root_entity)
        
    def create_axes(self):
        """Create coordinate axes"""
        if not self.show_axes_cb.isChecked():
            return
            
        axis_length = 500.0
        axis_radius = 5.0
        
        # X axis (Red)
        x_axis = self.create_axis(
            QVector3D(axis_length/2, 0, 0),
            QVector3D(0, 0, 90),
            axis_length, axis_radius,
            QColor(255, 0, 0)
        )
        
        # Y axis (Green)
        y_axis = self.create_axis(
            QVector3D(0, axis_length/2, 0),
            QVector3D(0, 0, 0),
            axis_length, axis_radius,
            QColor(0, 255, 0)
        )
        
        # Z axis (Blue)
        z_axis = self.create_axis(
            QVector3D(0, 0, axis_length/2),
            QVector3D(90, 0, 0),
            axis_length, axis_radius,
            QColor(0, 0, 255)
        )
        
    def create_axis(self, position, rotation, length, radius, color):
        """Create a single axis cylinder"""
        axis_entity = QEntity(self.root_entity)
        
        # Mesh
        cylinder = QCylinderMesh()
        cylinder.setRadius(radius)
        cylinder.setLength(length)
        
        # Transform
        transform = QTransform()
        transform.setTranslation(position)
        transform.setRotationX(rotation.x())
        transform.setRotationY(rotation.y())
        transform.setRotationZ(rotation.z())
        
        # Material
        material = QPhongMaterial()
        material.setDiffuse(color)
        material.setAmbient(color)
        
        axis_entity.addComponent(cylinder)
        axis_entity.addComponent(transform)
        axis_entity.addComponent(material)
        
        return axis_entity
        
    def create_image_plane(self, position, rotation, width, height, color):
        """Create a plane to represent an image"""
        plane_entity = QEntity(self.root_entity)
        
        # Use a flat cuboid as a plane
        cuboid = QCuboidMesh()
        cuboid.setXExtent(width)
        cuboid.setYExtent(height)
        cuboid.setZExtent(1.0)  # Very thin
        
        # Transform
        transform = QTransform()
        transform.setTranslation(position)
        transform.setRotationX(rotation.x())
        transform.setRotationY(rotation.y())
        transform.setRotationZ(rotation.z())
        
        # Material
        material = QPhongMaterial()
        material.setDiffuse(color)
        material.setAmbient(color.darker(150))
        material.setSpecular(QColor(50, 50, 50))
        material.setShininess(10.0)
        
        plane_entity.addComponent(cuboid)
        plane_entity.addComponent(transform)
        plane_entity.addComponent(material)
        
        return plane_entity
        
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
        # Clear and recreate scene
        self.root_entity = QEntity()
        
        # Recreate axes
        if self.show_axes_cb.isChecked():
            self.create_axes()
        
        # Plot frontal image plane if available and enabled
        if self.show_frontal_cb.isChecked() and self.frontal_image is not None:
            h, w = self.frontal_image.shape[:2]
            pixel_to_mm = 1800 / h
            width_mm = w * pixel_to_mm
            height_mm = h * pixel_to_mm
            
            # Frontal plane (YZ plane, perpendicular to X axis)
            self.create_image_plane(
                QVector3D(-self.image_separation / 2, width_mm / 2, -height_mm / 2),
                QVector3D(0, 90, 0),  # Rotate to face +X
                height_mm, width_mm,
                QColor(0, 200, 255, 180)  # Cyan with transparency
            )
        
        # Plot lateral image plane if available and enabled
        if self.show_lateral_cb.isChecked() and self.lateral_image is not None:
            h, w = self.lateral_image.shape[:2]
            pixel_to_mm = 1800 / h
            width_mm = w * pixel_to_mm
            height_mm = h * pixel_to_mm
            
            # Lateral plane (XZ plane, perpendicular to Y axis)
            self.create_image_plane(
                QVector3D(width_mm / 2, self.image_separation / 2, -height_mm / 2),
                QVector3D(90, 0, 0),  # Rotate to face -Y
                width_mm, height_mm,
                QColor(255, 0, 255, 180)  # Magenta with transparency
            )
        
        # Update scene
        self.view_3d.setRootEntity(self.root_entity)
        
        # Reset camera controller
        self.cam_controller = QOrbitCameraController(self.root_entity)
        self.cam_controller.setLinearSpeed(500.0)
        self.cam_controller.setLookSpeed(180.0)
        self.cam_controller.setCamera(self.camera)
        
    def on_separation_changed(self, value):
        """Handle image separation slider change"""
        self.image_separation = value
        self.separation_value_label.setText(f"{value} mm")
        self.update_3d_view()
        
    def reset_view(self):
        """Reset the 3D view to default"""
        self.camera.setPosition(QVector3D(2000, 1000, 2000))
        self.camera.setViewCenter(QVector3D(0, 0, 0))


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
