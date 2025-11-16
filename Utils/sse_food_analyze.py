#!/usr/bin/env python3
import json
import sys
import time
from collections import defaultdict

import requests

# --- CONFIG ---

SSE_URL = "http://localhost:8765/api/v1/events"

RESOURCES_URL = "http://localhost:8765/api/v1/resources/stored"
COLONISTS_URL = "http://localhost:8765/api/v1/colonists"

TICKS_PER_DAY = 60000  # RimWorld vanilla


# --- Data aggregation ---

class FoodStats:
    def __init__(self):
        # day_index -> stats
        self.days = defaultdict(lambda: {
            "nutrition_consumed": 0.0,
            "nutrition_produced": 0.0,
            "consumed_by_food": defaultdict(lambda: {"count": 0, "nutrition": 0.0}),
            "produced_by_food": defaultdict(lambda: {"count": 0, "nutrition": 0.0}),
        })
        # Learned nutrition per meal def (from events)
        # def_name -> {"total_nutrition": float, "items": int}
        self.nutrition_per_def = defaultdict(lambda: {"total_nutrition": 0.0, "items": 0})

    def _day_for_ticks(self, ticks: int) -> int:
        return ticks // TICKS_PER_DAY

    # --- Nutrition learning helpers ---

    def get_recent_production_avg(self, days_back=3):
        """Average nutrition produced per day over the last N days (if data)."""
        if not self.days:
            return None

        all_days = sorted(self.days.keys())
        last_days = all_days[-days_back:]
        total = 0.0
        counted_days = 0
        for d in last_days:
            stats = self.days[d]
            if stats["nutrition_produced"] > 0:
                total += stats["nutrition_produced"]
                counted_days += 1
        if counted_days == 0:
            return None
        return total / counted_days

    def _update_nutrition_def(self, def_name: str, nutrition: float):
        if nutrition <= 0:
            return
        info = self.nutrition_per_def[def_name]
        info["total_nutrition"] += nutrition
        info["items"] += 1

    def get_avg_nutrition_for_def(self, def_name: str) -> float:
        info = self.nutrition_per_def.get(def_name)
        if info and info["items"] > 0:
            return info["total_nutrition"] / info["items"]
        # Fallback guesses for common RimWorld meals (rough)
        if def_name == "MealSimple":
            return 0.9
        if def_name == "MealFine":
            return 0.9
        if def_name == "MealLavish":
            return 0.9
        if def_name == "Pemmican":
            return 0.25
        if def_name == "MealSurvivalPack":
            return 0.9
        return 0.9  # generic guess

    # --- Event handlers ---

    def handle_colonist_ate(self, data: dict):
        """Handle a 'colonist_ate' event."""
        try:
            ticks = data.get("ticks")
            if ticks is None:
                return
            day = self._day_for_ticks(ticks)

            food = data.get("food", {})
            def_name = (food.get("defName")
                        or food.get("def_name")
                        or "Unknown")
            nutrition = float(food.get("nutrition", 0.0))

            day_stats = self.days[day]
            day_stats["nutrition_consumed"] += nutrition

            f = day_stats["consumed_by_food"][def_name]
            f["count"] += 1
            f["nutrition"] += nutrition

            self._update_nutrition_def(def_name, nutrition)
        except Exception as e:
            print(f"[WARN] Failed to handle colonist_ate: {e}", file=sys.stderr)

    def handle_make_recipe_product(self, data: dict):
        """Handle a 'make_recipe_product' event."""
        try:
            ticks = data.get("ticks")
            if ticks is None:
                return

            day = self._day_for_ticks(ticks)
            results = data.get("result") or []
            day_stats = self.days[day]

            for item in results:
                def_name = (item.get("def_name")
                            or item.get("defName")
                            or "Unknown")
                nutrition = float(item.get("nutrition", 0.0))

                day_stats["nutrition_produced"] += nutrition

                f = day_stats["produced_by_food"][def_name]
                f["count"] += 1
                f["nutrition"] += nutrition

                self._update_nutrition_def(def_name, nutrition)
        except Exception as e:
            print(f"[WARN] Failed to handle make_recipe_product: {e}", file=sys.stderr)

    # --- Reporting ---

    def get_recent_consumption_avg(self, days_back=3):
        """Average nutrition consumed per day over the last N days (if data)."""
        if not self.days:
            return None

        all_days = sorted(self.days.keys())
        last_days = all_days[-days_back:]
        total = 0.0
        counted_days = 0
        for d in last_days:
            stats = self.days[d]
            if stats["nutrition_consumed"] > 0:
                total += stats["nutrition_consumed"]
                counted_days += 1
        if counted_days == 0:
            return None
        return total / counted_days

    def build_stored_summary(self, stored_items: list):
        """
        Build a summary from /resources/stored.
        Returns dict with:
          total_meals, total_nutrition_est, by_def
        """
        total_meals = 0
        total_nutrition_est = 0.0
        by_def = {}

        for it in stored_items:
            def_name = it.get("def_name") or "Unknown"
            stack = int(it.get("stack_count", 0))

            total_meals += stack

            avg_nut = self.get_avg_nutrition_for_def(def_name)
            total_nutrition_est += avg_nut * stack

            entry = by_def.setdefault(def_name, {
                "count": 0,
                "stacks": 0,
                "example_label": it.get("label", "")
            })
            entry["count"] += stack
            entry["stacks"] += 1

        return {
            "total_meals": total_meals,
            "total_nutrition_est": total_nutrition_est,
            "by_def": by_def,
        }

    def print_day(self, day: int, stored_snapshot: dict | None, colonist_count: int | None):
        """
        Print summary for a single in-game day:
        - production/consumption (if we saw events)
        - per-food breakdown
        - stored meals + estimated days of food
        - deeper analysis: days left, required production, shortfall
        """

        # Ensure we always have a stats dict (even if no events)
        stats = self.days.get(day)
        if stats is None:
            stats = {
                "nutrition_consumed": 0.0,
                "nutrition_produced": 0.0,
                "consumed_by_food": defaultdict(lambda: {"count": 0, "nutrition": 0.0}),
                "produced_by_food": defaultdict(lambda: {"count": 0, "nutrition": 0.0}),
            }

        consumed = stats["nutrition_consumed"]
        produced = stats["nutrition_produced"]
        net = produced - consumed

        print(f"\n====== Day {day} ======")

        if consumed == 0 and produced == 0:
            print("  (no food events recorded for this day while the script was running)")
        else:
            print(f"  Nutrition consumed today : {consumed:.2f}")
            print(f"  Nutrition produced today : {produced:.2f}")
            print(f"  Net nutrition (prod-cons): {net:+.2f}")

            # Per-type breakdown
            if stats["consumed_by_food"]:
                print("  Consumed by food type:")
                for def_name, info in sorted(
                    stats["consumed_by_food"].items(),
                    key=lambda kv: kv[1]["nutrition"],
                    reverse=True
                ):
                    print(f"    - {def_name}: {info['nutrition']:.2f} (x{info['count']})")

            if stats["produced_by_food"]:
                print("  Produced by food type:")
                for def_name, info in sorted(
                    stats["produced_by_food"].items(),
                    key=lambda kv: kv[1]["nutrition"],
                    reverse=True
                ):
                    print(f"    - {def_name}: {info['nutrition']:.2f} (x{info['count']})")

        # --- Stored meals snapshot ---
        total_meals = None
        total_nutrition_est = None
        per_colonist_today = None
        days_left_today = None
        days_left_recent = None

        if stored_snapshot is not None:
            total_meals = stored_snapshot.get("total_meals", 0)
            total_nutrition_est = stored_snapshot.get("total_nutrition_est", 0.0)

            print("\n  Stored meals at end of day:")
            print(f"    Total meals (stacks summed) : {total_meals}")
            print(f"    Estimated stored nutrition  : {total_nutrition_est:.2f}")

            if colonist_count and colonist_count > 0:
                # If we saw no consumption today, assume ~0.9 per colonist
                if consumed == 0:
                    per_colonist_today = 0.9
                    print("    (no consumption events seen; assuming ~0.9 nutrition/colonist/day)")
                else:
                    per_colonist_today = consumed / colonist_count
                    print(f"    Nutrition consumed per colonist today: {per_colonist_today:.2f}")

                if per_colonist_today > 0:
                    colony_need_today = per_colonist_today * colonist_count
                    days_left_today = total_nutrition_est / colony_need_today
                    print(f"    ≈ {days_left_today:.1f} days of food at today's consumption")
                else:
                    print("    (cannot compute days of food: zero per-colonist consumption)")

            by_def = stored_snapshot.get("by_def", {})
            if by_def:
                print("  Stored meals by type:")
                for def_name, info in sorted(
                    by_def.items(),
                    key=lambda kv: kv[1]["count"],
                    reverse=True
                ):
                    label = info.get("example_label", def_name)
                    print(f"    - {def_name} ({label}): {info['count']} (stacks: {info['stacks']})")

        # --- Deeper analysis: trend & required production ---

        # Average daily consumption / production over last N days
        recent_cons = self.get_recent_consumption_avg(days_back=3)
        recent_prod = self.get_recent_production_avg(days_back=3)

        print("\n  Food plan & forecast:")

        if recent_cons is not None:
            print(f"    Recent avg daily consumption : {recent_cons:.2f} nutrition/day")
        else:
            print("    Recent avg daily consumption : (not enough data)")

        if recent_prod is not None:
            print(f"    Recent avg daily production  : {recent_prod:.2f} nutrition/day")
        else:
            print("    Recent avg daily production  : (not enough data)")

        if recent_cons is not None and recent_prod is not None:
            diff = recent_prod - recent_cons
            if diff >= 0:
                print(f"    Balance (prod - cons)        : +{diff:.2f} (you are slowly STOCKING up)")
            else:
                print(f"    Balance (prod - cons)        : {diff:.2f} (you are slowly STARVING)")

            if diff < 0:
                print(f"    => You need +{-diff:.2f} more nutrition/day to break even.")
        else:
            print("    => Not enough history yet to judge long-term balance.")

        # If we know stored nutrition and recent consumption, estimate days left
        if stored_snapshot is not None and recent_cons is not None and recent_cons > 0:
            total_nutrition_est = stored_snapshot.get("total_nutrition_est", 0.0)
            days_left_recent = total_nutrition_est / recent_cons
            print(f"    Days of food at recent usage : {days_left_recent:.1f} days")

        # Suggest required production target explicitly
        if recent_cons is not None:
            print(f"    Required production to maintain demand: {recent_cons:.2f} nutrition/day")
            # If your main food is MealSimple, translate to meals/day
            main_meal_def = "MealSimple"
            avg_meal_nut = self.get_avg_nutrition_for_def(main_meal_def)
            meals_per_day_needed = recent_cons / avg_meal_nut
            print(f"    (~{meals_per_day_needed:.1f} {main_meal_def} per day)")

        # Risk warnings
        if recent_cons is not None and recent_prod is not None and recent_prod < recent_cons:
            print("    ⚠ You are not producing enough food on average. Increase cooking or reduce consumption (animals, lavish meals, drugs).")

        if stored_snapshot is not None and days_left_recent is not None:
            if days_left_recent < 3:
                print("    ⚠ Critical: less than 3 days of food at recent usage.")
            elif days_left_recent < 7:
                print("    ℹ Warning: less than a week of food at recent usage.")


    def print_summary(self):
        if not self.days:
            print("No data collected yet.")
            return
        for d in sorted(self.days.keys()):
            self.print_day(d, None, None)


# --- REST helpers ---

def fetch_stored_items(map_id=0, category="food_meals", timeout=5.0):
    try:
        params = {"map_id": map_id, "category": category}
        resp = requests.get(RESOURCES_URL, params=params, timeout=timeout)
        resp.raise_for_status()
        return resp.json()
    except Exception as e:
        print(f"[WARN] Failed to fetch stored resources: {e}", file=sys.stderr)
        return []


def fetch_colonist_count(timeout=5.0):
    """Use /colonists?fields=id,age but only care about count."""
    try:
        params = {"fields": "id,age"}
        resp = requests.get(COLONISTS_URL, params=params, timeout=timeout)
        resp.raise_for_status()
        colonists = resp.json()
        return len(colonists or [])
    except Exception as e:
        print(f"[WARN] Failed to fetch colonists: {e}", file=sys.stderr)
        return None


# --- SSE client ---

def sse_client(url: str):
    """
    Minimal SSE client:
    - reads raw bytes
    - decodes as UTF-8
    - yields (event_type, data_dict)
    """
    with requests.get(url, stream=True) as resp:
        resp.raise_for_status()

        event_type = None
        data_lines = []

        for raw_line in resp.iter_lines(decode_unicode=False):
            if raw_line is None:
                continue

            line = raw_line.decode("utf-8", errors="replace").rstrip("\r\n")

            if not line:
                # end of event
                if event_type and data_lines:
                    raw_data = "\n".join(data_lines)
                    try:
                        data = json.loads(raw_data)
                    except json.JSONDecodeError:
                        print(
                            f"[WARN] Failed to decode JSON for event '{event_type}': {raw_data!r}",
                            file=sys.stderr
                        )
                        data = {}
                    yield event_type, data

                event_type = None
                data_lines = []
                continue

            if line.startswith("event:"):
                event_type = line[len("event:"):].strip()
            elif line.startswith("data:"):
                data_lines.append(line[len("data:"):].strip())
            # ignore id:, retry:, etc.


# --- Main loop ---

def main():
    stats = FoodStats()
    last_day_seen = None

    while True:
        print(f"Connecting to SSE at {SSE_URL} ...")
        try:
            # one SSE session
            for event_type, data in sse_client(SSE_URL):
                # print(f"EVENT: {event_type}, data: {data}")  # debug

                if event_type == "colonist_ate":
                    stats.handle_colonist_ate(data)

                elif event_type == "make_recipe_product":
                    stats.handle_make_recipe_product(data)

                elif event_type == "date_changed":
                    ticks = data.get("ticksGame") or data.get("ticks") or 0
                    current_day = ticks // TICKS_PER_DAY

                    if last_day_seen is None:
                        # First date_changed after startup: we just entered current_day.
                        previous_day = current_day - 1
                        if previous_day >= 0:
                            stored_items = fetch_stored_items(map_id=0, category="food_meals")
                            stored_summary = stats.build_stored_summary(stored_items)
                            colonist_count = fetch_colonist_count()
                            stats.print_day(previous_day, stored_summary, colonist_count)

                        last_day_seen = current_day

                    else:
                        if current_day > last_day_seen:
                            previous_day = last_day_seen

                            stored_items = fetch_stored_items(map_id=0, category="food_meals")
                            stored_summary = stats.build_stored_summary(stored_items)
                            colonist_count = fetch_colonist_count()
                            stats.print_day(previous_day, stored_summary, colonist_count)

                            last_day_seen = current_day

                time.sleep(0.01)

            # If we exit the for-loop normally, the stream ended.
            print("[INFO] SSE stream ended (server closed connection). Reconnecting in 3s...")
            time.sleep(3)

        except KeyboardInterrupt:
            print("\nInterrupted by user. Printing full summary (no snapshots)...")
            stats.print_summary()
            return

        except requests.RequestException as e:
            print(f"[ERROR] SSE connection error: {e}. Reconnecting in 3s...")
            time.sleep(3)

        except Exception as e:
            print(f"[ERROR] Unexpected error in main loop: {e}. Reconnecting in 3s...")
            time.sleep(3)



if __name__ == "__main__":
    main()
