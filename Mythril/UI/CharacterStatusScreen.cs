using Myra.Graphics2D.UI;
using Mythril.Data;

namespace Mythril.UI;

public class CharacterStatusScreen : Dialog
{
    public CharacterStatusScreen(Character character)
    {
        Title = character.Name;

        var grid = new Grid { RowSpacing = 8, ColumnSpacing = 8 };

        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));

        // Name
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        var nameLabel = new Label { Text = "Name:" };
        Grid.SetRow(nameLabel, 0);
        Grid.SetColumn(nameLabel, 0);
        var nameValue = new Label { Text = character.Name };
        Grid.SetRow(nameValue, 0);
        Grid.SetColumn(nameValue, 1);
        grid.Widgets.Add(nameLabel);
        grid.Widgets.Add(nameValue);

        // Job
        grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
        var jobLabel = new Label { Text = "Job:" };
        Grid.SetRow(jobLabel, 1);
        Grid.SetColumn(jobLabel, 0);
        var jobValue = new Label { Text = character.Job?.Name };
        Grid.SetRow(jobValue, 1);
        Grid.SetColumn(jobValue, 1);
        grid.Widgets.Add(jobLabel);
        grid.Widgets.Add(jobValue);

        var mainPanel = new VerticalStackPanel();
        mainPanel.Widgets.Add(grid);

        var closeButton = new Button { Content = new Label { Text = "Close" }, HorizontalAlignment = HorizontalAlignment.Center };
        closeButton.Click += (s, a) => Close();
        mainPanel.Widgets.Add(closeButton);

        Content = mainPanel;
    }
}
