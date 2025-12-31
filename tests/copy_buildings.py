import requests
import json

BASE_URL = "http://localhost:8765/api/v1"

def copy_and_paste():
    # 1. COPY
    print("[-] Copying area (10,10) to (15,15)...")
    copy_payload = {
        "map_id": 0,
        "point_a": {"x": 10, "y": 0, "z": 10},
        "point_b": {"x": 16, "y": 0, "z": 16}
    }
    
    resp = requests.post(f"{BASE_URL}/builder/copy", json=copy_payload)
    if resp.status_code != 200:
        print(f"[!] Copy failed: {resp.text}")
        return

    blueprint_data = resp.json().get("data") # Get the blueprint object
    print(f"blueprint_data: {blueprint_data }")
    print(f"[+] Copy successful! Found {len(blueprint_data.get('buildings', []))} buildings.")

    # 2. PASTE
    print("[-] Pasting to (40, 40)...")
    paste_payload = {
        "map_id": 0,
        "position": {"x": 40, "y": 0, "z": 40},
        "blueprint": blueprint_data,
        "clear_obstacles": True
    }

    resp = requests.post(f"{BASE_URL}/builder/paste", json=paste_payload)
    print(f"[+] Paste Result: {resp.text}")
    
    print("[-] Placing Blueprints at (80, 80)...")
    payload = {
        "map_id": 0,
        "position": {"x": 80, "y": 0, "z": 80},
        "blueprint": blueprint_data
    }

    # Note the different endpoint: /builder/blueprint
    resp = requests.post(f"{BASE_URL}/builder/blueprint", json=payload)
    print(f"[+] Result: {resp.text}")

if __name__ == "__main__":
    copy_and_paste()