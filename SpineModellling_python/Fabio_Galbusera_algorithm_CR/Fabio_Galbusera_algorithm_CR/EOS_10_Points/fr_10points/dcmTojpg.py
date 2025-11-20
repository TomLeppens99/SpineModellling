import dicom2jpg

if __name__ == '__main__':
    dicom_dir = "D:\GOS2_PRE\XXX\_2D_secondary_03_01_2022_14_47_45"
    export_location = "D:\GOS2_PRE\XXX\_2D_secondary_03_01_2022_14_47_45"
    dicom2jpg.dicom2jpg(dicom_dir,target_root=export_location) 