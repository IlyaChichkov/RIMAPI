import requests
import json
import sys

# --- Configuration ---
API_URL = "http://localhost:8765"  # Change port if needed
SPAWN_ENDPOINT = f"{API_URL}/api/v1/pawn/spawn"
EDIT_SKILLS_ENDPOINT = f"{API_URL}/api/v1/pawn/edit/skills"

def spawn_pawn():
    """Spawns a pawn and returns the ID."""
    print(f"[-] Sending Spawn Request to {SPAWN_ENDPOINT}...")
    
    # Payload in snake_case (assuming your API handles snake_case -> PascalCase conversion)
    payload = {
        "pawn_kind": "Colonist",
        "faction": "PlayerColony",
        "xenotype": "Human ",
        "gender": "Female",
        "biological_age": 28,
        "chronological_age": 150,
        "first_name": "Elena",
        "last_name": "Vokul",
        "nick_name": "Ellie",
        "map_id": "0",
        "position": {
            "x": 140,
            "y": 0,
            "z": 108
        },
        "allow_dead": False,
        "allow_downed": False,
        "can_generate_pawn_relations": False,
        "must_be_capable_of_violence": True,
        "allow_gay": False,
        "allow_pregnant": False,
        "allow_food": False,
        "allow_addictions": False,
        "inhabitant": False
    }

    try:
        response = requests.post(SPAWN_ENDPOINT, json=payload)
        response.raise_for_status() # Raise error for 4xx/5xx
        
        data = response.json()
        
        # Check if the API wrapped the response (e.g., Result object) or returned raw data
        # Adjust based on your ApiResult structure. 
        # Assuming format: { "Success": true, "Data": { "pawn_id": 123, ... } }
        
        # NOTE: Keys here might be PascalCase or camelCase depending on your serializer settings
        pawn_id = data.get("data", {}).get("pawn_id") or data.get("pawn_id")
        
        if pawn_id:
            print(f"[+] Successfully spawned pawn! ID: {pawn_id}")
            return pawn_id
        else:
            print(f"[!] Spawn successful but ID not found in response: {data}")
            return None

    except requests.exceptions.RequestException as e:
        print(f"[!] Spawn Failed: {e}")
        if e.response is not None:
            print(f"    Response: {e.response.text}")
        return None

def edit_skills(pawn_id):
    """Edits skills for the given pawn ID."""
    print(f"\n[-] Sending Edit Skills Request to {EDIT_SKILLS_ENDPOINT}...")

    payload = {
        "pawn_id": pawn_id,
        "skills": [
            {
                "skill_name": "Shooting",
                "level": 20,
                "passion": "Major"  # None, Minor, Major
            },
            {
                "skill_name": "Medicine",
                "level": 15,
                "passion": "Minor"
            },
            {
                "skill_name": "Melee",
                "level": 0,
                "passion": "None"
            }
        ]
    }

    try:
        response = requests.post(EDIT_SKILLS_ENDPOINT, json=payload)
        response.raise_for_status()
        
        print(f"[+] Skills updated successfully.")
        print(f"    Response: {response.text}")

    except requests.exceptions.RequestException as e:
        print(f"[!] Edit Skills Failed: {e}")
        if e.response is not None:
            print(f"    Response: {e.response.text}")

if __name__ == "__main__":
    # 1. Spawn
    new_pawn_id = spawn_pawn()

    # 2. Edit (only if spawn worked)
    if new_pawn_id:
        edit_skills(new_pawn_id)
    else:
        print("\n[!] Aborting edit test because spawn failed.")