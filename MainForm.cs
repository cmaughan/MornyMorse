using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace MornyMorse;

public partial class MainForm : Form
{
    private readonly List<string> testSet = Morse.Letters.ToList();
    private bool loading = true;
    private readonly PlayerTask playerTask = new();
    private readonly Queue<StringFinishTime> pendingUserInputs = new();
    private Dictionary<string, RollingAverage> allStringTimes = new();
    private Dictionary<string, RollingAverage> stringTimes = new();
    private readonly RelativeValueRowControl rowControl = new();
    private string currentInput = string.Empty;

    private Dictionary<string, RollingAverage> GetCurrentTimes() =>
        allStringTimes
            .Where(entry => testSet.Contains(entry.Key))
            .ToDictionary(entry => entry.Key, entry => entry.Value);

    [Serializable]
    public class CheckboxItem
    {
        public string? Name { get; set; }
        public bool IsChecked { get; set; }
    }

    private BindingList<CheckboxItem> checkboxItems =
    [
        new CheckboxItem { Name = "Letters", IsChecked = true },
        new CheckboxItem { Name = "Numbers", IsChecked = true },
        new CheckboxItem { Name = "ProSigns", IsChecked = false },
        new CheckboxItem { Name = "Bigrams", IsChecked = false },
        new CheckboxItem { Name = "Trigrams", IsChecked = false },
        new CheckboxItem { Name = "Words", IsChecked = false }
    ];

    public MainForm()
    {
        InitializeComponent();
        loading = true;

        Load += MainForm_Load;
        FormClosing += MainForm_FormClosing;
        StartPosition = FormStartPosition.CenterScreen;
        KeyDown += MainForm_KeyDown;

        rowControl.Dock = DockStyle.Fill;
        panel1.Controls.Add(rowControl);

        // Check boxes
        {
            testSetChecks.DataSource = checkboxItems;
            testSetChecks.DisplayMember = "Name";
            testSetChecks.ValueMember = "IsChecked";
            testSetChecks.TabStop = false;

            for (int i = 0; i < checkboxItems.Count; i++)
            {
                testSetChecks.SetItemChecked(i, checkboxItems[i].IsChecked);
            }

            testSetChecks.ItemCheck += TestSets_ItemCheck;
            testSetChecks.KeyDown += (object? sender, KeyEventArgs e) => e.Handled = true;
        }

        // See audio devices, just in case
        Debug.WriteLine("Audio Devices:");
        for (int n = 0; n < WaveOut.DeviceCount; n++)
        {
            var caps = WaveOut.GetCapabilities(n);
            Debug.WriteLine("{0}: {1}", n, caps.ProductName);
        }

        playerTask.MessageSent += (msg) => Invoke(() => HandlePlayerMessage(msg));
        playerTask.Start();
    }

    private void UpdateTestSet()
    {
        testSet.Clear();
        foreach (var item in checkboxItems)
        {
            if (item.IsChecked)
            {
                testSet.AddRange(item.Name switch
                {
                    "Letters" => Morse.Letters,
                    "Words" => Morse.Words,
                    "ProSigns" => Morse.ProSigns,
                    "Numbers" => Morse.Numbers,
                    "Bigrams" => Morse.Bigrams,
                    "Trigrams" => Morse.Trigrams,
                    _ => Enumerable.Empty<string>()
                });
            }
        }
    }

    private void TestSets_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        var item = (CheckboxItem)testSetChecks.Items[e.Index];
        item.IsChecked = e.NewValue == CheckState.Checked;

        if (!loading)
        {
            UpdateTestSet();
            Reset(false);
            AddPlayerString(true);
        }
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        string filePath = Utils.GetDropBoxPath("string_timings.json");
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            filePath = Utils.GetFilePath("string_timings.json");
        }

        allStringTimes = LoadTimings(filePath) ?? new Dictionary<string, RollingAverage>();

        var items = LoadChecks(Utils.GetDropBoxPath("check_bindings.json")) ?? new BindingList<CheckboxItem>();
        if (items.Count != 0)
        {
            checkboxItems = items;
            testSetChecks.DataSource = checkboxItems;
            for (int i = 0; i < checkboxItems.Count; i++)
            {
                testSetChecks.SetItemChecked(i, checkboxItems[i].IsChecked);
            }
        }

        UpdateTestSet();
        Reset(false);

        string[] args = Environment.GetCommandLineArgs();
        if (args.Length > 1 && args[1] == "--autoStart")
        {
            start_Click(this, EventArgs.Empty);
        }

        loading = false;
    }


    public void HandlePlayerMessage(MessageFromPlayer msg)
    {
        if (msg.stringFinishedTimes != null && msg.stringFinishedTimes.Any())
        {
            foreach (var fin in msg.stringFinishedTimes)
            {
                pendingUserInputs.Enqueue(fin);
            }
        }

        if (pendingUserInputs.Count > 0 && pendingUserInputs.Peek().s == " ")
        {
            pendingUserInputs.Dequeue();
        }
    }

    public string GetNextStringToTest()
    {
        var sortedStrings = stringTimes
            .OrderByDescending(kvp => kvp.Value.GetAverage() == 0 ? 10 : kvp.Value.GetAverage())
            .ToList();

        if (!sortedStrings.Any()) return string.Empty;

        double maxAvgTime = sortedStrings.First().Value.GetAverage();
        if (maxAvgTime == 0) maxAvgTime = 10;

        var weightedChars = sortedStrings.Select(stringData =>
        {
            var av = stringData.Value.GetAverage();
            double weight = av <= 0.0 || maxAvgTime == 0.0 ? 10.0 : (Math.Pow(av, 4) / Math.Pow(maxAvgTime, 4)) * 100;
            return (stringData.Key, weight);
        }).ToList();

        double totalAdjustedWeight = weightedChars.Sum(wc => wc.weight);
        var random = new Random();
        double randomValue = random.NextDouble() * (totalAdjustedWeight + 1);
        double cumulativeWeight = 0;

        for (int i = 0; i < weightedChars.Count; i++)
        {
            cumulativeWeight += weightedChars[i].weight;
            weightedChars[i] = (weightedChars[i].Key, cumulativeWeight);
        }

        foreach (var weightedChar in weightedChars)
        {
            if (weightedChar.weight >= randomValue)
            {
                return weightedChar.Key;
            }
        }

        return weightedChars.Last().Key;
    }

    private void AddPlayerString(bool queueIt) => playerTask.QueuePlayerRequest(PlayerRequestType.Play, GetNextStringToTest(), queueIt);

    private void AddPlayerString(string val, bool queueIt) => playerTask.QueuePlayerRequest(PlayerRequestType.Play, val, queueIt);

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        e.Handled = true;

        if (pendingUserInputs.Count == 0)
        {
            currentInput = string.Empty;
            return;
        }

        if (e.Shift)
        {
            if (e.KeyCode == Keys.A)
            {
                AddPlayerString(pendingUserInputs.Peek().s, false);
            }
            else if (e.KeyCode == Keys.S)
            {
                playerTask.QueuePlayerRequest(PlayerRequestType.Save, pendingUserInputs.Peek().s, false);
            }

            currentInput = string.Empty;
            return;
        }
        else if (e.Control || e.Alt || e.KeyCode == Keys.Escape)
        {
            currentInput = string.Empty;
            return;
        }

        var pending = pendingUserInputs.Peek().s.ToUpper();
        var pendingFull = pendingUserInputs.Peek().s.ToUpper();
        var pendingTime = pendingUserInputs.Peek().time;

        var code = char.ToUpper((char)e.KeyCode);

        currentInput += code;
        if (pending[0] == '|')
        {
            pending = pending.Trim('|');
        }

        if (pending.Length > currentInput.Length) return;

        double time = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        bool skipWait = true;
        if (skipWait && (time >= pendingTime))
        {
            if (currentInput == pending)
            {
                double t = time - pendingTime;
                allStringTimes[pendingFull].AddValue(t);

                Debug.WriteLine($"Char: {code}, Time: {t:F1}, Av: {allStringTimes[pendingFull].GetAverage():F1}");

                pendingUserInputs.Dequeue();
                if (pendingUserInputs.Count == 0)
                {
                    AddPlayerString(true);
                }
            }
            else
            {
                allStringTimes[pendingFull].AddValue(allStringTimes[pendingFull].GetAverage() + 1.0);
                allStringTimes[pending].AddValue(allStringTimes[pending].GetAverage() + 1.0);
                playerTask.QueuePlayerRequest(PlayerRequestType.Buzz);
            }

            currentInput = string.Empty;
            rowControl.SetValues(stringTimes);
            UpdateTotals();
        }
        else
        {
            if (pending.Length < currentInput.Length)
            {
                currentInput = string.Empty;
            }
        }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        playerTask.Dispose();
        Save();
    }

    private void Save()
    {
        SaveLocal();
        SaveDropBox();
    }

    private void SaveLocal()
    {
        string filePath = Utils.GetFilePath("string_timings.json");
        SaveTimings(filePath, allStringTimes);

        filePath = Utils.GetFilePath("check_bindings.json");
        SaveChecks(filePath, checkboxItems);
    }

    private void SaveDropBox()
    {
        var filePath = Utils.GetDropBoxPath("string_timings.json");
        if (!string.IsNullOrEmpty(filePath))
        {
            SaveTimings(filePath, allStringTimes);

            filePath = Utils.GetDropBoxPath("check_bindings.json");
            SaveChecks(filePath, checkboxItems);
        }
    }

    private void UpdateTotals()
    {
        // Average of all averages
        double averageOfAllAverages = GetCurrentTimes().Values.Average(ra => ra.Average);   
        double totalOfAllValues = GetCurrentTimes().Values.Sum(ra => ra.Average);
        this.averageValue.Text = $"Total: {totalOfAllValues:F1}\nAvg: {averageOfAllAverages:F1}";

    }
    private void Reset(bool clearTimings)
    {
        pendingUserInputs.Clear();
        foreach (var s in testSet)
        {
            if (!allStringTimes.ContainsKey(s) || clearTimings)
            {
                allStringTimes[s.ToUpper()] = new RollingAverage(5);
            }
        }

        stringTimes = GetCurrentTimes();
        rowControl.SetValues(stringTimes);
        rowControl.Refresh();

        UpdateTotals();

        testSetChecks.SelectedIndex = -1;

    }

    private void start_Click(object sender, EventArgs e)
    {
        Reset(false);
        AddPlayerString(true);
    }

    private void replay_Click(object sender, EventArgs e)
    {
        if (pendingUserInputs.Count != 0)
        {
            AddPlayerString(pendingUserInputs.Peek().s, false);
        }
    }

    private void SaveChecks(string filePath, BindingList<CheckboxItem> bindingList)
    {
        var json = JsonSerializer.Serialize(bindingList, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    private BindingList<CheckboxItem>? LoadChecks(string filePath)
    {
        if (!File.Exists(filePath)) return new BindingList<CheckboxItem>();

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<BindingList<CheckboxItem>>(json);
    }

    public void SaveTimings(string filePath, Dictionary<string, RollingAverage> dictionary)
    {
        try
        {
            string json = JsonSerializer.Serialize(dictionary);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving dictionary: {ex.Message}");
        }
    }

    public Dictionary<string, RollingAverage> LoadTimings(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<Dictionary<string, RollingAverage>>(json) ?? new Dictionary<string, RollingAverage>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading dictionary: {ex.Message}");
        }

        // Return an empty dictionary if file doesn't exist or deserialization fails
        return new Dictionary<string, RollingAverage>();
    }

}