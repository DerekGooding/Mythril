using Myra.Graphics2D.UI;
using Mythril.GameLogic;

namespace Mythril.UI
{
    public class JobScreen : Dialog
    {
        public JobScreen(ResourceManager resourceManager)
        {
            Title = "Jobs";

            var scrollViewer = new ScrollViewer();
            var grid = new Grid { RowSpacing = 8, ColumnSpacing = 8 };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));

            int row = 0;
            foreach (var job in resourceManager.Jobs)
            {
                grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
                var nameLabel = new Label { Text = job.Name };
                Grid.SetRow(nameLabel, row);
                Grid.SetColumn(nameLabel, 0);

                var descriptionLabel = new Label { Text = job.Description };
                Grid.SetRow(descriptionLabel, row);
                Grid.SetColumn(descriptionLabel, 1);

                grid.Widgets.Add(nameLabel);
                grid.Widgets.Add(descriptionLabel);
                row++;
            }

            scrollViewer.Content = grid;

            var mainPanel = new VerticalStackPanel();
            mainPanel.Widgets.Add(scrollViewer);

            var closeButton = new Button { Content = new Label { Text = "Close" }, HorizontalAlignment = HorizontalAlignment.Center };
            closeButton.Click += (s, a) => Close();
            mainPanel.Widgets.Add(closeButton);

            Content = mainPanel;
        }
    }
}
