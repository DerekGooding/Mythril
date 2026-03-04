using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mythril.Data;

namespace Mythril.Headless.Simulation;

public partial class ReachabilitySimulator
{
    private void GenerateReport()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("# Simulation Reachability Report");
        report.AppendLine($"Generated at: {DateTime.Now}");
        report.AppendLine();

        var unreachableQuests = quests.All.Where(q => !_completedQuests.Contains(q.Name)).ToList();
        if (unreachableQuests.Any())
        {
            report.AppendLine("## ❌ Unreachable Content");
            foreach (var q in unreachableQuests)
            {
                var detail = questDetails[q];
                string reason = "Unknown";
                
                // Heuristic for failure reason
                if (detail.RequiredStats != null && detail.RequiredStats.Any(rs => _maxStats[rs.Key] < rs.Value))
                {
                    var failStat = detail.RequiredStats.First(rs => _maxStats[rs.Key] < rs.Value);
                    reason = $"Insufficient {failStat.Key} (Max potential: {_maxStats[failStat.Key]}, Need: {failStat.Value})";
                }
                else if (detail.Requirements.Any(r => !_infiniteResources.Contains(r.Item.Name) && _oneTimeResources.GetValueOrDefault(r.Item.Name, 0) < r.Quantity))
                {
                    var failItem = detail.Requirements.First(r => !_infiniteResources.Contains(r.Item.Name) && _oneTimeResources.GetValueOrDefault(r.Item.Name, 0) < r.Quantity);
                    reason = $"Missing resource: {failItem.Item.Name}";
                }
                else if (questUnlocks[q].Any(rq => !_completedQuests.Contains(rq.Name)))
                {
                    var failQuest = questUnlocks[q].First(rq => !_completedQuests.Contains(rq.Name));
                    reason = $"Prerequisite quest not completed: {failQuest.Name}";
                }

                report.AppendLine($"- **{q.Name}**: {reason}");
            }
        }
        else
        {
            report.AppendLine("## ✅ All Content Reachable");
            report.AppendLine("No orphaned or mathematically impossible quests detected.");
        }

        report.AppendLine();
        report.AppendLine("## ⏱️ Milestone Estimates (Optimal Path)");
        var keyMilestones = new[] { "Prologue", "Visit Starting Town", "Learn About Cadences", "Learn about the Mines", "Learn about the Dark Forest", "Rekindling the Spark" };
        foreach (var m in keyMilestones)
        {
            double time = _minTimeToReachQuest.GetValueOrDefault(m, double.PositiveInfinity);
            string timeStr = double.IsInfinity(time) ? "REACHABLE" : TimeSpan.FromSeconds(time).ToString(@"hh\:mm\:ss");
            report.AppendLine($"- **{m}**: {timeStr}");
        }

        report.AppendLine();
        report.AppendLine("## 📊 Economy Summary");
        report.AppendLine($"**Discovered Infinite Resources**: {string.Join(", ", _infiniteResources)}");
        report.AppendLine($"**Max Stat Potentials**: {string.Join(", ", _maxStats.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
        report.AppendLine($"**Final Magic Capacity**: {_magicCapacity}");

        File.WriteAllText("simulation_report.md", report.ToString());
        Console.WriteLine(unreachableQuests.Any() ? "SIMULATION FAILED: Unreachable content detected." : "SIMULATION PASSED: All content reachable.");
        
        if (unreachableQuests.Any())
        {
            Environment.Exit(1);
        }
    }
}
