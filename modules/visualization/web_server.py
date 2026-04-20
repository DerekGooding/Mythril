import http.server
import socketserver
import threading
import webbrowser
import time
import os

def start_server(port):
    handler = http.server.SimpleHTTPRequestHandler
    handler.log_message = lambda *args: None
    try:
        with socketserver.TCPServer(("", port), handler) as httpd:
            print(f"Server started at http://localhost:{port}")
            httpd.serve_forever()
    except Exception as e:
        print(f"Server error: {e}")

def run_visualizer_server(output_dir, output_file, port=8000):
    original_dir = os.getcwd()
    os.chdir(output_dir)
    
    server_thread = threading.Thread(target=start_server, args=(port,), daemon=True)
    server_thread.start()
    
    time.sleep(1)
    url = f"http://localhost:{port}/{output_file}"
    print(f"Opening {url}...")
    webbrowser.open(url)
    
    print("\nVisualizer is running. Press Ctrl+C to stop the server.")
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("\nStopping server...")
        os.chdir(original_dir)
