namespace MornyMorse;

public class RelativeValueRowControl : Control
{
    private List<(string, double)> stringTimes = new();

    private int barHeight = 20; // Height of the colored bar
    private double maxValue = 0.0;

    // Constructor to initialize values
    public RelativeValueRowControl()
    {
        this.Resize += (sender, e) => Refresh(); // Refresh the control on resize
        this.DoubleBuffered = true;
    }

    public void SetValues(Dictionary<string, RollingAverage> values)
    {
        stringTimes = values
            .Select(kvp => (kvp.Key, kvp.Value.GetAverage()))  // Convert Dictionary entry to (char, double) tuple
            .OrderByDescending(pair => pair.Item2)  // Sort by the average value (Item2)
            .ToList();

        int totalItems = values.Count;
        int rows = (totalItems + 1) / 2; // Number of rows needed for two columns
        this.Height = rows * (barHeight + 5); // Adjusting control height based on rows

        Refresh();
    }

    // Overriding the OnPaint method to handle custom drawing
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;

        maxValue = 0.0;
        foreach (var (s, value) in stringTimes)
        {
            maxValue = Math.Max(maxValue, value);
        }

        int inStep = 45;
        var sz = g.MeasureString("ABCDEF: 1234", this.Font);
        var szNum = g.MeasureString("ABCDEF:", this.Font);
        inStep = (int)sz.Width + 12;
        barHeight = (int)sz.Height;
        int gap = 2;

        float ratio = (this.Width * 2) / this.Height;
        int cols = (int)(Math.Min(Math.Max(ratio, 1), 5));
        cols = 2;
        // Divide into two columns
        int columnWidth = (this.Width - (gap * 3)) / cols;
        int baseWidth = columnWidth - inStep - gap;

        // Calculate the total number of rows (half the items, rounded up)
        int totalItems = stringTimes.Count;
        int rows = (totalItems + 1) / cols;

        int i = 0;

        foreach (var (key, value) in stringTimes)
        {
            // Calculate row and column based on the item's index
            int rowIndex = i % rows;         // Wrap to determine row
            int colIndex = i / rows;        // Determine column based on index and rows

            // Calculate positions
            int xOffset = colIndex * (columnWidth + gap); // Column offset
            int yOffset = (rowIndex * (barHeight + gap)) + gap;

            // Calculate bar width proportional to the max value
            int barWidth = maxValue == 0 ? baseWidth : (int)((value / maxValue) * baseWidth);

            // Draw the character on the left side
            g.DrawString($"{key.ToString().Trim('|')}", this.Font, Brushes.Black, 10 + xOffset, yOffset);
            g.DrawString($"{value:F2}", this.Font, Brushes.Black, 10 + xOffset + szNum.Width, yOffset);

            // Draw the background bar
            g.FillRectangle(new SolidBrush(this.BackColor), xOffset + inStep, yOffset, baseWidth, barHeight);

            // Draw the colored bar
            g.FillRectangle(new SolidBrush(GetBarColor(value)), xOffset + inStep, yOffset, barWidth, barHeight);

            i++;
        }
    }

    // This method returns a color based on the value (can customize further)
    private Color GetBarColor(double value)
    {
        if (maxValue == 0 || value <= 0.0)
        {
            return this.BackColor;
        }
        // Example: Color scale from red (low) to green (high)
        int green = (int)(255 * (1 - (value / maxValue))); // Red component decreases as value increases
        int red = (int)(255 * (value / maxValue)); // Green component increases as value increases
        return Color.FromArgb(red, green, 0); // RGB color (red, green, 0)
    }
}
