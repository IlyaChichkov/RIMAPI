#!/usr/bin/env python3
"""
SSE Client for RimWorld REST API - Filtered Events Only
"""

import requests
import json
import time
import sys

# Configuration - Edit these to filter events
FILTERED_EVENTS = {
    'colonist_ate': True,      # Food consumption events
    'gameUpdate': False,       # Regular game updates
    'connected': False,        # Connection established
    'gameState': False,        # Initial game state
    'heartbeat': False,        # Heartbeat messages
    'research': False,         # Research events
    'test': False,             # Test events
}

def rimworld_sse_client():
    """SSE client that only shows filtered events"""
    url = "http://localhost:8765/api/v1/events"
    
    print(f"🔗 Connecting to: {url}")
    print("🎯 Filtering events - only showing:")
    for event_type, enabled in FILTERED_EVENTS.items():
        if enabled:
            print(f"   ✅ {event_type}")
    print("=" * 50)
    
    headers = {
        'Accept': 'text/event-stream',
        'Cache-Control': 'no-cache',
        'User-Agent': 'RimWorld-SSE-Test/1.0'
    }
    
    try:
        response = requests.get(url, stream=True, timeout=30, headers=headers)
        
        if response.status_code != 200:
            print(f"❌ SSE endpoint returned: {response.status_code}")
            return
        
        print("✅ Connected! Waiting for filtered events...")
        print("💡 Make colonists eat food to test 'colonist_ate' events")
        print("=" * 50)
        
        buffer = ""
        total_messages = 0
        filtered_messages = 0
        
        for line in response.iter_lines(decode_unicode=True):
            if line is None:
                continue
                
            if line.strip() == "":  # Empty line = end of message
                if buffer.strip():
                    total_messages += 1
                    if process_and_filter_message(buffer):
                        filtered_messages += 1
                    buffer = ""
                continue
                
            buffer += line + "\n"
            
    except requests.exceptions.ConnectTimeout:
        print("❌ Connection timeout")
    except requests.exceptions.ConnectionError:
        print("❌ Connection refused")
    except KeyboardInterrupt:
        print(f"\n👋 Disconnected - Received {total_messages} total messages, displayed {filtered_messages}")
    except Exception as e:
        print(f"💥 Error: {e}")

def process_and_filter_message(message):
    """Process message and only display if it matches our filters"""
    lines = [line.strip() for line in message.strip().split('\n') if line.strip()]
    
    event_type = "message"
    data_content = ""
    
    for line in lines:
        if line.startswith('event:'):
            event_type = line[6:].strip()
        elif line.startswith('data:'):
            data_content = line[5:].strip()
    
    # Check if this event type should be displayed
    if event_type in FILTERED_EVENTS and FILTERED_EVENTS[event_type]:
        print(f"\n🎉 NEW {event_type.upper()} EVENT:")
        print("=" * 50)
        
        # Parse and display all message details
        for line in lines:
            if line.startswith('event:'):
                print(f"🎪 Event Type: {line[6:].strip()}")
            elif line.startswith('id:'):
                print(f"🆔 ID: {line[3:].strip()}")
            elif line.startswith('retry:'):
                print(f"🔁 Retry: {line[6:].strip()}")
            elif line.startswith(':'):
                print(f"💬 Comment: {line[1:].strip()}")
        
        if data_content:
            try:
                data_obj = json.loads(data_content)
                print("📊 Data:")
                print(json.dumps(data_obj, indent=2, ensure_ascii=False))
            except json.JSONDecodeError:
                print(f"📝 Data (raw): {data_content}")
        
        print("=" * 50)
        return True
    
    # Event is filtered out - show brief info if verbose mode
    if '-v' in sys.argv or '--verbose' in sys.argv:
        print(f"🔇 Filtered out: {event_type}")
    
    return False

def show_config():
    """Show current filter configuration"""
    print("🔧 Current Event Filters:")
    for event_type, enabled in FILTERED_EVENTS.items():
        status = "✅" if enabled else "❌"
        print(f"   {status} {event_type}")

def update_filter(event_type, enabled):
    """Update filter configuration"""
    if event_type in FILTERED_EVENTS:
        FILTERED_EVENTS[event_type] = enabled
        status = "enabled" if enabled else "disabled"
        print(f"🔧 {event_type} {status}")
    else:
        print(f"❌ Unknown event type: {event_type}")

if __name__ == "__main__":
    # Handle command line arguments
    if len(sys.argv) > 1:
        if sys.argv[1] == "config":
            show_config()
        elif sys.argv[1] == "enable" and len(sys.argv) > 2:
            update_filter(sys.argv[2], True)
        elif sys.argv[1] == "disable" and len(sys.argv) > 2:
            update_filter(sys.argv[2], False)
        elif sys.argv[1] == "all":
            for event_type in FILTERED_EVENTS:
                FILTERED_EVENTS[event_type] = True
            print("✅ All events enabled")
        elif sys.argv[1] == "none":
            for event_type in FILTERED_EVENTS:
                FILTERED_EVENTS[event_type] = False
            print("✅ All events disabled")
        elif sys.argv[1] in ["help", "-h", "--help"]:
            print("""
🚀 RimWorld SSE Client - Filtered Events

Usage:
  python sse_client.py                    # Run with current filters
  python sse_client.py config            # Show current filters
  python sse_client.py enable <event>    # Enable specific event
  python sse_client.py disable <event>   # Disable specific event
  python sse_client.py all               # Enable all events
  python sse_client.py none              # Disable all events
  python sse_client.py -v                # Verbose mode (show filtered events)

Available events: colonist_ate, gameUpdate, connected, gameState, heartbeat, research, test
            """)
            sys.exit(0)
    
    print("🚀 RimWorld SSE Client - Filtered Events")
    show_config()
    print("=" * 50)
    
    rimworld_sse_client()