import requests
import sys

# --- CONFIGURATION ---
BASE_URL = "http://localhost:8765/api/v1"

# Endpoints
URL_GET_FACTIONS = f"{BASE_URL}/factions"
URL_SET_GOODWILL = f"{BASE_URL}/faction/goodwill"
URL_SPAWN_PAWN   = f"{BASE_URL}/pawn/spawn"

def get_factions():
    """
    Fetches all factions to find the Player's ID and a suitable Enemy ID/Name.
    """
    print(f"[-] Fetching factions from {URL_GET_FACTIONS}...")

    return 4, 5

def set_hostile(player_id, enemy_id):
    """
    Sets goodwill to -100 to ensure they fight.
    """
    print(f"[-] Setting goodwill between ID {player_id} and {enemy_id} to -100...")
    payload = {
        "id": player_id,
        "other_id": enemy_id,
        "value": -100,
        "send_message": False,
        "can_send_hostility_letter": False
    }
    
    try:
        resp = requests.post(URL_SET_GOODWILL, json=payload)
        resp.raise_for_status()
        print("[+] Factions are now hostile.")
    except Exception as e:
        print(f"[!] Failed to set hostility: {e}")

def create_lord(faction_name, pawn_ids, job_type="AssaultColony", target_ids=None):
    print(f"[-] Creating Lord '{job_type}' for {len(pawn_ids)} pawns of {faction_name}...")
    payload = {
        "faction": faction_name,
        "pawn_ids": pawn_ids,
        "job_type": job_type,
        "target_ids": target_ids
    }
    resp = requests.post(f"{BASE_URL}/lords/create", json=payload)
    print(f"    Result: {resp.text}")

def spawn_pawn(x, z, faction_name, pawn_kind="Colonist"):
    """
    Spawns a pawn at a specific location for a specific faction.
    """
    print(f"[-] Spawning '{pawn_kind}' for faction '{faction_name}' at ({x}, 0, {z})...")
    
    payload = {
        "pawn_kind": pawn_kind,
        "faction": faction_name,
        "position": {
            "x": x,
            "y": 0,
            "z": z
        },
        # Optional: Give them weapons/skills so the fight is interesting
        "biological_age": 25
    }

    try:
        resp = requests.post(URL_SPAWN_PAWN, json=payload)
        resp.raise_for_status()
        result = resp.json()
        print(f"[+] Spawned successfully! Response: {result}")

        return result.get("data", {}).get("pawn_id") or result.get("pawn_id")
    except Exception as e:
        print(f"[!] Spawn failed: {e}")
        if hasattr(e, 'response') and e.response:
             print(f"    Server said: {e.response.text}")

if __name__ == "__main__":
    # 1. Get Faction Info
    p_id, e_id = get_factions()
    
    # Use Name or DefName for the spawn string
    p_name = "OutlanderCivil"
    e_name = "AncientsHostile"

    print(f"[*] Player Faction: {p_name} (ID: {p_id})")
    print(f"[*] Enemy Faction:  {e_name} (ID: {e_id})")

    # 3. Spawn Player Pawn at (11, 0, 11)
    defender_id = spawn_pawn(11, 11, p_name, pawn_kind="Mercenary_Gunner")

    # 4. Spawn Enemy Pawn at (28, 0, 28)
    # Using 'Mercenary' or 'Pirate' kind usually gives them a weapon automatically
    attacker_id = spawn_pawn(28, 28, e_name, pawn_kind="Mercenary_Gunner")

    print(f"[*] A: {defender_id}")
    print(f"[*] B:  {attacker_id}")

    if defender_id and attacker_id:
        # 4. Create AI Lord for the Attacker
        # "AssaultThings" will make the attacker specifically hunt the defender
        create_lord(
            e_name,
            [attacker_id],
            job_type="AssaultThings",
            target_ids=[defender_id]
        )
        create_lord(
            p_name,
            [defender_id],
            job_type="AssaultThings",
            target_ids=[attacker_id]
        )
        
        # Alternatively, use "AssaultColony" for general raiding behavior:
        # create_lord(enemy_fac, [attacker_id], job_type="AssaultColony")
        
        print("[+] Battle Initiated!")
    else:
        print("[!] Failed to spawn pawns.")