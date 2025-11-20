"""
DICOM to JPG converter utility for EOS images
Automatically converts frontal.dcm and lateral.dcm to the required naming format
"""
import os
import numpy as np
try:
    import pydicom
    from PIL import Image
    DICOM_AVAILABLE = True
except ImportError:
    DICOM_AVAILABLE = False
    print("⚠️ pydicom or PIL not available. DICOM conversion will be disabled.")
    print("   Install with: pip install pydicom pillow")


def convert_dicom_to_jpg(dicom_path, output_path, window_center=None, window_width=None):
    """
    Convert a single DICOM file to JPG format
    
    Args:
        dicom_path: Path to the input DICOM file
        output_path: Path for the output JPG file
        window_center: Optional windowing center value
        window_width: Optional windowing width value
    
    Returns:
        True if successful, False otherwise
    """
    if not DICOM_AVAILABLE:
        print(f"❌ Cannot convert {dicom_path}: pydicom not available")
        return False
    
    try:
        # Read DICOM file
        ds = pydicom.dcmread(dicom_path)
        
        # Get pixel array
        pixel_array = ds.pixel_array
        
        # Apply rescale slope and intercept if available
        if hasattr(ds, 'RescaleSlope') and hasattr(ds, 'RescaleIntercept'):
            pixel_array = pixel_array * ds.RescaleSlope + ds.RescaleIntercept
        
        # Apply windowing if specified
        if window_center is not None and window_width is not None:
            img_min = window_center - window_width // 2
            img_max = window_center + window_width // 2
            pixel_array = np.clip(pixel_array, img_min, img_max)
        
        # Normalize to 0-255 range
        pixel_array = pixel_array.astype(float)
        pixel_array = (pixel_array - pixel_array.min()) / (pixel_array.max() - pixel_array.min()) * 255.0
        pixel_array = pixel_array.astype(np.uint8)
        
        # Handle photometric interpretation
        if hasattr(ds, 'PhotometricInterpretation'):
            if ds.PhotometricInterpretation == "MONOCHROME1":
                # Invert grayscale (dark is high value)
                pixel_array = 255 - pixel_array
        
        # Convert to PIL Image and save as JPG
        image = Image.fromarray(pixel_array)
        image.save(output_path, 'JPEG', quality=95)
        
        print(f"✅ Converted: {os.path.basename(dicom_path)} -> {os.path.basename(output_path)}")
        return True
        
    except Exception as e:
        print(f"❌ Error converting {dicom_path}: {str(e)}")
        return False


def convert_patient_dicoms(patient_eos_dir, patient_id=None):
    """
    Convert frontal.dcm and lateral.dcm to the required JPG format
    
    Args:
        patient_eos_dir: Path to the patient's EOS directory
        patient_id: Patient ID (if None, will extract from directory path)
    
    Returns:
        Tuple of (frontal_jpg_path, lateral_jpg_path) or (None, None) if failed
    """
    if not DICOM_AVAILABLE:
        return None, None
    
    # Extract patient ID from directory if not provided
    if patient_id is None:
        # Path structure: .../Patients/{PatientID}/EOS
        parent_dir = os.path.dirname(patient_eos_dir)
        patient_id = os.path.basename(parent_dir)
    
    # Define file paths
    frontal_dcm = os.path.join(patient_eos_dir, "frontal.dcm")
    lateral_dcm = os.path.join(patient_eos_dir, "lateral.dcm")
    
    frontal_jpg = os.path.join(patient_eos_dir, f"{patient_id}_C.jpg")
    lateral_jpg = os.path.join(patient_eos_dir, f"{patient_id}_S.jpg")
    
    converted_any = False
    
    # Convert frontal (anteroposterior) image
    if os.path.exists(frontal_dcm):
        if not os.path.exists(frontal_jpg):
            print(f"📄 Converting frontal DICOM for patient {patient_id}...")
            if convert_dicom_to_jpg(frontal_dcm, frontal_jpg):
                converted_any = True
        else:
            print(f"✓ Frontal JPG already exists: {os.path.basename(frontal_jpg)}")
    
    # Convert lateral (sagittal) image
    if os.path.exists(lateral_dcm):
        if not os.path.exists(lateral_jpg):
            print(f"📄 Converting lateral DICOM for patient {patient_id}...")
            if convert_dicom_to_jpg(lateral_dcm, lateral_jpg):
                converted_any = True
        else:
            print(f"✓ Lateral JPG already exists: {os.path.basename(lateral_jpg)}")
    
    if converted_any:
        print(f"✅ DICOM conversion completed for patient {patient_id}")
    
    # Return paths if they exist
    frontal_path = frontal_jpg if os.path.exists(frontal_jpg) else None
    lateral_path = lateral_jpg if os.path.exists(lateral_jpg) else None
    
    return frontal_path, lateral_path


def check_and_convert_dicoms(patients_dir):
    """
    Check all patient directories and convert DICOM files if needed
    
    Args:
        patients_dir: Path to the Patients directory
    
    Returns:
        Number of patients processed
    """
    if not os.path.exists(patients_dir):
        print(f"⚠️ Patients directory not found: {patients_dir}")
        return 0
    
    processed_count = 0
    
    # Iterate through patient directories
    for patient_id in os.listdir(patients_dir):
        patient_path = os.path.join(patients_dir, patient_id)
        
        if not os.path.isdir(patient_path):
            continue
        
        eos_dir = os.path.join(patient_path, "EOS")
        
        if not os.path.exists(eos_dir):
            continue
        
        # Check if DICOM files exist
        frontal_dcm = os.path.join(eos_dir, "frontal.dcm")
        lateral_dcm = os.path.join(eos_dir, "lateral.dcm")
        
        if os.path.exists(frontal_dcm) or os.path.exists(lateral_dcm):
            print(f"\n📂 Processing patient: {patient_id}")
            convert_patient_dicoms(eos_dir, patient_id)
            processed_count += 1
    
    return processed_count


if __name__ == "__main__":
    # Test conversion
    import sys
    
    if len(sys.argv) > 1:
        eos_dir = sys.argv[1]
        convert_patient_dicoms(eos_dir)
    else:
        # Default: scan all patients
        script_dir = os.path.dirname(os.path.abspath(__file__))
        base_dir = os.path.join(script_dir, '..', '..', '..', '..')
        patients_dir = os.path.abspath(os.path.join(base_dir, 'Patients'))
        
        print("🔍 Scanning for DICOM files in:", patients_dir)
        count = check_and_convert_dicoms(patients_dir)
        print(f"\n✅ Processed {count} patient(s)")
