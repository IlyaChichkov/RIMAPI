import requests
import base64
import re
import numpy as np
from PIL import Image
from io import BytesIO

def process_unity_tint(url):
    # 1. Fetch Data
    response = requests.get(url)
    payload = response.json()
    
    if not payload.get("success"):
        return

    # 2. Parse and Normalize Color (0.0 to 1.0)
    # Unity colors in shaders are usually represented as floats 0-1
    color_str = payload["data"]["color"]
    rgba_floats = [float(x) for x in re.findall(r"[-+]?\d*\.\d+|\d+", color_str)]
    
    # 3. Decode the Base64 Icon
    img_b64 = payload["data"]["image"]["image_base64"]
    icon_bytes = base64.b64decode(img_b64)
    icon = Image.open(BytesIO(icon_bytes)).convert("RGBA")

    # 4. Apply Multiplicative Tint (Shader Logic)
    # Convert image to a numpy array of floats (0.0 - 1.0)
    data = np.array(icon).astype(float) / 255.0
    
    # Multiply R, G, B channels by the tint color
    # This is exactly what 'pixel.rgb * tint.rgb' does in a fragment shader
    data[..., 0] *= rgba_floats[0] # Red
    data[..., 1] *= rgba_floats[1] # Green
    data[..., 2] *= rgba_floats[2] # Blue
    data[..., 3] *= rgba_floats[3] # Alpha (Optional: usually tints don't affect A)

    # 5. Convert back to 0-255 and save
    final_data = (data * 255).astype(np.uint8)
    tinted_icon = Image.fromarray(final_data)
    
    tinted_icon.save("tinted_faction_icon.png")
    tinted_icon.show()
    print(f"Applied Unity-style tint: {rgba_floats}")

# process_unity_tint("<<baseURL>>/api/v1/faction/icon?id=1")

# Example usage (assuming the same endpoint)
process_unity_tint("http://localhost:8765/api/v1/faction/icon?id=1")