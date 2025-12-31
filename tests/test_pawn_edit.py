import unittest
import requests
import json

# CONFIGURATION
BASE_URL = "http://localhost:8765/api/v1"
TEST_PAWN_ID = 940  # CHANGE THIS to a valid pawn ID currently in your game

class TestPawnEditEndpoints(unittest.TestCase):

    def _post(self, endpoint, payload):
        """Helper to send POST requests and print useful debug info on failure."""
        url = f"{BASE_URL}{endpoint}"
        headers = {'Content-Type': 'application/json'}
        
        try:
            response = requests.post(url, json=payload, headers=headers)
            
            # Print failure details if status code is bad (4xx or 5xx)
            if not response.ok:
                print(f"\n[FAIL] {endpoint} Status: {response.status_code}")
                print(f"Response: {response.text}")
            
            self.assertEqual(response.status_code, 200, f"API returned {response.status_code}")
            
            # Check for API-level 'success' flag if your API wraps responses
            json_resp = response.json()
            if 'success' in json_resp:
                 self.assertTrue(json_resp['success'], f"API reported failure: {json_resp.get('errors')}")

            return json_resp

        except requests.exceptions.ConnectionError:
            self.fail(f"Could not connect to {url}. Is RimWorld running?")

    # --- 1. Basic Info ---
    def test_edit_basic(self):
        print("\nTesting Basic Info Edit...")
        payload = {
            "pawn_id": TEST_PAWN_ID,
            "nick_name": "Snake",
            "first_name": "David",
            "last_name": "Hayter",
            "gender": "Male",
            "biological_age": 35,
            "chronological_age": 150
        }
        self._post("/pawn/edit/basic", payload)

    # --- 2. Health ---
    def test_edit_health(self):
        print("Testing Health Edit...")
        payload = {
            "pawn_id": TEST_PAWN_ID,
            "heal_all_injuries": True,
            "restore_body_parts": True,
            "remove_all_diseases": True
        }
        self._post("/pawn/edit/health", payload)

    # --- 3. Needs ---
    def test_edit_needs(self):
        print("Testing Needs Edit...")
        payload = {
            "pawn_id": TEST_PAWN_ID,
            "food": 1.0,  # 100%
            "rest": 1.0,
            "mood": 1.0
        }
        self._post("/pawn/edit/needs", payload)

    # --- 4. Skills ---
    def test_edit_skills(self):
        print("Testing Skills Edit...")
        payload = {
            "pawn_id": TEST_PAWN_ID,
            "skills": [
                {
                    "skill_name": "Shooting",
                    "level": 20,
                    "passion": 2
                },
                {
                    "skill_name": "Melee",
                    "level": 10,
                    "passion": 1
                },
                {
                    "skill_name": "Cooking",
                    "level": 0,
                    "passion": 0
                }
            ]
        }
        self._post("/pawn/edit/skills", payload)

    # --- 5. Traits ---
    def test_edit_traits(self):
        print("Testing Traits Edit...")
        payload = {
            "pawn_id": TEST_PAWN_ID,
            "add_traits": [
                {
                    "trait_name": "Cannibal",
                    "degree": 0
                },
                {
                    "trait_name": "Psychopath",
                    "degree": 0 
                }
            ],
            "remove_traits": ["Wimp", "Abrasive"]
        }
        self._post("/pawn/edit/traits", payload)

    # --- 6. Inventory ---
    def test_edit_inventory(self):
        print("Testing Inventory Edit...")
        payload = {
            "pawn_id": TEST_PAWN_ID,
            "clear_inventory": True, # Wipe existing
            "drop_inventory": False,
            "add_items": [
                {
                    "def_name": "MealSurvivalPack",
                    "count": 5
                },
                {
                    "def_name": "MedicineIndustrial",
                    "count": 10
                }
            ]
        }
        self._post("/pawn/edit/inventory", payload)

    # --- 7. Apparel ---
    def test_edit_apparel(self):
        print("Testing Apparel Edit (Stripping)...")
        payload = {
            "pawn_id": TEST_PAWN_ID,
            "drop_apparel": True, # Strip clothes
            "drop_weapons": True  # Strip weapons
        }
        self._post("/pawn/edit/apparel", payload)

    # --- 8. Status ---
    def test_edit_status(self):
        print("Testing Status Edit (Drafting)...")
        # Be careful with Kill/Resurrect in automated tests
        payload = {
            "pawn_id": TEST_PAWN_ID,
            "is_drafted": True,
            "kill": False,
            "resurrect": False
        }
        self._post("/pawn/edit/status", payload)

    # --- 9. Position ---
    def test_edit_position(self):
        print("Testing Position Edit (Teleport)...")
        payload = {
            "pawn_id": TEST_PAWN_ID,
            # "map_id": "0", # Optional if we want to move maps
            "position": {
                "x": 120,
                "z": 120 # Uses 'z' usually for Map coordinates, but check if your DTO uses 'y'
            }
        }
        self._post("/pawn/edit/position", payload)

    # --- 10. Faction ---
    def test_edit_faction(self):
        print("Testing Faction Edit...")
        payload = {
            "pawn_id": TEST_PAWN_ID,
            "set_faction": "PlayerColony", 
            "make_colonist": True,
            "make_prisoner": False,
            "release_prisoner": False
        }
        self._post("/pawn/edit/faction", payload)

if __name__ == '__main__':
    unittest.main()