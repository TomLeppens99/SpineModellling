import os
from PIL import Image

def read_first_lines(filepath, n=5):
    print(f"--- Reading first {n} lines of {os.path.basename(filepath)} ---")
    try:
        with open(filepath, 'r') as f:
            lines = [next(f) for _ in range(n)]
        for i, line in enumerate(lines):
            print(f"Line {i+1}: {line.strip()}")
        return lines
    except Exception as e:
        print(f"Error reading {filepath}: {e}")
        return []

def check_image_dims(filepath):
    print(f"--- Checking dimensions of {os.path.basename(filepath)} ---")
    try:
        with Image.open(filepath) as img:
            print(f"Dimensions: {img.size} (Width x Height)")
            return img.size
    except Exception as e:
        print(f"Error opening {filepath}: {e}")
        return None

base_path = r"c:\GBW_MyPrograms\SpineModellling\SpineModellling_python\Patients"

# Text files
txt_501 = os.path.join(base_path, "ASD501", "EOS", "ASD_501_edited.txt")
txt_502 = os.path.join(base_path, "ASD502", "EOS", "ASD_502_edited.txt")

lines_501 = read_first_lines(txt_501)
print("\n")
lines_502 = read_first_lines(txt_502)
print("\n")

# Compare lines
print("--- Comparison ---")
min_len = min(len(lines_501), len(lines_502))
for i in range(min_len):
    if lines_501[i] == lines_502[i]:
        print(f"Line {i+1} matches.")
    else:
        print(f"Line {i+1} differs.")
        print(f"  501: {lines_501[i].strip()}")
        print(f"  502: {lines_502[i].strip()}")

print("\n")

# Images
img_paths = [
    os.path.join(base_path, "ASD501", "EOS", "ASD501_C.jpg"),
    os.path.join(base_path, "ASD501", "EOS", "ASD501_S.jpg"),
    os.path.join(base_path, "ASD502", "EOS", "ASD502_C.jpg"),
    os.path.join(base_path, "ASD502", "EOS", "ASD502_S.jpg")
]

for img_path in img_paths:
    check_image_dims(img_path)
