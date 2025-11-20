import sys
import os
from PyQt5.QtWidgets import QApplication, QMainWindow, QTabWidget, QVBoxLayout, QWidget
from PyQt5.QtGui import QIcon, QFont

# Add subdirectories to sys.path to allow imports
current_dir = os.path.dirname(os.path.abspath(__file__))
sys.path.append(os.path.join(current_dir, 'EOS_10_Points', 'EOS_2022_04_07'))
sys.path.append(os.path.join(current_dir, 'EOS_10_Points', 'fr_10points'))
sys.path.append(os.path.join(current_dir, 'EOS_10_Points', 'registration_3d'))

# Import the applications
# Note: We need to be careful about imports that might conflict or rely on current working directory
# The modifications made to the files should handle config paths correctly.

try:
    import app as detection_app
except ImportError as e:
    print(f"Error importing detection app: {e}")
    detection_app = None

try:
    import EOS_10points as editor_app
except ImportError as e:
    print(f"Error importing editor app: {e}")
    editor_app = None

# Import 3D Registration (PyQtGraph version)
try:
    import registration_viewer_pyqtgraph as registration_app
    print("✅ 3D Registration module (PyQtGraph) imported successfully")
except ImportError as e:
    print(f"⚠️ Error importing 3D registration app: {e}")
    registration_app = None

class UnifiedWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("Spine Modeling Unified Tool")
        self.setGeometry(100, 100, 1200, 800)
        
        # Set a modern stylesheet for the main window
        self.setStyleSheet("""
            QMainWindow {
                background-color: #2b2b2b;
            }
            QTabWidget::pane {
                border: 1px solid #444;
                background: #2b2b2b;
                top: -1px;
            }
            QTabBar::tab {
                background: #3a3a3a;
                color: #c0c0c0;
                padding: 10px 20px;
                border: 1px solid #444;
                border-bottom-color: #444;
                border-top-left-radius: 4px;
                border-top-right-radius: 4px;
                min-width: 150px;
                font-weight: bold;
            }
            QTabBar::tab:selected {
                background: #4a90e2;
                color: white;
                border-color: #4a90e2;
            }
            QTabBar::tab:hover:!selected {
                background: #444;
            }
        """)

        self.central_widget = QWidget()
        self.setCentralWidget(self.central_widget)
        
        self.layout = QVBoxLayout(self.central_widget)
        self.layout.setContentsMargins(0, 0, 0, 0)
        
        self.tabs = QTabWidget()
        self.layout.addWidget(self.tabs)
        
        self.init_tabs()
        
    def init_tabs(self):
        # Tab 1: Landmark Detection
        if detection_app:
            try:
                # We need to change directory for the detection app if it relies on relative paths for models
                # However, app.py seems to use resource_path or relative paths. 
                # Let's try to instantiate it.
                
                # Temporarily change CWD to detection app folder for model loading if needed
                # But app.py uses resource_path which uses __file__, so it should be fine.
                # However, it imports 'network_4_stacks' etc. which are in sys.path now.
                
                self.detection_widget = detection_app.Main(parent=self)
                self.tabs.addTab(self.detection_widget, "Landmark Detection")
            except Exception as e:
                print(f"Error initializing detection tab: {e}")
                import traceback
                traceback.print_exc()
        
        # Tab 2: Landmark Editor
        if editor_app:
            try:
                self.editor_widget = editor_app.MainWindow(parent=self)
                self.tabs.addTab(self.editor_widget, "Landmark Editor")
            except Exception as e:
                print(f"Error initializing editor tab: {e}")
                import traceback
                traceback.print_exc()
        
        # Tab 3: 3D Registration
        if registration_app:
            try:
                self.registration_widget = registration_app.Registration3DViewer(parent=self)
                self.tabs.addTab(self.registration_widget, "3D Registration")
            except Exception as e:
                print(f"Error initializing 3D registration tab: {e}")
                import traceback
                traceback.print_exc()

def main():
    try:
        print("Starting application...")
        app = QApplication(sys.argv)
        app.setStyle('Fusion')
        
        print("Creating main window...")
        window = UnifiedWindow()
        
        print("Showing window...")
        window.showMaximized()
        window.show()  # Explicitly call show() as well
        
        print("Window should be visible now!")
        print("Entering event loop...")
        sys.exit(app.exec_())
    except Exception as e:
        print(f"FATAL ERROR: {e}")
        import traceback
        traceback.print_exc()
        input("Press Enter to exit...")

if __name__ == '__main__':
    main()
