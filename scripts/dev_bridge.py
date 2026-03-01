import json
import os
import subprocess
from http.server import BaseHTTPRequestHandler, HTTPServer

class DevBridgeHandler(BaseHTTPRequestHandler):
    def do_OPTIONS(self):
        self.send_response(200)
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'POST, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        self.end_headers()

    def do_POST(self):
        if self.path == '/report':
            content_length = int(self.headers['Content-Length'])
            post_data = self.rfile.read(content_length)
            
            try:
                # Use the existing sync_feedback script to handle file creation and path printing
                # We wrap the data in a list because sync_feedback expects an array or single object
                data = json.loads(post_data.decode('utf-8'))
                json_str = json.dumps([data])
                
                # Run sync_feedback.py and capture its output
                result = subprocess.run(
                    ['python', 'scripts/sync_feedback.py', json_str],
                    capture_output=True,
                    text=True,
                    encoding='utf-8'
                )
                
                print(result.stdout)
                if result.stderr:
                    print(f"Error from sync script: {result.stderr}")

                self.send_response(200)
                self.send_header('Access-Control-Allow-Origin', '*')
                self.send_header('Content-Type', 'application/json')
                self.end_headers()
                self.wfile.write(json.dumps({"status": "synced"}).encode('utf-8'))
            except Exception as e:
                print(f"Bridge error: {e}")
                self.send_response(500)
                self.end_headers()

def run(server_class=HTTPServer, handler_class=DevBridgeHandler, port=8080):
    server_address = ('', port)
    httpd = server_class(server_address, handler_class)
    print(f"--- Mythril Dev Bridge active on port {port} ---")
    print("Capturing runtime errors and feedback directly into docs/...")
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        pass
    httpd.server_close()

if __name__ == "__main__":
    run()
