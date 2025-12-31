import requests
import json
import time
import threading
import sys

# --- CONFIGURATION ---
BASE_URL = "http://localhost:8765/api/v1"
SSE_URL  = f"{BASE_URL}/events"
URL_DIALOG = f"{BASE_URL}/ui/dialog"
URL_MESSAGE = f"{BASE_URL}/ui/message"
URL_DROP_POD = f"{BASE_URL}/map/droppod"
URL_SPAWN_PAWN = f"{BASE_URL}/pawn/spawn"
URL_CREATE_LORD = f"{BASE_URL}/lords/create" # New Endpoint

# Global flag
quest_active = True

QUEST_TITLE = "Distress Signal"
QUEST_TEXT = (
    "You pick up a faint transmission from a nearby drop pod crash.\n\n"
    "It appears to be a Glitterworld medical transport. "
    "Sensors detect valuable supplies, but also active defense mechanoids rebooting nearby."
)

OPTIONS = [
    {"label": "Secure the supplies", "action_id": "loot", "resolve_tree": True},
    {"label": "Hack the mechanoids", "action_id": "hack", "resolve_tree": True},
    {"label": "Ignore signal", "action_id": "ignore", "resolve_tree": True}
]

def show_quest():
    print(f"[-] Sending Quest Dialog...")
    requests.post(URL_DIALOG, json={
        "title": QUEST_TITLE,
        "text": QUEST_TEXT,
        "options": OPTIONS
    })

def create_lord(faction, pawns, job, targets=None):
    print(f"[-] AI COMMAND: Assigning {job} to {len(pawns)} pawns.")
    payload = {
        "faction": faction,
        "pawn_ids": pawns,
        "job_type": job,
        "target_ids": targets or []
    }
    requests.post(URL_CREATE_LORD, json=payload)

def spawn_reward_pod(items, x, z):
    print(f"[+] INCOMING DROP POD at ({x}, {z}) containing {len(items)} stacks.")
    payload = {
        "map_id": 0,
        "position": {"x": x, "y": 0, "z": z},
        "items": items,
        "open_delay": True
    }
    try:
        requests.post(URL_DROP_POD, json=payload)
    except Exception as e:
        print(f"[!] Network Error: {e}")

def spawn_enemy(kind, x, z):
    print(f"[!] SPAWNING ENEMY: {kind} at ({x}, {z})")
    try:
        resp = requests.post(URL_SPAWN_PAWN, json={
            "pawn_kind": kind,
            "faction": "Mechanoid",
            "position": {"x": x, "y": 0, "z": z}
        })
        if resp.status_code == 200:
            # Handle different casing from API response
            data = resp.json().get("data", {})
            return data.get("pawn_id") or data.get("pawn_id")
    except Exception as e:
        print(f"[!] Spawn Error: {e}")
    return None

def handle_choice(action_id):
    print(f"\n[+] HANDLING CHOICE: {action_id}")
    
    # Coordinates 
    drop_x, drop_z = 12, 12
    enemy_x, enemy_z = 15, 15
    
    if action_id == "loot":
        requests.post(URL_MESSAGE, json={"text": "You grab the loot, but the mechanoids wake up!", "button_text": "To Arms!"})
        
        # 1. Spawn Reward
        spawn_reward_pod([
            {"def_name": "Bullet_BoltActionRifle", "quality": 3, "count": 1},
        ], drop_x, drop_z)

        # 2. Spawn Enemy
        mech_id = spawn_enemy("Mech_Lancer", enemy_x, enemy_z)
        print('mech_id: ',mech_id)
        
        # 3. Assign Lord (Combat AI)
        if mech_id:
            # AssaultColony makes them attack any player structure/pawn
            create_lord("Mechanoid", [mech_id], "AssaultColony")

    elif action_id == "hack":
        requests.post(URL_MESSAGE, json={"text": "Hack successful! You override the drop pod locks safely.", "button_text": "Excellent"})
        
        spawn_reward_pod([
            {"def_name": "Gun_BoltActionRifle", "quality": 3, "count": 1},
        ], drop_x, drop_z)

    elif action_id == "ignore":
        print("[-] Quest ignored.")

    global quest_active
    quest_active = False

def process_event(event_type, data):
    if event_type == "dialog_option_selected":
        label = data.get("option", {}).get("label")
        print(f"[*] Listener: Detected click on '{label}'")
        
        found_id = next((o["action_id"] for o in OPTIONS if o["label"] == label), None)
        if found_id:
            handle_choice(found_id)

def sse_listener_thread():
    print("[-] Listener: Connecting to Event Stream...")
    current_event_type = None

    try:
        with requests.get(SSE_URL, stream=True) as response:
            if response.status_code == 200:
                print("[+] Listener: Connected & Ready.")
                for line in response.iter_lines():
                    if not quest_active: break
                    if line:
                        decoded = line.decode('utf-8')
                        if decoded.startswith("event:"):
                            current_event_type = decoded[6:].strip()
                        elif decoded.startswith("data:"):
                            json_str = decoded[5:].strip()
                            try:
                                data = json.loads(json_str)
                                process_event(current_event_type, data)
                            except json.JSONDecodeError:
                                pass
    except Exception as e:
        print(f"[!] Listener Error: {e}")

if __name__ == "__main__":
    t = threading.Thread(target=sse_listener_thread)
    t.start()

    time.sleep(1.0) 
    show_quest()

    try:
        while quest_active:
            time.sleep(0.5)
    except KeyboardInterrupt:
        quest_active = False
    
    print("[-] Quest finished. Exiting.")