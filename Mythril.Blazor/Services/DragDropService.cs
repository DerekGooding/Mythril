using Mythril.Data;

namespace Mythril.Blazor.Services;

public class DragDropService
{
    public object? Data { get; set; }
    public (Character character, Stat stat)? HoveredTarget { get; set; }

    public event Action? OnHoverChanged;

    public void SetHoveredTarget(Character? character, Stat? stat)
    {
        if (character == null || stat == null)
        {
            if (HoveredTarget != null)
            {
                HoveredTarget = null;
                OnHoverChanged?.Invoke();
            }
        }
        else
        {
            var target = (character.Value, stat.Value);
            if (HoveredTarget?.Equals(target) != true)
            {
                HoveredTarget = target;
                OnHoverChanged?.Invoke();
            }
        }
    }

    public void ClearHoveredTarget()
    {
        if (HoveredTarget != null)
        {
            HoveredTarget = null;
            OnHoverChanged?.Invoke();
        }
    }
}