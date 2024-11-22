namespace MornyMorse;

public class RollingAverage(int maxSize = 5)
{
    private Queue<double> values = new(); // Queue to store the last N values
    private double sum = 0.0; // Running sum of the values in the queue

    public double Average { get; private set; } = 0.0;

    public void SetCount(int count)
    {
        var val = Average;
        maxSize = count;
        values.Clear();
        sum = 0.0;

        for (int i = 0; i < count; i++)
        {
            AddValue(val);
        }
    }

    // Add a new value and calculate the rolling average
    public double AddValue(double newValue)
    {
        values.Enqueue(newValue);
        sum += newValue;

        if (values.Count > maxSize)
        {
            double removedValue = values.Dequeue(); // Remove the oldest value
            sum -= removedValue;
        }

        Average = sum / values.Count;
        return Average;
    }

    public double GetAverage()
    {
        return Average;
    }

    // Serialization-friendly properties
    public List<double> SerializedValues
    {
        get => values.ToList(); // Convert Queue to List for serialization
        set
        {
            values = new Queue<double>(value); // Rebuild Queue from List
            sum = values.Sum(); // Recalculate the sum
            Average = values.Count > 0 ? sum / values.Count : 0.0;
        }
    }

    public int MaxSize
    {
        get => maxSize; // Expose maxSize for serialization
        set => maxSize = value;
    }
}

