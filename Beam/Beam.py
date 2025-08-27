"""
Author: AKHITHA S MANJITHA
Date: 27 Feb 2025
"""

import asyncio
import websockets
import time
import json
from eyeware.client import TrackerClient  # Import Eyeware Beam SDK

# Initialize the eye tracker client
tracker = TrackerClient()

async def send_gaze_data(websocket, path):
    """Continuously send gaze data to Unity at 30 FPS."""
    print("✅ Unity client connected for eye tracking!")

    try:
        while True:
            if tracker.connected:
                screen_gaze = tracker.get_screen_gaze_info()
                if not screen_gaze.is_lost:
                    gaze_data = {
                        "x": (screen_gaze.x / 2560) * 1920 ,
                        "y": (screen_gaze.y / 1440) * 1080
                    }
                else:
                    gaze_data = {"error": "Gaze tracking lost"}
            else:
                gaze_data = {"error": "Tracker not connected"}

            # Convert to proper JSON string format
            json_data = json.dumps(gaze_data)

            # Send data as a JSON string
            await websocket.send(json_data)
            print(f"📩 Sent gaze data: {json_data}")

            # Maintain 30 FPS (1/30 seconds per frame)
            await asyncio.sleep(1 / 30)

    except websockets.exceptions.ConnectionClosed:
        print("❌ Connection closed by Unity")

    finally:
        print("🔌 Client disconnected")

async def main():
    """Start the WebSocket server."""
    server = await websockets.serve(send_gaze_data, "localhost", 4300)
    print("🚀 Eye Gaze WebSocket Server started at ws://localhost:4300")
    await server.wait_closed()


loop = asyncio.get_event_loop()
loop.run_until_complete(main())
loop.run_forever()
