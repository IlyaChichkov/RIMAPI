#!/usr/bin/env python3
import json
import sys
import time
from collections import defaultdict

import requests

SSE_URL = "http://localhost:8765/api/v1/events"  # your SSE endpoint
TICKS_PER_DAY = 60000


class FoodStats:
    def __init__(self):
        self.days = defaultdict(lambda: {
            "nutrition_consumed": 0.0,
            "nutrition_produced": 0.0,  # will stay 0 without C# change
            "consumed_by_food": defaultdict(float),
            "produced_by_food": defaultdict(float),
            "consumed_count_by_food": defaultdict(int),
            "produced_count_by_food": defaultdict(int),
            "produced_items_total": 0,  # 👈 new
        })

    def _day_for_ticks(self, ticks: int) -> int:
        return ticks // TICKS_PER_DAY

    def handle_colonist_ate(self, data: dict):
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
            day_stats["consumed_by_food"][def_name] += nutrition
            day_stats["consumed_count_by_food"][def_name] += 1
        except Exception as e:
            print(f"[WARN] Failed to handle colonist_ate: {e}", file=sys.stderr)

    def handle_make_recipe_product(self, data: dict):
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

                # we don't have nutrition, so treat it as 0
                nutrition = float(item.get("nutrition", 0.0))

                day_stats["nutrition_produced"] += nutrition  # stays 0
                day_stats["produced_by_food"][def_name] += nutrition
                day_stats["produced_count_by_food"][def_name] += 1
                day_stats["produced_items_total"] += 1        # 👈 count items
        except Exception as e:
            print(f"[WARN] Failed to handle make_recipe_product: {e}", file=sys.stderr)

    def print_day(self, day: int):
        stats = self.days.get(day)
        if not stats:
            print(f"\nDay {day}: (no events)")
            return

        print(f"\n====== Day {day} ======")
        print(f"  Total nutrition consumed : {stats['nutrition_consumed']:.2f}")
        print(f"  Total nutrition produced : {stats['nutrition_produced']:.2f}")
        print(f"  Total items produced     : {stats['produced_items_total']}")

        if stats["consumed_by_food"]:
            print("  Consumed by food type:")
            for def_name, nut in sorted(
                stats["consumed_by_food"].items(),
                key=lambda kv: kv[1],
                reverse=True
            ):
                count = stats["consumed_count_by_food"][def_name]
                print(f"    - {def_name}: {nut:.2f} (events: {count})")

        if stats["produced_by_food"]:
            print("  Produced by food type:")
            for def_name, nut in sorted(
                stats["produced_by_food"].items(),
                key=lambda kv: kv[1],
                reverse=True
            ):
                count = stats["produced_count_by_food"][def_name]
                print(f"    - {def_name}: {nut:.2f} (items: {count})")

    def print_summary(self):
        """Print all days collected so far."""
        if not self.days:
            print("No data collected yet.")
            return

        for day in sorted(self.days.keys()):
            self.print_day(day)


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


def main():
    stats = FoodStats()
    last_day_seen = None  # last day index we know we're currently in

    print(f"Connecting to SSE at {SSE_URL} ...")
    try:
        for event_type, data in sse_client(SSE_URL):
            # print(f"[DEBUG] Event: {event_type} data={data}")

            if event_type == "colonist_ate":
                stats.handle_colonist_ate(data)

            elif event_type == "make_recipe_product":
                stats.handle_make_recipe_product(data)

            elif event_type == "date_changed":
                # Your C# date_changed payload (from our earlier patch) has:
                # {
                #   "date": { "dayOfYear": int, ... },
                #   "ticksGame": int,
                #   ...
                # }
                ticks = data.get("ticksGame") or data.get("ticks") or 0
                day = ticks // TICKS_PER_DAY

                # On first date_changed, just initialize
                if last_day_seen is None:
                    last_day_seen = day
                else:
                    # If we entered a new day, print summary for the previous day
                    if day > last_day_seen:
                        previous_day = last_day_seen
                        stats.print_day(previous_day)
                        last_day_seen = day

                stats.print_summary()

            time.sleep(0.01)

    except KeyboardInterrupt:
        print("\nInterrupted by user. Printing full summary...")
        stats.print_summary()
    except requests.RequestException as e:
        print(f"[ERROR] SSE connection error: {e}")
    except Exception as e:
        print(f"[ERROR] Unexpected error: {e}")


if __name__ == "__main__":
    main()
