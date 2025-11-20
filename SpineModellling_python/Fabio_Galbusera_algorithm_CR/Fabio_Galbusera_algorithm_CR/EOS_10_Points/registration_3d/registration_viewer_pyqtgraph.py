"""
3D Registration Viewer for EOS Images using PyQtGraph
Displays frontal and lateral X-ray images in 3D space with proper coordinate system
"""
import sys
import os
import numpy as np
import pyqtgraph as pg
import pyqtgraph.opengl as gl
from PyQt5.QtWidgets import (QMainWindow, QWidget, QVBoxLayout, QHBoxLayout, 
                             QPushButton, QLabel, QComboBox, QSlider, QCheckBox,
                             QGroupBox, QListWidget, QListWidgetItem, QSpinBox,
                             QDoubleSpinBox, QSplitter)
from PyQt5.QtCore import Qt
from PyQt5.QtGui import QFont, QVector3D, QMatrix4x4, QVector4D
import cv2
import pydicom
from stl import mesh as stl_mesh


class CustomGLViewWidget(gl.GLViewWidget):
    """Custom GLViewWidget to handle mouse events for gizmo interaction"""
    def __init__(self, parent=None):
        super().__init__(parent)
        self.viewer = None
        
    def set_viewer(self, viewer):
        self.viewer = viewer
        
    def mousePressEvent(self, ev):
        if self.viewer and self.viewer.handle_mouse_press(ev):
            return
        super().mousePressEvent(ev)
        
    def mouseMoveEvent(self, ev):
        if self.viewer and self.viewer.handle_mouse_move(ev):
            return
        super().mouseMoveEvent(ev)
        
    def mouseReleaseEvent(self, ev):
        if self.viewer and self.viewer.handle_mouse_release(ev):
            return
        super().mouseReleaseEvent(ev)

class CustomGLViewWidget(gl.GLViewWidget):
    """Custom GLViewWidget to handle mouse events for gizmo interaction"""
    def __init__(self, parent=None):
        super().__init__(parent)
        self.viewer = None
        
    def set_viewer(self, viewer):
        self.viewer = viewer
        
    def mousePressEvent(self, ev):
        if self.viewer and self.viewer.handle_mouse_press(ev):
            return
        super().mousePressEvent(ev)
        
    def mouseMoveEvent(self, ev):
        if self.viewer and self.viewer.handle_mouse_move(ev):
            return
        super().mouseMoveEvent(ev)
        
    def mouseReleaseEvent(self, ev):
        if self.viewer and self.viewer.handle_mouse_release(ev):
            return
        super().mouseReleaseEvent(ev)


class Registration3DViewer(QMainWindow):
    """
    3D Registration viewer for EOS images using PyQtGraph
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
        
        # 3D objects
        self.frontal_plane = None
        self.lateral_plane = None
        self.axis_lines = []
        
        # STL mesh objects
        self.stl_meshes = {}  # Dict: filename -> mesh item
        self.stl_data = {}     # Dict: filename -> mesh data
        self.selected_stl = None
        
        # 3D markers for projection visualization
        self.marker_items = []  # List of 3D marker mesh items
        self.projection_items = {'frontal': None, 'lateral': None} # Items for projected outlines
        self.landmark_items = [] # List of 3D landmark items loaded from file
        
        # Gizmo state
        self.gizmo = None
        self.gizmo_scale = 100.0
        self.dragging_axis = None
        self.last_mouse_pos = None
        
        # Image positioning (in mm, typical EOS setup)
        self.image_separation = 0   # Images touching (no separation)
        
        # EOS imaging parameters (defaults, will be updated from actual images)
        self.distance_source_to_isocenter_frontal = 900.0  # mm (DSTI for frontal/AP image)
        self.distance_source_to_isocenter_lateral = 900.0  # mm (DSTI for lateral/sagittal image)
        self.pixel_spacing_frontal = (0.17, 0.17)  # mm/pixel (X, Y)
        self.pixel_spacing_lateral = (0.17, 0.17)  # mm/pixel (X, Y)
        
        self.init_ui()
        self.load_patients()
    
    # ==================== PROJECTION METHODS ====================
    
    def convert_pixel_to_mm(self, pixel_value, pixel_spacing):
        """
        Convert pixel coordinates to millimeters
        
        Args:
            pixel_value: Pixel coordinate value
            pixel_spacing: Spacing in mm/pixel
            
        Returns:
            float: Distance in millimeters
        """
        return float(pixel_value) * pixel_spacing
    
    def convert_mm_to_pixel(self, mm_value, pixel_spacing):
        """
        Convert millimeters to pixel coordinates
        
        Args:
            mm_value: Distance in millimeters
            pixel_spacing: Spacing in mm/pixel
            
        Returns:
            int: Pixel coordinate (rounded)
        """
        return int(round(mm_value / pixel_spacing))
    
    def project_3d_to_2d(self, x_real, z_real):
        """
        Project 3D coordinates to 2D image coordinates (3D → 2D)
        Perspective projection from 3D EOS space to image planes
        
        Based on EosSpace.cs Project method (lines 323-327)
        
        Args:
            x_real: Real X coordinate in 3D EOS space (mm)
            z_real: Real Z coordinate in 3D EOS space (mm)
            
        Returns:
            tuple: (x_projected, z_projected) - Projected coordinates on image planes (mm)
                   x_projected: Position on frontal image
                   z_projected: Position on lateral image
        """
        # Frontal projection (X coordinate on frontal image)
        x_projected = (x_real / (self.distance_source_to_isocenter_frontal + z_real)) * \
                      self.distance_source_to_isocenter_frontal
        
        # Lateral projection (Z coordinate on lateral image)  
        z_projected = (z_real / (self.distance_source_to_isocenter_lateral + x_real)) * \
                      self.distance_source_to_isocenter_lateral
        
        return x_projected, z_projected
    
    def inverse_project_2d_to_3d(self, x_projected, z_projected):
        """
        Inverse projection from 2D image coordinates to 3D space (2D → 3D)
        Ray triangulation from dual X-ray projections
        
        Based on UC_measurementsMain.cs InverseProject method (lines 513-546)
        and EosSpace.cs InverseProject method (lines 340+)
        
        Args:
            x_projected: Projected X coordinate on frontal image (mm)
            z_projected: Projected Z coordinate on lateral image (mm)
            
        Returns:
            tuple: (x_real, z_real) - Real 3D coordinates (mm)
        """
        # Handle division by zero case
        if x_projected == 0:
            slope_lateral = 1e12  # Very large number instead of infinity
        else:
            # Slope of ray from lateral X-ray source through projection point
            slope_lateral = (0 - (-self.distance_source_to_isocenter_frontal)) / (x_projected - 0)
        
        # Slope of ray from frontal X-ray source through projection point
        slope_frontal = ((-z_projected - 0) / (0 - self.distance_source_to_isocenter_lateral))
        
        # Find intersection of the two rays (triangulation)
        x_real = ((-slope_frontal * self.distance_source_to_isocenter_lateral) - 
                  (-self.distance_source_to_isocenter_frontal)) / (slope_lateral - slope_frontal)
        
        z_real = slope_lateral * x_real + (-self.distance_source_to_isocenter_frontal)
        
        return x_real, z_real
    
    def reconstruct_3d_from_pixels(self, frontal_pixel_x, frontal_pixel_y, lateral_pixel_x, lateral_pixel_y):
        """
        Reconstruct 3D coordinates from pixel coordinates on the unprocessed images.
        
        Pipeline:
        1. Unprocessed Pixel Space -> Centered MM Space (using PixelSpacing)
        2. Centered MM Space -> 3D Space (using Inverse Projection)
        
        Args:
            frontal_pixel_x: X pixel coordinate on frontal image
            frontal_pixel_y: Y pixel coordinate on frontal image  
            lateral_pixel_x: X pixel coordinate on lateral image
            lateral_pixel_y: Y pixel coordinate on lateral image
            
        Returns:
            tuple: (x, y, z) - 3D coordinates in EOS space (mm)
        """
        if self.frontal_image is None or self.lateral_image is None:
            return None
        
        # Get dimensions of the unprocessed images
        frontal_h, frontal_w = self.frontal_image.shape[:2]
        lateral_h, lateral_w = self.lateral_image.shape[:2]
        
        # 1. Convert Pixels to MM (Centered)
        
        # Frontal X (Lateral-Medial axis)
        # Pixel 0 is Left. Center is w/2.
        # We want distance from center.
        # Note: The original code used (w/2 - px), which gives + for Left, - for Right.
        # We preserve this convention.
        m_frontal_x = self.convert_pixel_to_mm(
            (frontal_w / 2) - frontal_pixel_x, 
            self.pixel_spacing_frontal[0]
        )
        
        # Frontal Y (Vertical axis)
        # Pixel 0 is Top. Height is h.
        # We want 0 at center, + Up, - Down.
        # This matches create_image_plane which centers the image at Z=0.
        m_frontal_y = self.convert_pixel_to_mm(
            (frontal_h / 2) - frontal_pixel_y,
            self.pixel_spacing_frontal[1]
        )
        
        # Lateral X (Anterior-Posterior axis)
        # Pixel 0 is Posterior? (depends on patient orientation).
        # Original code used (w/2 - px).
        m_lateral_x = self.convert_pixel_to_mm(
            (lateral_w / 2) - lateral_pixel_x,
            self.pixel_spacing_lateral[0]
        )
        
        # Lateral Y (Vertical axis)
        m_lateral_y = self.convert_pixel_to_mm(
            (lateral_h / 2) - lateral_pixel_y,
            self.pixel_spacing_lateral[1]
        )
        
        # 2. Inverse Project to 3D
        # We pass -m_frontal_x because the projection math expects X to be Right+, 
        # but m_frontal_x is Left+.
        x_3d, z_3d = self.inverse_project_2d_to_3d(-m_frontal_x, m_lateral_x)
        
        # Y is average of both views
        y_3d = (m_frontal_y + m_lateral_y) / 2.0
        
        # Return in EOS coordinate system (x, y, z)
        # Note: The original code returned (-x_3d, y_3d, z_3d)
        return -x_3d, y_3d, z_3d
    
    def coords_3d_to_image_pixel(self, x_3d, y_3d, z_3d):
        """
        Convert 3D space coordinates to image pixel coordinates
        Complete pipeline: 3D → Projection → Pixel
        
        Args:
            x_3d: X coordinate in 3D EOS space (mm)
            y_3d: Y coordinate in 3D EOS space (mm)
            z_3d: Z coordinate in 3D EOS space (mm)
            
        Returns:
            dict: {
                'frontal': (pixel_x, pixel_y),
                'lateral': (pixel_x, pixel_y)
            }
        """
        if self.frontal_image is None or self.lateral_image is None:
            return None
        
        # Project 3D point to 2D image planes
        x_proj, z_proj = self.project_3d_to_2d(x_3d, z_3d)
        
        frontal_h, frontal_w = self.frontal_image.shape[:2]
        lateral_h, lateral_w = self.lateral_image.shape[:2]
        
        # Convert to pixels for frontal image
        frontal_pixel_x = (frontal_w / 2) - self.convert_mm_to_pixel(
            x_proj, 
            self.pixel_spacing_frontal[0]
        )
        frontal_pixel_y = frontal_h - self.convert_mm_to_pixel(
            y_3d,
            self.pixel_spacing_frontal[1]
        )
        
        # Convert to pixels for lateral image
        lateral_pixel_x = (lateral_w / 2) + self.convert_mm_to_pixel(
            z_proj,
            self.pixel_spacing_lateral[0]
        )
        lateral_pixel_y = lateral_h - self.convert_mm_to_pixel(
            y_3d,
            self.pixel_spacing_lateral[1]
        )
        
        return {
            'frontal': (int(frontal_pixel_x), int(frontal_pixel_y)),
            'lateral': (int(lateral_pixel_x), int(lateral_pixel_y))
        }
    
    # ==================== END PROJECTION METHODS ====================
        
    def init_ui(self):
        """Initialize the user interface"""
        central_widget = QWidget()
        self.setCentralWidget(central_widget)
        
        main_layout = QHBoxLayout(central_widget)
        
        # Create splitter for resizable panels
        splitter = QSplitter(Qt.Horizontal)
        
        # Left panel - STL Layers (like Photoshop layers)
        left_panel = QWidget()
        left_layout = QVBoxLayout(left_panel)
        left_layout.setContentsMargins(10, 10, 10, 10)
        left_panel.setMaximumWidth(350)
        
        # STL Layers Group
        stl_group = QGroupBox("3D Models (Vertebrae)")
        stl_layout = QVBoxLayout()
        
        self.stl_list = QListWidget()
        self.stl_list.setSelectionMode(QListWidget.SingleSelection)
        self.stl_list.itemSelectionChanged.connect(self.on_stl_selection_changed)
        self.stl_list.itemChanged.connect(self.on_stl_visibility_changed) # Connect visibility change
        stl_layout.addWidget(self.stl_list)
        
        # Visibility controls for each layer
        visibility_layout = QHBoxLayout()
        self.show_all_stl_btn = QPushButton("Show All")
        self.show_all_stl_btn.clicked.connect(self.show_all_stl)
        self.hide_all_stl_btn = QPushButton("Hide All")
        self.hide_all_stl_btn.clicked.connect(self.hide_all_stl)
        visibility_layout.addWidget(self.show_all_stl_btn)
        visibility_layout.addWidget(self.hide_all_stl_btn)
        stl_layout.addLayout(visibility_layout)
        
        # Refresh button
        self.refresh_stl_btn = QPushButton("Refresh STL List")
        self.refresh_stl_btn.clicked.connect(self.load_stl_files)
        stl_layout.addWidget(self.refresh_stl_btn)
        
        # Toggle Landmarks Button
        self.toggle_landmarks_btn = QPushButton("Toggle Landmarks")
        self.toggle_landmarks_btn.setCheckable(True)
        self.toggle_landmarks_btn.setChecked(True)
        self.toggle_landmarks_btn.clicked.connect(self.toggle_landmarks)
        stl_layout.addWidget(self.toggle_landmarks_btn)
        
        stl_group.setLayout(stl_layout)
        left_layout.addWidget(stl_group)
        
        # Transform Controls Group
        transform_group = QGroupBox("Transform Selected Model")
        transform_layout = QVBoxLayout()
        
        # Translation X
        tx_layout = QHBoxLayout()
        tx_layout.addWidget(QLabel("X:"))
        self.tx_spin = QDoubleSpinBox()
        self.tx_spin.setRange(-1000, 1000)
        self.tx_spin.setSingleStep(1.0)
        self.tx_spin.setValue(0)
        self.tx_spin.valueChanged.connect(self.update_selected_stl_transform)
        tx_layout.addWidget(self.tx_spin)
        self.tx_left_btn = QPushButton("◄")
        self.tx_left_btn.clicked.connect(lambda: self.nudge_stl('x', -5))
        self.tx_right_btn = QPushButton("►")
        self.tx_right_btn.clicked.connect(lambda: self.nudge_stl('x', 5))
        tx_layout.addWidget(self.tx_left_btn)
        tx_layout.addWidget(self.tx_right_btn)
        transform_layout.addLayout(tx_layout)
        
        # Translation Y
        ty_layout = QHBoxLayout()
        ty_layout.addWidget(QLabel("Y:"))
        self.ty_spin = QDoubleSpinBox()
        self.ty_spin.setRange(-1000, 1000)
        self.ty_spin.setSingleStep(1.0)
        self.ty_spin.setValue(0)
        self.ty_spin.valueChanged.connect(self.update_selected_stl_transform)
        ty_layout.addWidget(self.ty_spin)
        self.ty_down_btn = QPushButton("▼")
        self.ty_down_btn.clicked.connect(lambda: self.nudge_stl('y', -5))
        self.ty_up_btn = QPushButton("▲")
        self.ty_up_btn.clicked.connect(lambda: self.nudge_stl('y', 5))
        ty_layout.addWidget(self.ty_down_btn)
        ty_layout.addWidget(self.ty_up_btn)
        transform_layout.addLayout(ty_layout)
        
        # Translation Z
        tz_layout = QHBoxLayout()
        tz_layout.addWidget(QLabel("Z:"))
        self.tz_spin = QDoubleSpinBox()
        self.tz_spin.setRange(-1000, 1000)
        self.tz_spin.setSingleStep(1.0)
        self.tz_spin.setValue(0)
        self.tz_spin.valueChanged.connect(self.update_selected_stl_transform)
        tz_layout.addWidget(self.tz_spin)
        self.tz_down_btn = QPushButton("▼")
        self.tz_down_btn.clicked.connect(lambda: self.nudge_stl('z', -5))
        self.tz_up_btn = QPushButton("▲")
        self.tz_up_btn.clicked.connect(lambda: self.nudge_stl('z', 5))
        tz_layout.addWidget(self.tz_down_btn)
        tz_layout.addWidget(self.tz_up_btn)
        transform_layout.addLayout(tz_layout)
        
        # Rotation X
        rx_layout = QHBoxLayout()
        rx_layout.addWidget(QLabel("Rot X:"))
        self.rx_spin = QDoubleSpinBox()
        self.rx_spin.setRange(-360, 360)
        self.rx_spin.setSingleStep(1.0)
        self.rx_spin.setValue(0)
        self.rx_spin.valueChanged.connect(self.update_selected_stl_transform)
        rx_layout.addWidget(self.rx_spin)
        transform_layout.addLayout(rx_layout)

        # Rotation Y
        ry_layout = QHBoxLayout()
        ry_layout.addWidget(QLabel("Rot Y:"))
        self.ry_spin = QDoubleSpinBox()
        self.ry_spin.setRange(-360, 360)
        self.ry_spin.setSingleStep(1.0)
        self.ry_spin.setValue(0)
        self.ry_spin.valueChanged.connect(self.update_selected_stl_transform)
        ry_layout.addWidget(self.ry_spin)
        transform_layout.addLayout(ry_layout)

        # Rotation Z
        rz_layout = QHBoxLayout()
        rz_layout.addWidget(QLabel("Rot Z:"))
        self.rz_spin = QDoubleSpinBox()
        self.rz_spin.setRange(-360, 360)
        self.rz_spin.setSingleStep(1.0)
        self.rz_spin.setValue(0)
        self.rz_spin.valueChanged.connect(self.update_selected_stl_transform)
        rz_layout.addWidget(self.rz_spin)
        transform_layout.addLayout(rz_layout)
        
        # Reset button
        self.reset_transform_btn = QPushButton("Reset Transform")
        self.reset_transform_btn.clicked.connect(self.reset_selected_stl_transform)
        transform_layout.addWidget(self.reset_transform_btn)
        
        transform_group.setLayout(transform_layout)
        left_layout.addWidget(transform_group)
        
        left_layout.addStretch()
        
        splitter.addWidget(left_panel)
        
        # Middle panel - Controls
        middle_panel = QWidget()
        middle_layout = QVBoxLayout(middle_panel)
        middle_layout.setContentsMargins(10, 10, 10, 10)
        middle_panel.setMaximumWidth(300)
        
        # Patient selection
        patient_group = QGroupBox("Patient Selection")
        patient_layout = QVBoxLayout()
        
        self.patient_label = QLabel("Select Patient:")
        patient_layout.addWidget(self.patient_label)
        
        self.patient_combo = QComboBox()
        self.patient_combo.currentIndexChanged.connect(self.on_patient_changed)
        patient_layout.addWidget(self.patient_combo)
        
        patient_group.setLayout(patient_layout)
        middle_layout.addWidget(patient_group)
        
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
        self.show_grid_cb.setChecked(True)
        self.show_grid_cb.stateChanged.connect(self.update_grid)
        view_layout.addWidget(self.show_grid_cb)
        
        view_group.setLayout(view_layout)
        middle_layout.addWidget(view_group)
        
        # Geometry controls - REMOVED (images always touching)
        
        # Projection Testing Group - REMOVED
        
        # Action buttons
        button_layout = QVBoxLayout()
        
        self.reset_view_btn = QPushButton("Reset View")
        self.reset_view_btn.clicked.connect(self.reset_view)
        button_layout.addWidget(self.reset_view_btn)
        
        middle_layout.addLayout(button_layout)
        middle_layout.addStretch()
        
        # Info label
        self.info_label = QLabel("Load a patient to begin\nSelect a vertebra to move it with the gizmo")
        self.info_label.setWordWrap(True)
        self.info_label.setStyleSheet("color: #888; font-size: 10px;")
        middle_layout.addWidget(self.info_label)
        
        splitter.addWidget(middle_panel)
        
        # Right panel - 3D visualization using PyQtGraph
        self.view_3d = CustomGLViewWidget() # Use custom widget for mouse handling
        self.view_3d.setCameraPosition(distance=2500, elevation=20, azimuth=45)
        self.view_3d.setBackgroundColor('k')
        self.view_3d.set_viewer(self) # Link back to viewer
        
        # Add grid (will be positioned dynamically based on image height)
        self.grid = gl.GLGridItem()
        self.grid.setSize(2000, 2000, 1)
        self.grid.setSpacing(100, 100, 1)
        self.view_3d.addItem(self.grid)
        
        splitter.addWidget(self.view_3d)
        
        # Set splitter sizes (left: 350px, middle: 300px, right: remaining)
        splitter.setSizes([350, 300, 1000])
        
        main_layout.addWidget(splitter)
        
        # Create coordinate axes
        self.create_axes()
        
    def create_axes(self):
        """Create coordinate axes"""
        if not self.show_axes_cb.isChecked():
            for line in self.axis_lines:
                self.view_3d.removeItem(line)
            self.axis_lines.clear()
            return
        
        # Clear existing axes
        for line in self.axis_lines:
            self.view_3d.removeItem(line)
        self.axis_lines.clear()
        
        axis_length = 500.0
        
        # X axis (Red)
        x_axis = gl.GLLinePlotItem(
            pos=np.array([[0, 0, 0], [axis_length, 0, 0]]),
            color=(1, 0, 0, 1),
            width=3,
            antialias=True
        )
        self.view_3d.addItem(x_axis)
        self.axis_lines.append(x_axis)
        
        # Y axis (Green)
        y_axis = gl.GLLinePlotItem(
            pos=np.array([[0, 0, 0], [0, axis_length, 0]]),
            color=(0, 1, 0, 1),
            width=3,
            antialias=True
        )
        self.view_3d.addItem(y_axis)
        self.axis_lines.append(y_axis)
        
        # Z axis (Blue)
        z_axis = gl.GLLinePlotItem(
            pos=np.array([[0, 0, 0], [0, 0, axis_length]]),
            color=(0, 0, 1, 1),
            width=3,
            antialias=True
        )
        self.view_3d.addItem(z_axis)
        self.axis_lines.append(z_axis)
        
    def update_grid(self):
        """Toggle grid visibility"""
        if self.show_grid_cb.isChecked():
            if self.grid not in self.view_3d.items:
                self.view_3d.addItem(self.grid)
        else:
            if self.grid in self.view_3d.items:
                self.view_3d.removeItem(self.grid)
    
    def update_grid_position(self, image_height_mm):
        """Update grid position based on image height"""
        # Position grid at the base of the images (bottom)
        grid_z = -image_height_mm / 2
        
        # Reset grid transform and reposition
        self.grid.resetTransform()
        self.grid.translate(0, 0, grid_z)
        
    def create_image_plane(self, image, position, rotation, scale=1.0):
        """Create a plane displaying an image using mesh with vertex colors"""
        if image is None:
            return None
        
        h, w = image.shape[:2]
        
        # Convert to grayscale if color
        if len(image.shape) == 3:
            # Convert to grayscale for display
            image_gray = np.mean(image, axis=2).astype(np.float32) / 255.0
        else:
            image_gray = image.astype(np.float32) / 255.0
        
        # Scale to millimeters
        pixel_to_mm = scale
        width_mm = w * pixel_to_mm
        height_mm = h * pixel_to_mm
        
        # Downsample image for performance (every 10th pixel)
        downsample = 10
        image_downsampled = image_gray[::downsample, ::downsample]
        h_down, w_down = image_downsampled.shape
        
        # Create mesh vertices
        vertices = []
        colors = []
        faces = []
        
        if rotation == 'frontal':
            # YZ plane (perpendicular to X axis)
            x = position[0]
            for i in range(h_down):
                for j in range(w_down):
                    y = (j * downsample * pixel_to_mm) - width_mm / 2
                    z = -((i * downsample * pixel_to_mm) - height_mm / 2)  # Flip Z
                    vertices.append([x, y, z])
                    
                    # Grayscale color
                    intensity = image_downsampled[i, j]
                    colors.append([intensity, intensity, intensity, 1.0])
            
        else:  # lateral
            # XZ plane (perpendicular to Y axis)
            y = position[1]
            for i in range(h_down):
                for j in range(w_down):
                    x = (j * downsample * pixel_to_mm) - width_mm / 2
                    z = -((i * downsample * pixel_to_mm) - height_mm / 2)  # Flip Z
                    vertices.append([x, y, z])
                    
                    # Grayscale color
                    intensity = image_downsampled[i, j]
                    colors.append([intensity, intensity, intensity, 1.0])
        
        # Create faces (triangles)
        for i in range(h_down - 1):
            for j in range(w_down - 1):
                idx = i * w_down + j
                # Two triangles per quad
                faces.append([idx, idx + 1, idx + w_down])
                faces.append([idx + 1, idx + w_down + 1, idx + w_down])
        
        vertices = np.array(vertices, dtype=np.float32)
        colors = np.array(colors, dtype=np.float32)
        faces = np.array(faces, dtype=np.uint32)
        
        # Create mesh data
        mesh_data = gl.MeshData(vertexes=vertices, faces=faces, vertexColors=colors)
        
        # Create mesh item
        mesh = gl.GLMeshItem(
            meshdata=mesh_data,
            smooth=False,
            drawEdges=False,
            glOptions='translucent'
        )
        
        return mesh
        
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
        
        # Load STL files for this patient
        self.load_stl_files()
        
        # Load Landmarks
        self.load_landmarks()
        
        # Check if images were loaded
        if self.frontal_image is None and self.lateral_image is None:
            from PyQt5.QtWidgets import QMessageBox
            QMessageBox.warning(
                self,
                "No Images Found",
                f"No EOS images found for patient {self.current_patient}.\n\n"
                "Please ensure the patient has frontal (_C) and/or lateral (_S) images in their EOS folder."
            )
            return
        
        self.update_3d_view()
        
    def extract_dicom_metadata(self, eos_path):
        """Extract metadata from DICOM files in the directory"""
        # Reset to defaults
        self.distance_source_to_isocenter_frontal = 900.0
        self.distance_source_to_isocenter_lateral = 900.0
        self.pixel_spacing_frontal = (0.17, 0.17)
        self.pixel_spacing_lateral = (0.17, 0.17)
        
        dcm_files = [f for f in os.listdir(eos_path) if f.lower().endswith('.dcm')]
        
        if not dcm_files:
            print("⚠️ No DICOM files found for metadata extraction. Using defaults.")
            return

        print(f"ℹ️ Found {len(dcm_files)} DICOM files. Extracting metadata...")
        
        for dcm_file in dcm_files:
            try:
                ds = pydicom.dcmread(os.path.join(eos_path, dcm_file))
                
                # Determine if frontal or lateral
                # Check filename or ViewPosition tag
                is_frontal = False
                is_lateral = False
                
                filename_lower = dcm_file.lower()
                if 'frontal' in filename_lower or 'ap' in filename_lower:
                    is_frontal = True
                elif 'lateral' in filename_lower or 'lat' in filename_lower:
                    is_lateral = True
                
                # If filename is ambiguous, check ViewPosition (0018,5101) if available
                # CS: AP, PA, LL, RL, RLD, LLD, RLO, LLO
                if not is_frontal and not is_lateral and 'ViewPosition' in ds:
                    view_pos = ds.ViewPosition
                    if view_pos in ['AP', 'PA']:
                        is_frontal = True
                    elif view_pos in ['LL', 'RL']:
                        is_lateral = True
                
                # Extract parameters
                # Distance Source to Isocenter (0018, 1110)
                dsti = 900.0
                if 'DistanceSourceToIsocenter' in ds:
                    dsti = float(ds.DistanceSourceToIsocenter)
                
                # Pixel Spacing (0028, 0030) - Row spacing (Y), Column spacing (X)
                pixel_spacing = (0.17, 0.17)
                if 'PixelSpacing' in ds:
                    # DICOM PixelSpacing is [RowSpacing, ColumnSpacing] -> [Y, X]
                    # We store as (X, Y) to match our convention
                    pixel_spacing = (float(ds.PixelSpacing[1]), float(ds.PixelSpacing[0]))
                
                if is_frontal:
                    self.distance_source_to_isocenter_frontal = dsti
                    self.pixel_spacing_frontal = pixel_spacing
                    print(f"  ✅ Frontal Metadata ({dcm_file}): DSTI={dsti}mm, Spacing={pixel_spacing}")
                elif is_lateral:
                    self.distance_source_to_isocenter_lateral = dsti
                    self.pixel_spacing_lateral = pixel_spacing
                    print(f"  ✅ Lateral Metadata ({dcm_file}): DSTI={dsti}mm, Spacing={pixel_spacing}")
                else:
                    print(f"  ⚠️ Unknown view for {dcm_file}. Metadata ignored.")
                    
            except Exception as e:
                print(f"  ❌ Error reading {dcm_file}: {e}")

    def load_patient_images(self, eos_path):
        """Load frontal and lateral images for the patient"""
        self.frontal_image = None
        self.lateral_image = None
        self.frontal_path = None
        self.lateral_path = None
        
        # Extract DICOM metadata first
        self.extract_dicom_metadata(eos_path)
        
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
        # Remove existing image planes
        if self.frontal_plane is not None:
            self.view_3d.removeItem(self.frontal_plane)
            self.frontal_plane = None
            
        if self.lateral_plane is not None:
            self.view_3d.removeItem(self.lateral_plane)
            self.lateral_plane = None
        
        # Update axes
        self.create_axes()
        
        image_height_mm = 1800.0  # Default fallback
        
        # Plot frontal image plane if available and enabled
        if self.show_frontal_cb.isChecked() and self.frontal_image is not None:
            h, w = self.frontal_image.shape[:2]
            # Use extracted pixel spacing (Y component)
            pixel_to_mm = self.pixel_spacing_frontal[1]
            image_height_mm = h * pixel_to_mm
            
            self.frontal_plane = self.create_image_plane(
                self.frontal_image,
                position=[0, 0, 0],  # Center at origin
                rotation='frontal',
                scale=pixel_to_mm
            )
            if self.frontal_plane:
                self.view_3d.addItem(self.frontal_plane)
        
        # Plot lateral image plane if available and enabled
        if self.show_lateral_cb.isChecked() and self.lateral_image is not None:
            h, w = self.lateral_image.shape[:2]
            # Use extracted pixel spacing (Y component)
            pixel_to_mm = self.pixel_spacing_lateral[1]
            # Update image_height_mm (should be similar to frontal)
            image_height_mm = h * pixel_to_mm
            
            self.lateral_plane = self.create_image_plane(
                self.lateral_image,
                position=[0, 0, 0],  # Center at origin, overlapping frontal
                rotation='lateral',
                scale=pixel_to_mm
            )
            if self.lateral_plane:
                self.view_3d.addItem(self.lateral_plane)
        
        # Update grid position to be at the base of the images
        self.update_grid_position(image_height_mm)
        
    def load_stl_files(self):
        """Load STL files from patient's CT directory"""
        if not self.current_patient:
            return
        
        # Clear existing STL meshes
        for mesh_item in self.stl_meshes.values():
            self.view_3d.removeItem(mesh_item)
        self.stl_meshes.clear()
        self.stl_data.clear()
        self.stl_list.clear()
        
        # Get CT directory path
        script_dir = os.path.dirname(os.path.abspath(__file__))
        base_dir = os.path.join(script_dir, '..', '..', '..', '..')
        ct_dir = os.path.abspath(os.path.join(base_dir, 'Patients', self.current_patient, 'CT'))
        
        if not os.path.exists(ct_dir):
            print(f"⚠️ CT directory not found: {ct_dir}")
            return
        
        # Load all STL files
        stl_files = [f for f in os.listdir(ct_dir) if f.lower().endswith('.stl')]
        
        if not stl_files:
            print(f"⚠️ No STL files found in: {ct_dir}")
            return
        
        print(f"✅ Found {len(stl_files)} STL files in {ct_dir}")
        
        for stl_file in sorted(stl_files):
            stl_path = os.path.join(ct_dir, stl_file)
            try:
                # Load STL mesh
                mesh_data = stl_mesh.Mesh.from_file(stl_path)
                
                # Convert to PyQtGraph format
                vertices = mesh_data.vectors.reshape(-1, 3)
                faces = np.arange(len(vertices)).reshape(-1, 3)
                
                # Create mesh data
                pg_mesh_data = gl.MeshData(vertexes=vertices, faces=faces)
                
                # Calculate centroid
                centroid = np.mean(vertices, axis=0)
                
                # Create mesh item
                mesh_item = gl.GLMeshItem(
                    meshdata=pg_mesh_data,
                    smooth=True,
                    drawEdges=True,
                    edgeColor=(0.5, 0.5, 0.5, 0.5),
                    color=(0.8, 0.8, 0.2, 0.8),
                    glOptions='translucent'
                )
                
                # Store mesh
                self.stl_meshes[stl_file] = mesh_item
                self.stl_data[stl_file] = {
                    'mesh_data': mesh_data,
                    'vertices': vertices,
                    'centroid': centroid,
                    'transform': {'x': 0, 'y': 0, 'z': 0, 'rx': 0, 'ry': 0, 'rz': 0}
                }
                
                # Add to view
                self.view_3d.addItem(mesh_item)
                
                # Add to list widget
                item = QListWidgetItem(stl_file)
                item.setCheckState(Qt.Checked)  # Visible by default
                self.stl_list.addItem(item)
                
                print(f"  ✅ Loaded: {stl_file}")
                
            except Exception as e:
                print(f"  ❌ Error loading {stl_file}: {e}")
    
    def on_stl_visibility_changed(self, item):
        """Handle visibility checkbox change"""
        stl_file = item.text()
        if stl_file in self.stl_meshes:
            mesh_item = self.stl_meshes[stl_file]
            if item.checkState() == Qt.Checked:
                if mesh_item not in self.view_3d.items:
                    self.view_3d.addItem(mesh_item)
            else:
                if mesh_item in self.view_3d.items:
                    self.view_3d.removeItem(mesh_item)
    
    def on_stl_selection_changed(self):
        """Handle STL layer selection change"""
        selected_items = self.stl_list.selectedItems()
        if selected_items:
            self.selected_stl = selected_items[0].text()
            # Update spin boxes with current transform
            data = self.stl_data[self.selected_stl]
            transform = data['transform']
            
            self.tx_spin.blockSignals(True)
            self.ty_spin.blockSignals(True)
            self.tz_spin.blockSignals(True)
            self.rx_spin.blockSignals(True)
            self.ry_spin.blockSignals(True)
            self.rz_spin.blockSignals(True)
            
            self.tx_spin.setValue(transform['x'])
            self.ty_spin.setValue(transform['y'])
            self.tz_spin.setValue(transform['z'])
            self.rx_spin.setValue(transform.get('rx', 0))
            self.ry_spin.setValue(transform.get('ry', 0))
            self.rz_spin.setValue(transform.get('rz', 0))
            
            self.tx_spin.blockSignals(False)
            self.ty_spin.blockSignals(False)
            self.tz_spin.blockSignals(False)
            self.rx_spin.blockSignals(False)
            self.ry_spin.blockSignals(False)
            self.rz_spin.blockSignals(False)
            
            # Update gizmo
            self.update_gizmo()
            
            # Update projections
            self.update_vertebra_projections()
        else:
            self.selected_stl = None
            self.remove_gizmo()
            self.clear_projections()
    
    def update_selected_stl_transform(self):
        """Update the transform of the selected STL mesh"""
        if not self.selected_stl or self.selected_stl not in self.stl_meshes:
            return
        
        # Get new transform values
        tx = self.tx_spin.value()
        ty = self.ty_spin.value()
        tz = self.tz_spin.value()
        rx = self.rx_spin.value()
        ry = self.ry_spin.value()
        rz = self.rz_spin.value()
        
        # Update stored transform
        self.stl_data[self.selected_stl]['transform'] = {
            'x': tx, 'y': ty, 'z': tz,
            'rx': rx, 'ry': ry, 'rz': rz
        }
        
        # Apply transform
        mesh_item = self.stl_meshes[self.selected_stl]
        data = self.stl_data[self.selected_stl]
        centroid = data.get('centroid', np.array([0, 0, 0]))
        cx, cy, cz = centroid
        
        mesh_item.resetTransform()
        # Translate to final position (relative to origin)
        # We want to rotate around centroid.
        # M = T(pos) * R * T(-centroid)
        # pos = centroid + translation
        
        mesh_item.translate(tx + cx, ty + cy, tz + cz)
        mesh_item.rotate(rx, 1, 0, 0)
        mesh_item.rotate(ry, 0, 1, 0)
        mesh_item.rotate(rz, 0, 0, 1)
        mesh_item.translate(-cx, -cy, -cz)
        
        # Update gizmo position
        self.update_gizmo()
        
        # Update projections
        self.update_vertebra_projections()
    
    def nudge_stl(self, axis, amount):
        """Nudge the selected STL mesh in the specified direction"""
        if not self.selected_stl:
            return
        
        if axis == 'x':
            self.tx_spin.setValue(self.tx_spin.value() + amount)
        elif axis == 'y':
            self.ty_spin.setValue(self.ty_spin.value() + amount)
        elif axis == 'z':
            self.tz_spin.setValue(self.tz_spin.value() + amount)
        elif axis == 'rx':
            self.rx_spin.setValue(self.rx_spin.value() + amount)
        elif axis == 'ry':
            self.ry_spin.setValue(self.ry_spin.value() + amount)
        elif axis == 'rz':
            self.rz_spin.setValue(self.rz_spin.value() + amount)
            
    # ==================== GIZMO & INTERACTION ====================
    
    def update_gizmo(self):
        """Create or update the transformation gizmo"""
        if not self.selected_stl:
            self.remove_gizmo()
            return
            
        # Get current position
        data = self.stl_data[self.selected_stl]
        transform = data['transform']
        centroid = data.get('centroid', np.array([0, 0, 0]))
        
        # Gizmo position = Transform + Centroid
        gx = transform['x'] + centroid[0]
        gy = transform['y'] + centroid[1]
        gz = transform['z'] + centroid[2]
        
        if self.gizmo is None:
            self.create_gizmo()
            
        # Move gizmo to object position
        for item in self.gizmo.values():
            item.resetTransform()
            item.translate(gx, gy, gz)
            
    def create_gizmo(self):
        """Create the 3D gizmo axes"""
        self.gizmo = {}
        
        # Z Axis (Blue) - Default Cylinder (along Z)
        md_z = gl.MeshData.cylinder(rows=10, cols=20, radius=[2, 2], length=100)
        z_axis = gl.GLMeshItem(meshdata=md_z, smooth=True, color=(0, 0, 1, 1), glOptions='opaque')
        self.view_3d.addItem(z_axis)
        self.gizmo['z'] = z_axis
        
        # X Axis (Red) - Rotate Z to X (90 deg around Y)
        md_x = gl.MeshData.cylinder(rows=10, cols=20, radius=[2, 2], length=100)
        verts_x = md_x.vertexes()
        # Rotate: x'=z, y'=y, z'=-x
        new_verts_x = np.zeros_like(verts_x)
        new_verts_x[:, 0] = verts_x[:, 2]
        new_verts_x[:, 1] = verts_x[:, 1]
        new_verts_x[:, 2] = -verts_x[:, 0]
        md_x.setVertexes(new_verts_x)
        
        x_axis = gl.GLMeshItem(meshdata=md_x, smooth=True, color=(1, 0, 0, 1), glOptions='opaque')
        self.view_3d.addItem(x_axis)
        self.gizmo['x'] = x_axis
        
        # Y Axis (Green) - Rotate Z to Y (-90 deg around X)
        md_y = gl.MeshData.cylinder(rows=10, cols=20, radius=[2, 2], length=100)
        verts_y = md_y.vertexes()
        # Rotate: x'=x, y'=z, z'=-y
        new_verts_y = np.zeros_like(verts_y)
        new_verts_y[:, 0] = verts_y[:, 0]
        new_verts_y[:, 1] = verts_y[:, 2]
        new_verts_y[:, 2] = -verts_y[:, 1]
        md_y.setVertexes(new_verts_y)
        
        y_axis = gl.GLMeshItem(meshdata=md_y, smooth=True, color=(0, 1, 0, 1), glOptions='opaque')
        self.view_3d.addItem(y_axis)
        self.gizmo['y'] = y_axis
        
    def remove_gizmo(self):
        """Remove gizmo from scene"""
        if self.gizmo:
            for item in self.gizmo.values():
                if item in self.view_3d.items:
                    self.view_3d.removeItem(item)
            self.gizmo = None
            
    def project_point_to_screen(self, point_3d):
        """Project a 3D point to screen coordinates"""
        # Get matrices from view widget
        view_matrix = self.view_3d.viewMatrix()
        proj_matrix = self.view_3d.projectionMatrix()
        
        # Convert to QMatrix4x4 if they are numpy arrays (PyQtGraph usually returns numpy arrays or QMatrix4x4)
        # PyQtGraph 0.13+ returns QMatrix4x4 usually
        
        # Transform point
        # Model -> View -> Projection -> Viewport
        
        # Point is in World Coordinates
        pt = QVector4D(point_3d[0], point_3d[1], point_3d[2], 1.0)
        
        # Apply View Matrix
        pt = view_matrix * pt
        
        # Apply Projection Matrix
        pt = proj_matrix * pt
        
        if pt.w() == 0:
            return None
            
        # Normalized Device Coordinates
        x = pt.x() / pt.w()
        y = pt.y() / pt.w()
        
        # Viewport transform
        width = self.view_3d.width()
        height = self.view_3d.height()
        
        screen_x = (x + 1.0) * width / 2.0
        screen_y = (1.0 - y) * height / 2.0 # Screen Y is down
        
        return np.array([screen_x, screen_y])

    def get_axis_screen_vector(self, axis_name):
        """Get the screen vector for a gizmo axis"""
        if not self.selected_stl:
            return None, None
            
        data = self.stl_data[self.selected_stl]
        transform = data['transform']
        centroid = data.get('centroid', np.array([0, 0, 0]))
        
        # Origin is Transform + Centroid
        origin = np.array([
            transform['x'] + centroid[0],
            transform['y'] + centroid[1],
            transform['z'] + centroid[2]
        ])
        
        # Axis end point (100mm length)
        if axis_name == 'x':
            end = origin + np.array([100, 0, 0])
        elif axis_name == 'y':
            end = origin + np.array([0, 100, 0])
        elif axis_name == 'z':
            end = origin + np.array([0, 0, 100])
            
        start_screen = self.project_point_to_screen(origin)
        end_screen = self.project_point_to_screen(end)
        
        if start_screen is None or end_screen is None:
            return None, None
            
        return start_screen, end_screen

    def handle_mouse_press(self, ev):
        """Handle mouse press for gizmo interaction"""
        if not self.gizmo or ev.button() != Qt.LeftButton:
            return False
            
        self.dragging_axis = None
        self.last_mouse_pos = ev.pos()
        mouse_pos = np.array([ev.pos().x(), ev.pos().y()])
        
        # Check distance to each axis
        min_dist = 20.0 # Pixel threshold
        closest_axis = None
        
        # Check Translation Axes
        for axis in ['x', 'y', 'z']:
            start, end = self.get_axis_screen_vector(axis)
            if start is None:
                continue
                
            # Distance from point to line segment
            line_vec = end - start
            line_len_sq = np.dot(line_vec, line_vec)
            
            if line_len_sq == 0:
                continue
                
            t = np.dot(mouse_pos - start, line_vec) / line_len_sq
            t = max(0, min(1, t))
            
            projection = start + t * line_vec
            dist = np.linalg.norm(mouse_pos - projection)
            
            if dist < min_dist:
                min_dist = dist
                closest_axis = axis

        if closest_axis:
            self.dragging_axis = closest_axis
            print(f"🎯 Grabbed {closest_axis.upper()} axis")
            return True
            
        return False

    def handle_mouse_move(self, ev):
        """Handle mouse move for dragging"""
        if not self.dragging_axis or not self.selected_stl:
            return False
            
        mouse_pos = np.array([ev.pos().x(), ev.pos().y()])
        
        if self.dragging_axis in ['x', 'y', 'z']:
            # Translation
            start, end = self.get_axis_screen_vector(self.dragging_axis)
            if start is None:
                return False
                
            axis_screen_vec = end - start
            axis_len_sq = np.dot(axis_screen_vec, axis_screen_vec)
            
            if axis_len_sq < 1e-6:
                return False
                
            last_pos = np.array([self.last_mouse_pos.x(), self.last_mouse_pos.y()])
            delta = mouse_pos - last_pos
            
            axis_dir = axis_screen_vec / np.sqrt(axis_len_sq)
            screen_move = np.dot(delta, axis_dir)
            
            scale = 100.0 / np.sqrt(axis_len_sq)
            move_mm = screen_move * scale
            
            self.last_mouse_pos = ev.pos()
            self.nudge_stl(self.dragging_axis, move_mm)
             
        return True

    def handle_mouse_release(self, ev):
        """Handle mouse release"""
        if self.dragging_axis:
            self.dragging_axis = None
            return True
        return False

    # ==================== PROJECTION VISUALIZATION ====================
    
    def update_vertebra_projections(self):
        """Project the selected vertebra onto the image planes"""
        if not self.selected_stl or not self.stl_data.get(self.selected_stl):
            self.clear_projections()
            return
            
        # Get mesh vertices (in local coords)
        vertices = self.stl_data[self.selected_stl]['vertices']
        transform = self.stl_data[self.selected_stl]['transform']
        
        # Apply transform to get Viewer Coordinates
        # We need to add translation
        # Vertices is (N, 3)
        t_vec = np.array([transform['x'], transform['y'], transform['z']])
        viewer_vertices = vertices + t_vec
        
        # Downsample for performance (projecting every vertex is slow)
        # Use every 50th vertex
        sample_rate = 50
        sampled_verts = viewer_vertices[::sample_rate]
        
        # Project to EOS 2D coordinates
        # Map Viewer Coords to EOS Coords
        # Viewer Y = EOS X (Left-Right)
        # Viewer Z = EOS Y (Up-Down)
        # Viewer X = EOS Z (Front-Back)
        
        eos_x = sampled_verts[:, 1] # Viewer Y
        eos_y = sampled_verts[:, 2] # Viewer Z
        eos_z = sampled_verts[:, 0] # Viewer X
        
        # Calculate projections
        # project_3d_to_2d is vectorized? No, it expects scalars.
        # Let's vectorize it or loop
        
        # Vectorized projection
        dsti_f = self.distance_source_to_isocenter_frontal
        dsti_l = self.distance_source_to_isocenter_lateral
        
        # Frontal X projection
        # x_proj = (x / (dsti + z)) * dsti
        denom_f = dsti_f + eos_z
        # Avoid division by zero
        denom_f[np.abs(denom_f) < 1e-6] = 1e-6
        proj_x_frontal = (eos_x / denom_f) * dsti_f
        
        # Lateral Z projection
        # z_proj = (z / (dsti + x)) * dsti
        denom_l = dsti_l + eos_x
        denom_l[np.abs(denom_l) < 1e-6] = 1e-6
        proj_z_lateral = (eos_z / denom_l) * dsti_l
        
        # Y is parallel projection (EOS slot scanner)
        proj_y = eos_y
        
        # Now map back to Viewer Coords for display on planes
        
        # Frontal Plane (Viewer X=0)
        # Display at: X=0, Y=proj_x_frontal, Z=proj_y
        frontal_pts = np.column_stack((
            np.zeros_like(proj_x_frontal),
            proj_x_frontal,
            proj_y
        ))
        
        # Lateral Plane (Viewer Y=0)
        # Display at: X=proj_z_lateral, Y=0, Z=proj_y
        lateral_pts = np.column_stack((
            proj_z_lateral,
            np.zeros_like(proj_z_lateral),
            proj_y
        ))
        
        # Update Scatter Plots
        self.update_projection_item('frontal', frontal_pts, (1, 1, 0, 0.6)) # Yellow
        self.update_projection_item('lateral', lateral_pts, (1, 1, 0, 0.6)) # Yellow
        
    def update_projection_item(self, plane, points, color):
        """Update the scatter plot for a projection"""
        if self.projection_items[plane] is None:
            self.projection_items[plane] = gl.GLScatterPlotItem(
                pos=points, color=color, size=2, pxMode=True
            )
            self.view_3d.addItem(self.projection_items[plane])
        else:
            self.projection_items[plane].setData(pos=points, color=color)
            
    def clear_projections(self):
        """Remove projection items"""
        for plane in ['frontal', 'lateral']:
            if self.projection_items[plane]:
                if self.projection_items[plane] in self.view_3d.items:
                    self.view_3d.removeItem(self.projection_items[plane])
                self.projection_items[plane] = None

    def on_stl_selection_changed(self):
        """Handle STL layer selection change"""
        selected_items = self.stl_list.selectedItems()
        if selected_items:
            self.selected_stl = selected_items[0].text()
            # Update spin boxes with current transform
            transform = self.stl_data[self.selected_stl]['transform']
            self.tx_spin.blockSignals(True)
            self.ty_spin.blockSignals(True)
            self.tz_spin.blockSignals(True)
            self.tx_spin.setValue(transform['x'])
            self.ty_spin.setValue(transform['y'])
            self.tz_spin.setValue(transform['z'])
            self.tx_spin.blockSignals(False)
            self.ty_spin.blockSignals(False)
            self.tz_spin.blockSignals(False)
            
            # Update gizmo
            self.update_gizmo()
            
            # Update projections
            self.update_vertebra_projections()
        else:
            self.selected_stl = None
            self.remove_gizmo()
            self.clear_projections()
    
    def reset_selected_stl_transform(self):
        """Reset the transform of the selected STL mesh"""
        if not self.selected_stl:
            return
        
        self.tx_spin.setValue(0)
        self.ty_spin.setValue(0)
        self.tz_spin.setValue(0)
    
    def show_all_stl(self):
        """Show all STL meshes"""
        for i in range(self.stl_list.count()):
            item = self.stl_list.item(i)
            item.setCheckState(Qt.Checked)
            stl_file = item.text()
            if stl_file in self.stl_meshes:
                mesh_item = self.stl_meshes[stl_file]
                if mesh_item not in self.view_3d.items:
                    self.view_3d.addItem(mesh_item)
    
    def hide_all_stl(self):
        """Hide all STL meshes"""
        for i in range(self.stl_list.count()):
            item = self.stl_list.item(i)
            item.setCheckState(Qt.Unchecked)
            stl_file = item.text()
            if stl_file in self.stl_meshes:
                mesh_item = self.stl_meshes[stl_file]
                if mesh_item in self.view_3d.items:
                    self.view_3d.removeItem(mesh_item)
        
    def on_separation_changed(self, value):
        """Handle image separation slider change (DISABLED - images always touch)"""
        pass
        
    def reset_view(self):
        """Reset the 3D view to default"""
        self.view_3d.setCameraPosition(distance=2500, elevation=20, azimuth=45)
    
    def add_3d_marker(self, x, y, z, color=(1, 0, 0, 1), size=20, label=None):
        """
        Add a 3D marker sphere at the specified coordinates
        
        Args:
            x, y, z: 3D coordinates in mm
            color: RGBA tuple (0-1 range)
            size: Marker size in mm
            label: Optional text label for the marker
        """
        # Create sphere mesh data
        md = gl.MeshData.sphere(rows=10, cols=10, radius=size)
        
        # Create mesh item
        marker = gl.GLMeshItem(
            meshdata=md,
            smooth=True,
            color=color,
            glOptions='translucent'
        )
        
        # Position the marker
        marker.translate(x, y, z)
        
        # Add to view and storage
        self.view_3d.addItem(marker)
        self.marker_items.append(marker)
        
        # Print info
        if label:
            print(f"✅ Added marker '{label}' at ({x:.2f}, {y:.2f}, {z:.2f}) mm")
        else:
            print(f"✅ Added marker at ({x:.2f}, {y:.2f}, {z:.2f}) mm")
        
        return marker
    
    def clear_3d_markers(self):
        """Clear all 3D markers from the view"""
        for marker in self.marker_items:
            self.view_3d.removeItem(marker)
        self.marker_items.clear()
        print("🧹 Cleared all 3D markers")
    
    def load_landmarks(self):
        """Load landmarks from _edited.txt or original .txt file and visualize them in 3D"""
        # Clear existing landmarks
        for item in self.landmark_items:
            if item in self.view_3d.items:
                self.view_3d.removeItem(item)
        self.landmark_items = []
        
        if not self.current_patient:
            return

        eos_path = self.patient_combo.currentData()
        if not eos_path:
            return

        # Find landmark file
        landmark_file = None
        
        # Priority 1: _edited.txt
        edited_files = [f for f in os.listdir(eos_path) if f.endswith('_edited.txt')]
        if edited_files:
            landmark_file = os.path.join(eos_path, edited_files[0])
            print(f"Found edited landmark file: {landmark_file}")
        else:
            # Priority 2: Any other .txt file
            txt_files = [f for f in os.listdir(eos_path) if f.endswith('.txt')]
            if txt_files:
                # Simple heuristic: take the first one.
                landmark_file = os.path.join(eos_path, txt_files[0])
                print(f"Found original landmark file: {landmark_file}")
        
        if not landmark_file:
            print(f"No landmark file found in {eos_path}")
            return
        
        try:
            with open(landmark_file, 'r') as f:
                lines = f.readlines()
            
            print(f"Parsing landmark file: {landmark_file}")
            
            for line in lines:
                line = line.strip()
                if not line:
                    continue
                
                # Handle header line which might be malformed and contain data
                # e.g. "X: ... (shared)T1,Plat_Sup_G,..."
                if "X:" in line:
                    if "(shared)" in line:
                        parts = line.split("(shared)")
                        if len(parts) > 1:
                            line = parts[1] # Use the part after (shared)
                        else:
                            continue # Just header
                    else:
                        continue # Just header
                
                parts = line.strip().split(',')
                if len(parts) < 5:
                    continue
                
                # Format: Vertebra, PointName, px (lat X), py (front X), pz (shared Y)
                vertebra = parts[0]
                point_name = parts[1]
                try:
                    # Coordinates from file are in Unprocessed Pixel Space
                    # (Scaled from 512x1024 to Original Size by postprocessing.py)
                    px_lat = float(parts[2])  # Lateral X
                    px_front = float(parts[3]) # Frontal X
                    px_y = float(parts[4])    # Vertical (Y) - Shared
                    
                    # Reconstruct 3D position
                    coords_3d = self.reconstruct_3d_from_pixels(px_front, px_y, px_lat, px_y)
                    
                    if coords_3d:
                        x, y, z = coords_3d
                        
                        # Create 3D point (sphere)
                        md = gl.MeshData.sphere(rows=8, cols=8, radius=2.0)
                        colors = np.ones((md.vertexes().shape[0], 4))
                        colors[:, 0] = 1.0 # R
                        colors[:, 1] = 1.0 # G
                        colors[:, 2] = 0.0 # B (Yellow)
                        colors[:, 3] = 1.0 # Alpha
                        md.setVertexColors(colors)
                        
                        mesh_item = gl.GLMeshItem(meshdata=md, smooth=True, shader='shaded', glOptions='opaque')
                        
                        # Apply transformation:
                        # 1. Map Vertical (y) to Viewer Z (y -> z_final)
                        # 2. Rotate 180 deg relative to previous (which was 90 deg CCW)
                        #    Previous: (-z, x, y)
                        #    New (Rotated 180 around Z): (z, -x, y)
                        mesh_item.translate(z, -x, y)
                        
                        self.view_3d.addItem(mesh_item)
                        self.landmark_items.append(mesh_item)
                        
                except ValueError:
                    continue
            
            print(f"Loaded {len(self.landmark_items)} landmarks")
            # Apply visibility based on toggle button
            self.toggle_landmarks()
            
        except Exception as e:
            print(f"Error loading landmarks: {e}")

    def toggle_landmarks(self):
        """Toggle visibility of landmark items"""
        visible = self.toggle_landmarks_btn.isChecked()
        for item in self.landmark_items:
            item.setVisible(visible)
        

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
