using Microsoft.VisualBasic;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MornyMorse;

public enum PlayerMessageType
{
    PlayStopped,
    PlayStarted
}


public enum PlayerRequestType
{
    Play,
    Buzz,
    Save,
    SendTone,
    StopTone,
}

public readonly record struct StringFinishTime(string s, double time);

public readonly record struct MessageFromPlayer(PlayerMessageType type, List<StringFinishTime>? stringFinishedTimes);

public readonly record struct PlayerRequest(PlayerRequestType type, string text, bool queueIt);

public class PlayerTask
{
    public float wpm = 25.0f;
    public float letterWpm = 5.0f;
    private readonly ConcurrentQueue<PlayerRequest> _playerMessageQueue = new();
    private readonly CancellationTokenSource _cts = new();
    private WaveFileWriter? _waveFileWriter = null;
    private Task? _task = null;
    public event Action<MessageFromPlayer>? MessageSent;
    private WaveOutEvent? _sendTone = null;
    private ADSRSineWaveProvider? _sentToneADSR = null;
    private Stopwatch _stopwatch = new();
    private double _startTime = 0.0;

    public void QueuePlayerRequest(PlayerRequestType type, string message = "", bool queueIt = false)
    {
        _playerMessageQueue.Enqueue(new PlayerRequest(type, message, queueIt));
    }

    public double StartTime => _startTime;
    public double CurrentTime => _stopwatch.Elapsed.TotalMilliseconds / 1000.0;
    public float DitDuration => 1.2f / wpm;

    public void Start()
    {

        if (_task != null)
        {
            return;
        }

        try
        {
            _task = Task.Run(() =>
            {
                ISampleProvider? currentProvider = null;
                double totalDuration = 0.0;
                int sampleRate = 44100;

                void AddSpace(int ditLength, float thisWpm)
                {
                    var ditTime = 1.2f / thisWpm;
                    var seconds = ditLength * ditTime;
                    var wordPause = new SignalGenerator(sampleRate, 1)
                    {
                        Gain = 0.0
                    }.Take(TimeSpan.FromSeconds(seconds)) as ISampleProvider;

                    currentProvider = currentProvider?.FollowedBy(wordPause) ?? wordPause;
                    totalDuration += seconds;
                }

                void AddLetter(string morseDigraph)
                {
                    var ditDuration = 1.2f / wpm;
                    if (Morse.ToMorse.TryGetValue(morseDigraph, out string? val))
                    {
                        bool previousCharacterInWord = false;
                        foreach (char ditDash in val)
                        {
                            if (previousCharacterInWord)
                            {
                                AddSpace(1, wpm);
                            }
                            previousCharacterInWord = true;

                            var duration = (ditDash == '.') ? ditDuration : ditDuration * 3.0;
                            double attackReleaseTime = ditDuration * 0.10;
                            totalDuration += duration - (attackReleaseTime * 2);

                            var samp = new ADSRSineWaveProvider(
                                frequency: 600,
                                sampleRate: sampleRate,
                                duration: (float)duration,
                                attackTime: (float)attackReleaseTime,
                                decayTime: 0.0f,
                                sustainLevel: 1.0f,
                                releaseTime: (float)attackReleaseTime) as ISampleProvider;

                            currentProvider = currentProvider?.FollowedBy(samp) ?? samp;
                        }
                    }
                }

                var finishedTimes = new List<StringFinishTime>();
                void AddFinishTime(string s, double duration)
                {
                    finishedTimes.Add(new StringFinishTime
                    {
                        s = s,
                        time = duration + CurrentTime
                    });
                }

                while (!_cts.Token.IsCancellationRequested)
                {
                    _stopwatch.Start();
                    _startTime = CurrentTime;

                    finishedTimes.Clear();

                    if (_playerMessageQueue.TryDequeue(out PlayerRequest message))
                    {
                        totalDuration = 0.0f;
                        currentProvider = null;

                        if (message.type == PlayerRequestType.SendTone)
                        {
                            if (_sendTone != null || _sentToneADSR != null)
                            {
                                continue;
                            }
                                
                            _sendTone = new WaveOutEvent { DeviceNumber = 0 };
                            _sendTone.DesiredLatency = 100;

                            var ditDuration = 1.2f / wpm;
                            double attackReleaseTime = ditDuration * 0.0010;
                            _sentToneADSR = new ADSRSineWaveProvider(
                                frequency: 600,
                                sampleRate: sampleRate,
                                duration: (float)1000.0,
                                attackTime: (float)attackReleaseTime,
                                decayTime: 0.0f,
                                sustainLevel: 1.0f,
                                releaseTime: (float)attackReleaseTime);

                            _sendTone.Init(_sentToneADSR as ISampleProvider);
                            _sendTone.Play();
                            continue;
                        }
                        else if (message.type == PlayerRequestType.StopTone)
                        {
                            _sentToneADSR?.EnterRelease();

                            _sendTone = null;
                            _sentToneADSR = null;
                            continue;
                        }

                        var wo = new WaveOutEvent { DeviceNumber = 0 };
                        wo.DesiredLatency = 100;
                        wo.PlaybackStopped += (sender, e) =>
                        {
                            MessageSent?.Invoke(new MessageFromPlayer { type = PlayerMessageType.PlayStopped });
                        };

                        if (message.type == PlayerRequestType.Buzz)
                        {
                            var buzz = new SignalGenerator(sampleRate, 1)
                            {
                                Gain = 0.6,
                                Frequency = 600,
                                Type = SignalGeneratorType.Triangle
                            }.Take(TimeSpan.FromSeconds(0.1)) as ISampleProvider;

                            wo.Init(buzz);
                            wo.Play();
                            continue;
                        }

                        if (string.IsNullOrEmpty(message.text)) continue;

                        bool lastLetter = false;
                        foreach (char c in message.text)
                        {
                            if (c == ' ')
                            {
                                // Space between words
                                AddSpace(7, letterWpm);
                                lastLetter = false;
                                continue;
                            }

                            if (Morse.ToMorse.TryGetValue(c.ToString(), out string? val))
                            {
                                // Add space between letters, 3 dits; a dah
                                if (lastLetter)
                                {
                                    AddSpace(3, letterWpm);
                                }
                                AddLetter(c.ToString());
                                lastLetter = true;
                            }
                        }

                        var duration = totalDuration;
                        //AddFinishTime(message.text);

                        // Space between words, 7 dits
                        AddSpace(7, letterWpm);

                        if (currentProvider != null)
                        {
                            if (message.type == PlayerRequestType.Save)
                            {
                                _waveFileWriter?.Dispose();
                                _waveFileWriter = new WaveFileWriter(Utils.GetFilePath("wave.wav"), currentProvider.WaveFormat);
                                var waveProvider = new SampleToWaveProvider(currentProvider);
                                var recordingProvider = new RecordingWaveProvider(waveProvider, _waveFileWriter);
                                wo.Init(recordingProvider);
                            }
                            else
                            {
                                wo.Init(currentProvider);
                            }

                            wo.Play();

                            AddFinishTime(message.text, duration);
                            
                            MessageSent?.Invoke(new MessageFromPlayer
                            {
                                type = PlayerMessageType.PlayStarted,
                                stringFinishedTimes = message.queueIt ? finishedTimes : null
                            });

                        }
                    }
                }

                Debug.WriteLine("Player stopping due to cancellation.");
            });
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Exception: {e}");
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _task?.Wait();
        _waveFileWriter?.Dispose();
    }
}

