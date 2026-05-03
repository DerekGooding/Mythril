import unittest
import sys
import os
import json
from unittest.mock import patch, MagicMock

# Add the project root to sys.path
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), "../../..")))

from modules.visualization import template_engine
from modules.visualization import orchestrator

class TestIntegration(unittest.TestCase):
    def test_template_generation(self):
        nodes = [{"id": "n1", "name": "Node 1", "x": 0, "y": 0, "tier": 0}]
        clusters = {"c1": "Cluster 1"}
        js_code = "console.log('test');"
        
        html = template_engine.generate_full_html(nodes, clusters, js_code)
        
        self.assertIn("Node 1", html)
        self.assertIn("Cluster 1", html)
        self.assertIn("console.log('test');", html)
        self.assertIn("<!DOCTYPE html>", html)

    @patch("modules.visualization.data_processor.load_graph")
    @patch("modules.visualization.data_processor.parse_simulation_report")
    @patch("builtins.open", new_callable=MagicMock)
    @patch("os.path.exists")
    @patch("os.makedirs")
    def test_orchestrator_run(self, mock_makedirs, mock_exists, mock_open, mock_parse, mock_load):
        mock_load.return_value = [{"id": "q1", "name": "Q1", "type": "Quest"}]
        mock_parse.return_value = {"sustainable": set(), "unsustainable": set(), "rates": {}}
        mock_exists.return_value = True
        
        # Mock reading JS files
        mock_file = MagicMock()
        mock_file.read.return_value = "var x = 1;"
        mock_open.return_value.__enter__.return_value = mock_file
        
        # Run orchestrator in no_serve mode
        orchestrator.run_visualization(no_serve=True)
        
        # Verify that it tried to write the output file
        # The first few opens are for JS files, the last one is for the output HTML
        write_calls = [call for call in mock_open.call_args_list if call[1].get('mode') == 'w' or (len(call[0]) > 1 and call[0][1] == 'w')]
        self.assertTrue(len(write_calls) >= 1)

if __name__ == "__main__":
    unittest.main()
