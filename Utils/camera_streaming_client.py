import socket
import cv2
import numpy as np
import struct
import time

class RimWorldStreamReceiver:
    def __init__(self, port=5001):
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.socket.bind(('0.0.0.0', port))
        self.socket.settimeout(1.0)  # 1 second timeout
        self.buffer = {}
        self.last_frame_time = time.time()
        self.packet_count = 0
        print(f"🎥 Listening for RimWorld stream on port {port}...")
        print("⏳ Waiting for data from RimWorld...")
    
    def receive_stream(self):
        while True:
            try:
                data, addr = self.socket.recvfrom(65535)
                self.packet_count += 1
                self.process_packet(data)
                
            except socket.timeout:
                continue
            except Exception as e:
                print(f"❌ Error: {e}")
                continue
            
            # Display frame if available
            self.try_display_frame()
            
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break
    
    def process_packet(self, data):
        
        if len(data) < 9:  # Minimum: 3 header + 4 length + 2 chunk info
            print("❌ Packet too small, ignoring")
            return
            
        header = data[:3]
        if header != b'CAM':
            print("❌ Invalid header, ignoring")
            return
            
        # Parse packet info
        data_length = struct.unpack('<I', data[3:7])[0]
        chunk_index = data[7]
        total_chunks = data[8]
        
        # Calculate expected packet size
        header_size = 9  # 3 header + 4 length + 2 chunk info
        expected_packet_size = header_size + data_length
        
        # More lenient validation - allow packets that are at least the expected size
        if len(data) < expected_packet_size:
            print(f"❌ Packet too small: got {len(data)}, expected at least {expected_packet_size}")
            return
            
        # Take only the expected amount of data (in case there's extra)
        image_data = data[9:9 + data_length]
        
        if total_chunks == 1:
            self.process_complete_frame(image_data)
        else:
            self.buffer_frame_chunk(chunk_index, total_chunks, image_data)

    
    def buffer_frame_chunk(self, chunk_index, total_chunks, chunk_data):
        frame_key = int(time.time() * 1000) // 1000
        
        if frame_key not in self.buffer:
            self.buffer[frame_key] = {
                'chunks': [None] * total_chunks,
                'received_chunks': 0,
                'total_chunks': total_chunks
            }
        
        if self.buffer[frame_key]['chunks'][chunk_index] is None:
            self.buffer[frame_key]['chunks'][chunk_index] = chunk_data
            self.buffer[frame_key]['received_chunks'] += 1
    
    def process_complete_frame(self, image_data):
        frame = cv2.imdecode(np.frombuffer(image_data, np.uint8), cv2.IMREAD_COLOR)
        if frame is not None:
            cv2.imshow('RimWorld Camera Stream', frame)
            self.last_frame_time = time.time()
        else:
            print("❌ Failed to decode JPEG frame")
    
    def try_display_frame(self):
        current_time = time.time()
        complete_frames = []
        
        # Find complete frames
        for frame_key, frame_data in list(self.buffer.items()):
            if frame_data['received_chunks'] == frame_data['total_chunks']:
                complete_image_data = b''.join(frame_data['chunks'])
                complete_frames.append((frame_key, complete_image_data))
        
        # Process complete frames
        if complete_frames:
            complete_frames.sort(key=lambda x: x[0], reverse=True)
            newest_frame_key, newest_frame_data = complete_frames[0]
            
            frame = cv2.imdecode(np.frombuffer(newest_frame_data, np.uint8), cv2.IMREAD_COLOR)
            if frame is not None:
                cv2.imshow('RimWorld Camera Stream', frame)
                self.last_frame_time = current_time
            else:
                print("❌ Failed to decode reassembled frame")
            
            # Clean up
            for frame_key, _ in complete_frames:
                if frame_key in self.buffer:
                    del self.buffer[frame_key]
    
    def clean_old_buffers(self):
        current_time = time.time()
        removed = 0
        for frame_key in list(self.buffer.keys()):
            if current_time - frame_key > 2.0:
                del self.buffer[frame_key]
                removed += 1
        if removed > 0:
            print(f"🧹 Cleaned up {removed} old buffer(s)")

if __name__ == "__main__":
    print("🚀 Starting RimWorld Camera Stream Receiver")
    print("===========================================")
    
    # Try multiple ports
    ports_to_try = [5007]
    
    for port in ports_to_try:
        try:
            print(f"🔌 Trying port {port}...")
            receiver = RimWorldStreamReceiver(port)
            print(f"✅ Successfully bound to port {port}")
            break
        except OSError as e:
            print(f"❌ Port {port} unavailable: {e}")
            continue
    else:
        print("💥 No available ports found!")
        exit(1)
    
    print("\n🎮 Now go to RimWorld and:")
    print("   1. Load your game or be in main menu")
    print("   2. Make sure the Camera Streamer mod is enabled")
    print("   3. Click the 'Start Camera Stream' button")
    print("   4. You should see packets received here\n")
    
    try:
        receiver.receive_stream()
    except KeyboardInterrupt:
        print("\n🛑 Stopped by user")
    except Exception as e:
        print(f"💥 Fatal error: {e}")
    finally:
        receiver.close()
        print("👋 Receiver closed")