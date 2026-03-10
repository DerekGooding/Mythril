import os
import sys
from export_graph_mermaid import generate_mermaid
from visualize_graph import generate_html

def main():
    print("Generating Clustered Mermaid graph HTML only...")
    try:
        mermaid_code = generate_mermaid()
        html = generate_html(mermaid_code)
        
        output_dir = "output"
        if not os.path.exists(output_dir):
            os.makedirs(output_dir)
            
        file_path = os.path.join(output_dir, "graph_visualizer_test.html")
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(html)
            
        abs_path = os.path.abspath(file_path)
        print(f"Graph generated: {abs_path}")
        
        if os.path.exists(file_path) and os.path.getsize(file_path) > 0:
            print("[SUCCESS] HTML file generated and is not empty.")
            sys.exit(0)
        else:
            print("[FAIL] HTML file was not generated or is empty.")
            sys.exit(1)
            
    except Exception as e:
        print(f"Error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()
