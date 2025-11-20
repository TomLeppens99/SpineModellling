# EOS Projection Methods - Documentation

## Overview
This module implements the projection mathematics for converting between 2D EOS X-ray images and 3D coordinate space, ported from the C# codebase.

## Key Concepts

### EOS Imaging Geometry
- **Frontal (AP) Image**: Captures front view (YZ plane projection)
- **Lateral (Sagittal) Image**: Captures side view (XZ plane projection)
- **Dual X-ray System**: Two perpendicular X-ray sources enable 3D reconstruction via triangulation

### Coordinate System
- **X-axis**: Left-Right (Red line in viewer)
- **Y-axis**: Up-Down / Superior-Inferior (Green line in viewer)
- **Z-axis**: Front-Back / Anterior-Posterior (Blue line in viewer)
- **Origin (0,0,0)**: Isocenter of EOS system

## Core Methods

### 1. **2D → 3D Reconstruction (Inverse Projection)**

```python
def inverse_project_2d_to_3d(self, x_projected, z_projected):
    """
    Ray triangulation from dual X-ray projections
    
    Args:
        x_projected: Position on frontal image (mm from center)
        z_projected: Position on lateral image (mm from center)
    
    Returns:
        (x_real, z_real): 3D coordinates in EOS space (mm)
    """
```

**Mathematical Principle:**
- Cast rays from each X-ray source through the 2D projection points
- Find intersection of these rays in 3D space (triangulation)
- Uses similar triangles and ray-line intersection

**Algorithm** (from C# UC_measurementsMain.cs lines 513-546):
```
slope_lateral = (0 - (-DSTI_frontal)) / (x_proj - 0)
slope_frontal = ((-z_proj - 0) / (0 - DSTI_lateral))

x_real = ((-slope_frontal * DSTI_lateral) - (-DSTI_frontal)) / (slope_lateral - slope_frontal)
z_real = slope_lateral * x_real + (-DSTI_frontal)
```

Where `DSTI` = Distance Source To Isocenter (typically 900mm)

### 2. **3D → 2D Projection (Forward Projection)**

```python
def project_3d_to_2d(self, x_real, z_real):
    """
    Perspective projection for visualization
    
    Args:
        x_real: X coordinate in 3D EOS space (mm)
        z_real: Z coordinate in 3D EOS space (mm)
    
    Returns:
        (x_projected, z_projected): Projected positions on images (mm)
    """
```

**Mathematical Principle:**
- Perspective projection using pinhole camera model
- Projects 3D point onto 2D image plane through X-ray source

**Algorithm** (from C# EosSpace.cs lines 323-327):
```
x_proj = (x_real / (DSTI_frontal + z_real)) * DSTI_frontal
z_proj = (z_real / (DSTI_lateral + x_real)) * DSTI_lateral
```

### 3. **Complete Pipeline: Pixel → 3D**

```python
def image_pixel_to_3d(self, frontal_px_x, frontal_px_y, lateral_px_x, lateral_px_y):
    """
    Convert image pixel coordinates to 3D space
    
    Returns:
        (x, y, z): 3D coordinates in EOS space (mm)
    """
```

**Pipeline:**
1. Convert pixels to mm using pixel spacing
2. Adjust for image center and coordinate system orientation
3. Apply inverse projection (triangulation)
4. Average Y coordinates from both images

### 4. **Complete Pipeline: 3D → Pixel**

```python
def coords_3d_to_image_pixel(self, x_3d, y_3d, z_3d):
    """
    Convert 3D coordinates to image pixel positions
    
    Returns:
        dict: {'frontal': (px_x, px_y), 'lateral': (px_x, px_y)}
    """
```

**Pipeline:**
1. Apply forward projection to get mm coordinates
2. Convert mm to pixels using pixel spacing
3. Adjust for image center and coordinate system

## Usage Examples

### Example 1: Reconstruct 3D point from image clicks

```python
# User clicks on both images at specific pixels
frontal_click = (512, 1024)  # (x, y) in pixels
lateral_click = (498, 1024)  # (x, y) in pixels

# Reconstruct 3D position
x_3d, y_3d, z_3d = viewer.image_pixel_to_3d(
    frontal_click[0], frontal_click[1],
    lateral_click[0], lateral_click[1]
)

print(f"3D Position: ({x_3d:.2f}, {y_3d:.2f}, {z_3d:.2f}) mm")

# Add marker in 3D view
viewer.add_3d_marker(x_3d, y_3d, z_3d, color=(1, 0, 0, 1), label="Landmark")
```

### Example 2: Project 3D marker back to images

```python
# You have a 3D point (e.g., from STL vertebra model)
vertebra_center = (10.5, 850.0, -20.3)  # (x, y, z) in mm

# Project to image coordinates
projections = viewer.coords_3d_to_image_pixel(*vertebra_center)

frontal_px = projections['frontal']
lateral_px = projections['lateral']

print(f"Shows at Frontal: {frontal_px}, Lateral: {lateral_px}")

# Draw crosshair on images at these positions
```

### Example 3: Test projection accuracy

```python
# Click "Test Projection (Center Point)" button in UI
# OR programmatically:
viewer.test_projection()

# This will:
# 1. Take center pixels of both images
# 2. Reconstruct 3D position
# 3. Project back to 2D
# 4. Calculate reprojection error
# 5. Add visual markers in 3D view
```

## Important Parameters (Automatically Extracted)

### Distance Source To Isocenter (DSTI)
- **Frontal**: `self.distance_source_to_isocenter_frontal`
- **Lateral**: `self.distance_source_to_isocenter_lateral`
- Extracted from DICOM tag `(0018, 1110)`
- Defaults to 900mm if not found

### Pixel Spacing
- **Frontal**: `self.pixel_spacing_frontal`
- **Lateral**: `self.pixel_spacing_lateral`
- Extracted from DICOM tag `(0028, 0030)`
- Defaults to 0.17mm/pixel if not found

### Coordinate System Conversions
- **Y-axis**: Measured from bottom in EOS, but pixels count from top
  - Conversion: `mm_y = (image_height - pixel_y) * pixel_spacing`
- **X-axis**: Measured from center, inverted for perspective
  - Conversion: `mm_x = (image_center - pixel_x) * pixel_spacing`

## Testing & Validation

### Built-in Test Function
The viewer includes `test_projection()` which:
- ✅ Tests center point reconstruction
- ✅ Calculates reprojection error
- ✅ Tests multiple depth points
- ✅ Adds visual 3D markers
- ✅ Prints detailed analysis to console

**Expected Results:**
- Reprojection error < 1 pixel = Excellent
- Reprojection error < 5 pixels = Good
- Reprojection error > 5 pixels = Check calibration parameters

### Manual Validation
1. Select a visible anatomical landmark in both images
2. Note pixel coordinates
3. Reconstruct 3D position
4. Project back to 2D
5. Compare with original pixel coordinates

## Integration with C# Codebase

### Source References
- **Inverse Projection**: `UC_measurementsMain.cs` lines 513-546
- **Forward Projection**: `EosSpace.cs` lines 323-327
- **Pixel Conversions**: `UC_measurementsMain.cs` lines 355-362
- **Complete Example**: `UC_measurementsMain.cs` method `computeCenterOf2Measurements()`

### Algorithm Verification
The Python implementation uses identical mathematics to the C# version:
- Same projection formulas
- Same coordinate system conventions
- Same handling of edge cases (division by zero)

## Future Enhancements

### Recommended Additions
1. **DICOM Parameter Extraction**: Auto-load DSTI and pixel spacing from DICOM headers
2. **Interactive Landmark Placement**: Click on images to create 3D markers
3. **Calibration Tools**: Fine-tune parameters using known reference points
4. **Distance Measurements**: Calculate 3D distances between landmarks
5. **Angle Measurements**: Calculate 3D angles (e.g., Cobb angle)
6. **Export to TRC**: Save landmark coordinates for biomechanical analysis

### Click-to-3D Feature (Proposed)
```python
def on_image_click(self, image_type, pixel_x, pixel_y):
    """Store clicked point and reconstruct when both images clicked"""
    if image_type == 'frontal':
        self.pending_frontal_click = (pixel_x, pixel_y)
    elif image_type == 'lateral':
        self.pending_lateral_click = (pixel_x, pixel_y)
    
    # If both clicks available, reconstruct
    if self.pending_frontal_click and self.pending_lateral_click:
        x, y, z = self.image_pixel_to_3d(...)
        self.add_3d_marker(x, y, z, label="User Landmark")
        self.pending_frontal_click = None
        self.pending_lateral_click = None
```

## Troubleshooting

### Common Issues

**Issue**: High reprojection error
- **Solution**: Verify DSTI parameters match actual EOS system
- **Solution**: Check pixel spacing from DICOM metadata
- **Solution**: Ensure images are properly aligned

**Issue**: NaN (Not a Number) results
- **Solution**: Check for division by zero (x_projected = 0)
- **Solution**: Verify input coordinates are within valid range

**Issue**: 3D position seems inverted
- **Solution**: Check coordinate system sign conventions
- **Solution**: Verify image orientation (front/back, left/right)

### Debug Output
Enable detailed logging:
```python
# In projection methods, add:
print(f"Input: x_proj={x_projected:.2f}, z_proj={z_projected:.2f}")
print(f"Slopes: lateral={slope_lateral:.6f}, frontal={slope_frontal:.6f}")
print(f"Output: x_real={x_real:.2f}, z_real={z_real:.2f}")
```

## References

- **Original C# Implementation**: SpineModeling/EosSpace.cs
- **Measurement Tools**: SpineModeling/SkeletalModeling/UC_measurementsMain.cs
- **EOS Imaging**: https://www.eos-imaging.com/
- **Stereo Triangulation**: Computer Vision textbooks (Hartley & Zisserman, "Multiple View Geometry")

---

**Version**: 1.0  
**Date**: November 2025  
**Author**: Integrated from C# SpineModeling codebase  
**Status**: ✅ Fully Functional
