import unittest
import sys
import os
from unittest.mock import patch, MagicMock

# Add the project root to sys.path so we can import modules
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), "../../..")))

from modules.visualization import data_processor

class TestDataProcessor(unittest.TestCase):
    def test_matches_activity(self):
        # Basic exact match
        self.assertTrue(data_processor._matches_activity("Refine Wood - Herb", ["Refine Wood - Herb"]))
        # Normalized match (stripping spaces, dashes, colons, arrows)
        # "Refine Wood - Herb" -> "refinewoodherb"
        # "Refine Wood:Herb" -> "refinewoodherb"
        self.assertTrue(data_processor._matches_activity("Refine Wood - Herb", ["Refine Wood:Herb"]))
        self.assertFalse(data_processor._matches_activity("Refine Wood - Herb", ["Refine Stone"]))

    @patch("modules.visualization.data_processor.parse_simulation_report")
    def test_bfs_tiering_complex(self, mock_parse):
        mock_parse.return_value = {"sustainable": set(), "unsustainable": set(), "rates": {}}
        
        # Linear dependency
        nodes = [
            {"id": "q1", "name": "Q1", "type": "Quest", "in_edges": {}},
            {"id": "q2", "name": "Q2", "type": "Quest", "in_edges": {"requires_quest": ["q1"]}},
            {"id": "q3", "name": "Q3", "type": "Quest", "in_edges": {"requires_quest": ["q2"]}},
        ]
        
        enriched, _ = data_processor.enrich_data(nodes)
        node_tiers = {n["id"]: n["tier"] for n in enriched}
        
        self.assertEqual(node_tiers["q1"], 0)
        self.assertEqual(node_tiers["q2"], 1)
        self.assertEqual(node_tiers["q3"], 2)

    @patch("modules.visualization.data_processor.parse_simulation_report")
    def test_sustainable_node_splitting(self, mock_parse):
        # Mock simulation saying "Wood" and "Sustainable Quest" are sustainable
        mock_parse.return_value = {
            "sustainable": {"Wood", "Sustainable Quest"},
            "unsustainable": set(),
            "rates": {}
        }
        
        # We need an initial producer and a much later sustainable producer
        nodes = [
            {"id": "q_init", "name": "Initial Quest", "type": "Quest", "in_edges": {}},
            {"id": "wood", "name": "Wood", "type": "Item"},
            # Bridge to push q_sust much later
            {"id": "q_gap1", "name": "G1", "type": "Quest", "in_edges": {"requires_quest": ["q_init"]}},
            {"id": "q_gap2", "name": "G2", "type": "Quest", "in_edges": {"requires_quest": ["q_gap1"]}},
            {"id": "q_sust", "name": "Sustainable Quest", "type": "Quest", "in_edges": {"requires_quest": ["q_gap2"]}}
        ]
        
        # Add edges
        nodes[0]["out_edges"] = {"rewards": [{"targetId": "wood"}]}
        nodes[4]["out_edges"] = {"rewards": [{"targetId": "wood"}]}
        
        enriched, _ = data_processor.enrich_data(nodes)
        
        # Check if we have a sustainable instance
        sust_node = next((n for n in enriched if n.get("is_sustainable_instance")), None)
        self.assertIsNotNone(sust_node)
        self.assertEqual(sust_node["original_id"], "wood")
        self.assertEqual(sust_node["name"], "Wood (Sustainable)")
        # q_sust tier should be 3. sust_node tier should be 3 + 1 = 4.
        self.assertEqual(sust_node["tier"], 4)

    @patch("modules.visualization.data_processor.parse_simulation_report")
    def test_clustering(self, mock_parse):
        mock_parse.return_value = {"sustainable": set(), "unsustainable": set(), "rates": {}}
        
        nodes = [
            {"id": "loc1", "name": "Forest", "type": "Location", "out_edges": {"contains": [{"targetId": "quest1"}]}},
            {"id": "quest1", "name": "Chop Wood", "type": "Quest", "in_edges": {}}
        ]
        
        enriched, clusters = data_processor.enrich_data(nodes)
        
        quest1 = next(n for n in enriched if n["id"] == "quest1")
        self.assertEqual(quest1["cluster_id"], "cluster_loc_loc1")
        self.assertIn("cluster_loc_loc1", clusters)
        self.assertEqual(clusters["cluster_loc_loc1"], "Forest")

if __name__ == "__main__":
    unittest.main()
