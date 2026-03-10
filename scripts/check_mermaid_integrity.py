import re
import os

def check_integrity(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Extract mermaid section
    match = re.search(r'<div class="mermaid">\s*(.*?)\s*</div>', content, re.DOTALL)
    if not match:
        print("Could not find mermaid section in HTML.")
        return
    
    mermaid_code = match.group(1)
    lines = mermaid_code.split('\n')
    
    defined_nodes = set()
    referenced_nodes = set()
    
    # Simple regex for node definition: id["Label"]:::style or id["Label"] or id
    # Note: id can be anything alphanumeric + underscores in this project
    node_def_re = re.compile(r'^(\w+)\[".*?"\](:::(\w+))?')
    # Simple regex for edges: id1 -- edge -- id2
    edge_re = re.compile(r'(\w+)\s*([-=.][->ox]+|--o|--x|==>|-.->)\s*(\|".*?"\|)?\s*(\w+)')
    
    for line in lines:
        line = line.strip()
        if not line or line.startswith('graph') or line.startswith('classDef') or line.startswith('subgraph') or line.startswith('end'):
            continue
            
        m = node_def_re.match(line)
        if m:
            defined_nodes.add(m.group(1))
            continue
            
        m = edge_re.search(line)
        if m:
            referenced_nodes.add(m.group(1))
            referenced_nodes.add(m.group(4))
    
    missing = referenced_nodes - defined_nodes
    if missing:
        print(f"Found {len(missing)} referenced nodes that are NOT defined:")
        for m in sorted(missing):
            # Some nodes might be "stat_*" which are defined later or implicitly
            # Let's check if they are defined anywhere in the file
            if f'{m}["' in mermaid_code or f'{m}:::' in mermaid_code:
                continue
            print(f"  - {m}")
    else:
        print("All referenced nodes are defined.")

if __name__ == "__main__":
    check_integrity("output/graph_visualizer_test.html")
