namespace Mythril.Data;

public class QuestProgress(Quest quest, Character character)
{
    public Quest Quest { get; set; } = quest;
    public Character Character { get; set; } = character;
    public DateTime StartTime { get; set; } = DateTime.Now;
    public double Progress { get; set; } = 0;
}
