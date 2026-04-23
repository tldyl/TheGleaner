from PIL import Image
import numpy as np
from scipy.ndimage import binary_dilation
import os

TASKS = [
    r"TheGleaner\images\ancients\frost_nova_director\run_icon.png",
    r"TheGleaner\images\ancients\frost_nova_director\map_icon.png"
]

EXPAND = 4
ALPHA_THRESHOLD = 10


def generate_solid_outline(input_path: str) -> None:
    print(f"Processing: {input_path}")

    img = Image.open(input_path).convert("RGBA")
    arr = np.array(img)

    alpha = arr[:, :, 3] > ALPHA_THRESHOLD
    expanded = binary_dilation(alpha, iterations=EXPAND)

    result = np.zeros((arr.shape[0], arr.shape[1], 4), dtype=np.uint8)
    result[expanded] = [255, 255, 255, 255]

    base, _ = os.path.splitext(input_path)
    output_path = base + "_outline.png"

    Image.fromarray(result).save(output_path)
    print(f"Saved: {output_path}\n")


def main():
    for path in TASKS:
        if not os.path.exists(path):
            print(f"File not found: {path}")
            continue
        generate_solid_outline(path)

    print("DONE")


if __name__ == "__main__":
    main()
