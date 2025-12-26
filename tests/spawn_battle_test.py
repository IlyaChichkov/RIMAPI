import requests
import sys
import time

# --- CONFIGURATION ---
BASE_URL = "http://localhost:8765/api/v1"

# Endpoints
URL_GET_FACTIONS   = f"{BASE_URL}/factions"
URL_SET_GOODWILL   = f"{BASE_URL}/faction/goodwill"
URL_SPAWN_PAWN     = f"{BASE_URL}/pawn/spawn"
URL_CLEAR_RECT     = f"{BASE_URL}/map/destroy/rect"
URL_REPAIR_RECT     = f"{BASE_URL}/map/repair/rect"
URL_PAWN_DETAILS   = f"{BASE_URL}/pawns/details"
URL_CREATE_LORD    = f"{BASE_URL}/lords/create"
URL_ANNOUNCE = f"{BASE_URL}/ui/announce"

# Battle Settings
CHECK_INTERVAL = 2.0  # Seconds between checks
CRITICAL_HEALTH = 0.15 # 15% health (Assumed downed/dying threshold)

def announce_winner(text, color_hex):
    print(f"[-] Announcing: {text}")
    payload = {
        "text": text,
        "duration": 5.0,
        "color": color_hex,
        "scale": 3.0
    }
    try:
        requests.post(URL_ANNOUNCE, json=payload)
    except:
        pass

def repair_arena():
    
    print("[-] Repairing arena...")
    payload = {
        "map_id": 0,
        "point_a": {"x": 7, "y": 0, "z": 7},
        "point_b": {"x": 32, "y": 0, "z": 32}
    }
    try:
        requests.post(URL_REPAIR_RECT, json=payload)
    except Exception as e:
        print(f"[!] Error repairing arena: {e}")

def clear_arena():
    """Destroys all items, pawns, and filth in the battle area."""
    print("[-] Clearing arena...")
    payload = {
        "map_id": 0,
        "point_a": {"x": 10, "y": 0, "z": 10},
        "point_b": {"x": 29, "y": 0, "z": 29}
    }
    try:
        requests.post(URL_CLEAR_RECT, json=payload)
    except Exception as e:
        print(f"[!] Error clearing arena: {e}")

def get_pawn_status(pawn_id):
    """
    Returns a dictionary with status flags.
    """
    try:
        resp = requests.get(f"{URL_PAWN_DETAILS}?id={pawn_id}")
        if resp.status_code != 200:
            return {"dead": True, "downed": True, "hp": 0}
        
        data = resp.json().get("data", {})
        med_info = data.get("colonist_medical_info", {})
        
        return {
            "name": data.get("colonist", {}).get('name'),
            "dead": med_info.get("is_dead", False),
            "downed": med_info.get("is_downed", False),
            "moving": med_info.get("moving", 1.0),
            "consciousness": med_info.get("consciousness", 1.0),
            "hp": med_info.get("health", 0.0)
        }
    except Exception as e:
        print(f"[!] Error checking pawn {pawn_id}: {e}")
        return {"dead": True, "downed": True, "hp": 0}

def spawn_pawn(x, z, faction, kind):
    payload = {
        "pawn_kind": kind,
        "faction": faction,
        "position": {"x": x, "y": 0, "z": z},
        "biological_age": 25
    }
    try:
        resp = requests.post(URL_SPAWN_PAWN, json=payload)
        if resp.status_code == 200:
            data = resp.json()
            # Handle different casing from API (Pascal vs Snake)
            return data.get("data", {}).get("pawn_id") or data.get("pawn_id")
    except Exception as e:
        print(f"[!] Spawn failed: {e}")
    return None

def setup_battle(p_faction, e_faction):
    """Spawns units and sets up AI."""
    print("\n--- NEW ROUND STARTING ---")
    
    # 1. Spawn Units
    def_id = spawn_pawn(11, 11, p_faction, "Mercenary_Gunner")
    att_id = spawn_pawn(28, 28, e_faction, "Mercenary_Gunner")

    if not def_id or not att_id:
        print("[!] Failed to spawn one or both pawns.")
        return None, None

    print(f"[+] Spawned Defender ({def_id}) vs Attacker ({att_id})")

    # 2. Assign Lords (AI)
    # Force them to target each other specifically
    create_lord(e_faction, [att_id], "AssaultThings", [def_id])
    create_lord(p_faction, [def_id], "AssaultThings", [att_id])

    return def_id, att_id

def create_lord(faction, pawns, job, targets):
    payload = {
        "faction": faction,
        "pawn_ids": pawns,
        "job_type": job,
        "target_ids": targets
    }
    requests.post(URL_CREATE_LORD, json=payload)

def monitor_battle(id_a, id_b):
    """Loops until one pawn is incapacitated."""
    print("[-] Monitoring battle...")
    
    while True:
        time.sleep(1.0) # Check every second

        stat_a = get_pawn_status(id_a)
        stat_b = get_pawn_status(id_b)

        # Log detailed status
        print(f"    A: {int(stat_a['hp']*100)}% HP (Mov: {int(stat_a['moving']*100)}%) | "
              f"B: {int(stat_b['hp']*100)}% HP (Mov: {int(stat_b['moving']*100)}%)")

        # WIN CONDITION: Enemy is Dead OR Downed
        if stat_a['dead'] or stat_a['downed']:
            print(f"[!] Defender ({id_a}) lost! (Dead: {stat_a['dead']}, Downed: {stat_a['downed']})")
            msg = f"Attacker ({stat_b['name']}) WINS!"
            announce_winner(msg, "#00FF00") # Green
            return "B_WINS"
            
        if stat_b['dead'] or stat_b['downed']:
            print(f"[!] Attacker ({id_b}) lost! (Dead: {stat_b['dead']}, Downed: {stat_b['downed']})")
            msg = f"Defender ({stat_a['name']}) WINS!"
            announce_winner(msg, "#00FF00") # Green
            return "A_WINS"

# --- MAIN EXECUTION ---
if __name__ == "__main__":
    # Settings
    p_name = "OutlanderCivil"
    e_name = "AncientsHostile" # Ensure this faction exists, or use 'Pirate'

    print(f"[*] Starting Auto-Battle Loop: {p_name} vs {e_name}")

    while True:
        # 1. Clean up previous mess
        repair_arena()
        clear_arena()
        
        # 2. Setup new fight
        id_1, id_2 = setup_battle(p_name, e_name)
        
        if id_1 and id_2:
            # 3. Wait for result
            monitor_battle(id_1, id_2)
            
            print("[-] Round over. Restarting in 3 seconds...")
            time.sleep(3)
        else:
            print("[!] Setup failed. Retrying in 5 seconds...")
            time.sleep(5)