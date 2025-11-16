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
        self.days = defaultdict(lambda: {
            "nutrition_consumed": 0.0,
            "nutrition_produced": 0.0,
            "consumed_by_food": defaultdict(lambda: {"count": 0, "nutrition": 0.0}),
            "produced_by_food": defaultdict(lambda: {"count": 0, "nutrition": 0.0}),
        })
        self.nutrition_per_def = defaultdict(lambda: {"total_nutrition": 0.0, "items": 0})
        self.previous_day_stats = {}  # Store previous day for comparison

    def _day_for_ticks(self, ticks: int) -> int:
        return ticks // TICKS_PER_DAY

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
        # Fallback guesses for common RimWorld meals
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

    def _get_day_comparison(self, day: int, current_stats: dict, stored_snapshot: dict):
        """Calculate changes compared to previous day."""
        if day - 1 not in self.previous_day_stats:
            return None
            
        prev_stats = self.previous_day_stats[day - 1]
        changes = {}
        
        # Storage changes
        if stored_snapshot and "stored" in prev_stats:
            current_meals = stored_snapshot.get("total_meals", 0)
            prev_meals = prev_stats["stored"].get("total_meals", 0)
            if prev_meals > 0:
                changes["meals_change"] = current_meals - prev_meals
                changes["meals_change_pct"] = ((current_meals - prev_meals) / prev_meals) * 100
        
        # Production changes
        current_prod = current_stats.get("nutrition_produced", 0)
        prev_prod = prev_stats.get("stats", {}).get("nutrition_produced", 0)
        if prev_prod > 0:
            changes["prod_change"] = current_prod - prev_prod
            changes["prod_change_pct"] = ((current_prod - prev_prod) / prev_prod) * 100
            
        return changes

    def _get_progress_bar(self, value: float, max_value: float, length: int = 10) -> str:
        """Create a simple progress bar visualization."""
        if max_value <= 0:
            return "[??????????]"
        filled = int((value / max_value) * length)
        filled = min(filled, length)
        return "[" + "■" * filled + "□" * (length - filled) + "]"

    def print_day(self, day: int, stored_snapshot: dict | None, colonist_snapshot: dict | None):
        """
        Print summary for a single in-game day with improved formatting.
        """
        stats = self.days.get(day) or {
            "nutrition_consumed": 0.0,
            "nutrition_produced": 0.0,
            "consumed_by_food": defaultdict(lambda: {"count": 0, "nutrition": 0.0}),
            "produced_by_food": defaultdict(lambda: {"count": 0, "nutrition": 0.0}),
        }

        consumed = stats["nutrition_consumed"]
        produced = stats["nutrition_produced"]
        net = produced - consumed

        # Store current state for next day comparison
        self.previous_day_stats[day] = {
            "stats": stats,
            "stored": stored_snapshot,
            "colonists": colonist_snapshot
        }

        # Calculate comparisons
        changes = self._get_day_comparison(day, stats, stored_snapshot)

        print(f"\n{'='*10} Day {day} {'='*10}")
        print("📊 DAILY SUMMARY")

        # Balance status with emoji
        if net > 0:
            balance_status = f"🟢 SURPLUS: +{net:.2f}"
        elif net < 0:
            balance_status = f"🔴 DEFICIT: {net:.2f}"
        else:
            balance_status = "🟡 BALANCED"

        print(f"├── Balance: {balance_status}")
        
        if changes and changes.get("prod_change") is not None:
            change_emoji = "📈" if changes["prod_change"] > 0 else "📉"
            print(f"├── Production Trend: {change_emoji} {changes['prod_change']:+.1f} ({changes['prod_change_pct']:+.1f}%)")

        # Core metrics in compact format
        print(f"├── Consumption: {consumed:.1f} nutrition ({stats['consumed_by_food'].get('MealSimple', {}).get('count', 0)} meals)")
        print(f"├── Production:  {produced:.1f} nutrition ({stats['produced_by_food'].get('MealSimple', {}).get('count', 0)} meals)")

        # Stored meals section
        if stored_snapshot is not None:
            total_meals = stored_snapshot.get("total_meals", 0)
            total_nutrition_est = stored_snapshot.get("total_nutrition_est", 0.0)
            
            print(f"├── 📦 STORED: {total_meals} meals ({total_nutrition_est:.1f} nutrition)")
            
            if changes and changes.get("meals_change") is not None:
                change_sign = "+" if changes["meals_change"] > 0 else ""
                print(f"│   └── Change: {change_sign}{changes['meals_change']} meals ({changes['meals_change_pct']:+.1f}%)")

            # Detailed stored meals by type
            by_def = stored_snapshot.get("by_def", {})
            if by_def:
                print(f"│   └── Stored by type:")
                for def_name, info in sorted(by_def.items(), key=lambda kv: kv[1]["count"], reverse=True):
                    label = info.get("example_label", def_name)
                    print(f"│       ├── {def_name}: {info['count']} meals ({info['stacks']} stacks)")

        # Colonist status
        if colonist_snapshot is not None:
            colonist_count = colonist_snapshot.get("count")
            avg_hunger = colonist_snapshot.get("avg_hunger")
            hungry_count = colonist_snapshot.get("hungry_count", 0)
            starving_count = colonist_snapshot.get("starving_count", 0)
            
            print(f"├── 👥 COLONISTS: {colonist_count} total")
            
            if avg_hunger is not None:
                hunger_bar = self._get_progress_bar(avg_hunger, 1.0, 8)
                hunger_status = "🟢" if avg_hunger > 0.7 else "🟡" if avg_hunger > 0.4 else "🔴"
                print(f"│   ├── Avg Hunger: {hunger_bar} {avg_hunger:.2f} {hunger_status}")
            
            if hungry_count > 0 or starving_count > 0:
                print(f"│   └── Hunger Alert: {hungry_count} hungry, {starving_count} starving")

        # Food security analysis
        recent_cons = self.get_recent_consumption_avg(days_back=3)
        recent_prod = self.get_recent_production_avg(days_back=3)
        baseline_demand = 0.9 * colonist_count if colonist_count else None

        print(f"└── 🔮 FOOD SECURITY")

        if stored_snapshot and recent_cons and recent_cons > 0:
            total_nutrition_est = stored_snapshot.get("total_nutrition_est", 0.0)
            days_left = total_nutrition_est / recent_cons
            
            # Progress bar for food reserves
            reserve_bar = self._get_progress_bar(days_left, 10.0)  # 10 days = full
            reserve_status = "🟢" if days_left > 7 else "🟡" if days_left > 3 else "🔴"
            
            print(f"    ├── Reserves: {reserve_bar} {days_left:.1f} days {reserve_status}")

        # Production efficiency
        if recent_prod and recent_cons:
            efficiency = (recent_prod / recent_cons) * 100 if recent_cons > 0 else 0
            efficiency_status = "🟢" if efficiency > 120 else "🟡" if efficiency > 80 else "🔴"
            print(f"    ├── Production: {efficiency:.0f}% of needs {efficiency_status}")

        # Baseline demand comparison
        if baseline_demand and recent_cons:
            consumption_ratio = (recent_cons / baseline_demand) * 100 if baseline_demand > 0 else 0
            consumption_status = "🟢" if consumption_ratio > 90 else "🟡" if consumption_ratio > 70 else "🔴"
            print(f"    ├── Consumption: {consumption_ratio:.0f}% of baseline {consumption_status}")

        # Critical alerts
        alerts = []
        if stored_snapshot and recent_cons and recent_cons > 0:
            days_left = stored_snapshot.get("total_nutrition_est", 0.0) / recent_cons
            if days_left < 2:
                alerts.append("🚨 CRITICAL: Less than 2 days of food")
            elif days_left < 5:
                alerts.append("⚠️  WARNING: Less than 5 days of food")
                
        if colonist_snapshot and colonist_snapshot.get("starving_count", 0) > 0:
            alerts.append("🚨 COLONISTS STARVING")
            
        if recent_prod and recent_cons and recent_prod < recent_cons * 0.8:
            alerts.append("⚠️  Production below demand")

        # Check if consumption is significantly below baseline with hungry colonists
        if (baseline_demand and recent_cons and recent_cons < baseline_demand * 0.8 and 
            colonist_snapshot and colonist_snapshot.get("hungry_count", 0) > 0):
            alerts.append("⚠️  Under-reporting: Low consumption but colonists hungry")

        if alerts:
            print(f"    └── ALERTS:")
            for alert in alerts:
                print(f"        {alert}")

        # Food type breakdown (only if we have data)
        if stats["consumed_by_food"] or stats["produced_by_food"]:
            print(f"\n🍽️  FOOD BREAKDOWN")
            
            if stats["consumed_by_food"]:
                print("    Consumed:")
                for def_name, info in sorted(stats["consumed_by_food"].items(), 
                                           key=lambda kv: kv[1]["nutrition"], reverse=True):
                    print(f"    ├── {def_name}: {info['nutrition']:.1f} (x{info['count']})")
                    
            if stats["produced_by_food"]:
                print("    Produced:")
                for def_name, info in sorted(stats["produced_by_food"].items(), 
                                           key=lambda kv: kv[1]["nutrition"], reverse=True):
                    print(f"    └── {def_name}: {info['nutrition']:.1f} (x{info['count']})")

        # Detailed analysis section (preserving original calculations)
        print(f"\n📈 DETAILED ANALYSIS")
        
        # Recent averages
        if recent_cons is not None:
            print(f"    Recent avg consumption: {recent_cons:.2f} nutrition/day")
        if recent_prod is not None:
            print(f"    Recent avg production:  {recent_prod:.2f} nutrition/day")
        
        # Baseline demand
        if baseline_demand is not None:
            print(f"    Baseline demand (0.9 x {colonist_count}): {baseline_demand:.2f} nutrition/day")
            
            # Balance calculation
            if recent_prod is not None:
                balance = recent_prod - baseline_demand
                balance_status = "🟢 Meeting demand" if balance >= 0 else "🔴 Below demand"
                print(f"    Production vs demand: {balance:+.2f} {balance_status}")
                
                if balance < 0:
                    print(f"    Shortfall: {-balance:.2f} nutrition/day needed")
        
        # Meal production targets
        if baseline_demand is not None:
            main_meal_def = "MealSimple"
            avg_meal_nut = self.get_avg_nutrition_for_def(main_meal_def)
            meals_per_day_needed = baseline_demand / avg_meal_nut
            print(f"    Required: {meals_per_day_needed:.1f} {main_meal_def}/day")

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


def fetch_colonist_hunger(timeout=5.0):
    """
    Use /colonists?fields=id,age,hunger to get hunger stats.
    """
    try:
        params = {"fields": "id,age,hunger"}
        resp = requests.get(COLONISTS_URL, params=params, timeout=timeout)
        resp.raise_for_status()
        colonists = resp.json() or []

        count = len(colonists)
        if count == 0:
            return {
                "count": 0,
                "avg_hunger": None,
                "min_hunger": None,
                "max_hunger": None,
                "starving_count": 0,
                "hungry_count": 0,
            }

        hunger_vals = [c.get("hunger") for c in colonists if c.get("hunger") is not None]
        if not hunger_vals:
            return {
                "count": count,
                "avg_hunger": None,
                "min_hunger": None,
                "max_hunger": None,
                "starving_count": 0,
                "hungry_count": 0,
            }

        avg_hunger = sum(hunger_vals) / len(hunger_vals)
        min_hunger = min(hunger_vals)
        max_hunger = max(hunger_vals)

        starving_threshold = 0.15
        hungry_threshold = 0.5

        starving_count = sum(1 for h in hunger_vals if h is not None and h < starving_threshold)
        hungry_count = sum(1 for h in hunger_vals if h is not None and h < hungry_threshold)

        return {
            "count": count,
            "avg_hunger": avg_hunger,
            "min_hunger": min_hunger,
            "max_hunger": max_hunger,
            "starving_count": starving_count,
            "hungry_count": hungry_count,
        }

    except Exception as e:
        print(f"[WARN] Failed to fetch colonist hunger: {e}", file=sys.stderr)
        return None


# --- SSE client ---

def sse_client(url: str):
    """
    Minimal SSE client.
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


# --- Main loop ---

def main():
    stats = FoodStats()
    last_day_seen = None

    while True:
        print(f"Connecting to SSE at {SSE_URL} ...")
        try:
            for event_type, data in sse_client(SSE_URL):
                if event_type == "colonist_ate":
                    stats.handle_colonist_ate(data)
                elif event_type == "make_recipe_product":
                    stats.handle_make_recipe_product(data)
                elif event_type == "date_changed":
                    ticks = data.get("ticksGame") or data.get("ticks") or 0
                    current_day = ticks // TICKS_PER_DAY

                    if last_day_seen is None:
                        previous_day = current_day - 1
                        if previous_day >= 0:
                            stored_items = fetch_stored_items(map_id=0, category="food_meals")
                            stored_summary = stats.build_stored_summary(stored_items)
                            colonist_snapshot = fetch_colonist_hunger()
                            stats.print_day(previous_day, stored_summary, colonist_snapshot)
                        last_day_seen = current_day
                    else:
                        if current_day > last_day_seen:
                            previous_day = last_day_seen
                            stored_items = fetch_stored_items(map_id=0, category="food_meals")
                            stored_summary = stats.build_stored_summary(stored_items)
                            colonist_snapshot = fetch_colonist_hunger()
                            stats.print_day(previous_day, stored_summary, colonist_snapshot)
                            last_day_seen = current_day

                time.sleep(0.01)

            print("[INFO] SSE stream ended (server closed connection). Reconnecting in 3s...")
            time.sleep(3)

        except KeyboardInterrupt:
            print("\nInterrupted by user. Printing full summary...")
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