import requests
import base64
import struct
import sys

# Configuration
API_URL = "http://localhost:8765/api/v1/map/fog-grid?map_id=0" # Adjust if your controller route is different
MAP_ID = 0 # -1 for current map
VIEW_WIDTH = 50
VIEW_HEIGHT = 50

# Visual characters
CHAR_FOG = "█"
CHAR_CLEAR = "░"

def decode_rle_fog(base64_str, total_expected_cells):
    """
    Decodes Base64 -> Bytes -> Int32 Array -> Boolean Array
    """
    # 1. Decode Base64 to bytes
    try:
        compressed_bytes = base64.b64decode(base64_str)
    except Exception as e:
        print(f"Error decoding Base64: {e}")
        return []

    # 2. Unpack bytes to list of integers (Little Endian Int32)
    # 4 bytes per integer
    count = len(compressed_bytes) // 4
    # struct format: < = little-endian, i = int (4 bytes)
    # We create a format string like "<500i" based on count
    try:
        rle_counts = struct.unpack(f"<{count}i", compressed_bytes)
    except struct.error as e:
        print(f"Error unpacking binary data: {e}")
        return []

    # 3. Reconstruct grid
    grid = []
    is_fogged = False # C# implementation starts with currentVal = false (Revealed)
    
    for length in rle_counts:
        # Append 'length' amount of the current state
        grid.extend([is_fogged] * length)
        # Flip state for next run
        is_fogged = not is_fogged

    if len(grid) != total_expected_cells:
        print(f"Warning: Decoded grid size ({len(grid)}) does not match map dimensions ({total_expected_cells})")
        
    return grid

def main():
    print(f"Fetching fog data from {API_URL}...")
    
    try:
        response = requests.get(API_URL)
        response.raise_for_status()
        json_resp = response.json()
        
        # Handle ApiResult wrapper if present
        if not json_resp.get("success", True):
            print(f"API Error: {json_resp.get('message')}")
            return

        data = json_resp.get("data", json_resp)
        print(data)
        
        width = data["width"]
        height = data["height"]
        base64_data = data["fog_data"]
        
        
        # Decode
        grid = decode_rle_fog(base64_data, width * height)
        print(grid)
        
        if not grid:
            print("Failed to decode grid.")
            return

        print(f"\nVisualizing 0-{VIEW_WIDTH}, 0-{VIEW_HEIGHT} (Bottom-Left origin):")
        print("-" * (VIEW_WIDTH + 2))

        # RimWorld Z is "Up" (Rows). 
        # Standard loop 0->Height prints "upside down" relative to how we usually read maps 
        # (where row 0 is top). 
        # We will print Z=49 down to Z=0 to match visual map orientation (Top-North).
        
        start_z = min(VIEW_HEIGHT, height) - 1
        
        for z in range(start_z, -1, -1):
            row_str = ""
            for x in range(0, min(VIEW_WIDTH, width)):
                # 1D Array Index Formula: (z * width) + x
                index = (z * width) + x
                
                if index < len(grid):
                    is_fogged = grid[index]
                    row_str += CHAR_FOG if is_fogged else CHAR_CLEAR
                else:
                    row_str += "?"
            
            # Print row number and data
            print(f"{z:02} {row_str}")

        print("-" * (VIEW_WIDTH + 2))
        print(f"Legend: {CHAR_FOG} = Fogged (Hidden), {CHAR_CLEAR} = Revealed")

    except requests.exceptions.RequestException as e:
        print(f"HTTP Request failed: {e}")
    except KeyError as e:
        print(f"Unexpected JSON format, missing key: {e}")
    except Exception as e:
        print(f"An error occurred: {e}")

if __name__ == "__main__":
    main()