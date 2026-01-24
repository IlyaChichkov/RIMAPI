import requests
import json
import time
import sys

# --- CONFIGURATION ---
SSE_URL = "http://localhost:8765/api/v1/events"
RECONNECT_DELAY = 2  # Seconds to wait before reconnecting

def format_json(text):
    """Tries to pretty-print JSON data."""
    try:
        data = json.loads(text)
        return json.dumps(data, indent=4)
    except:
        return text

def listen_to_sse():
    print(f"[-] Connecting to Event Stream at {SSE_URL}...")
    
    session = requests.Session()
    
    while True:
        try:
            # stream=True is critical for SSE to keep the connection open
            with session.get(SSE_URL, stream=True, timeout=None) as response:
                if response.status_code == 200:
                    print("[+] Connected! Waiting for events...\n")
                    
                    # Iterates over lines as they arrive
                    for line in response.iter_lines():
                        if line:
                            decoded_line = line.decode('utf-8')
                            
                            if decoded_line.startswith("event:"):
                                raw_data = decoded_line[6:].strip()
                                print(f"============ EVENT RECEIVED: {raw_data} ============")
                                                  
                            # SSE format usually sends "data: {json}"
                            if decoded_line.startswith("data:"):
                                raw_data = decoded_line[5:].strip() # Remove "data:" prefix
                                
                                # Print Timestamp
                                timestamp = time.strftime("%H:%M:%S")
                                print(f"[{timestamp}] EVENT RECEIVED:")
                                print(format_json(raw_data))
                                print("-" * 40)
                                
                            elif decoded_line.startswith(":"):
                                # This is a heartbeat/comment
                                pass
                else:
                    print(f"[!] Server returned status: {response.status_code}")
                    
        except requests.exceptions.ConnectionError:
            print(f"[!] Connection failed. Retrying in {RECONNECT_DELAY}s...")
        except KeyboardInterrupt:
            print("\n[!] Stopping Debugger.")
            sys.exit(0)
        except Exception as e:
            print(f"[!] Unexpected Error: {e}")
        
        time.sleep(RECONNECT_DELAY)

if __name__ == "__main__":
    listen_to_sse()