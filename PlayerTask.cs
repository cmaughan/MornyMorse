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
    private readonly ConcurrentQueue<PlayerRequest> _playerMessageQueue = new();
    private readonly CancellationTokenSource _cts = new();
    private WaveFileWriter? _waveFileWriter = null;
    private Task? _task = null;
    public event Action<MessageFromPlayer>? MessageSent;
    private WaveOutEvent? _sendTone = null;
    private ADSRSineWaveProvider? _sentToneADSR = null;

    public void QueuePlayerRequest(PlayerRequestType type, string message = "", bool queueIt = false)
    {
        _playerMessageQueue.Enqueue(new PlayerRequest(type, message, queueIt));
    }

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
                const float wpm = 20.0f;
                const double ditDuration = 1.2f / wpm;
                double totalDuration = 0.0;
                int sampleRate = 44100;

                void AddSpace(int ditLength)
                {
                    var wordPause = new SignalGenerator(sampleRate, 1)
                    {
                        Gain = 0.0
                    }.Take(TimeSpan.FromSeconds(ditDuration * ditLength)) as ISampleProvider;

                    currentProvider = currentProvider?.FollowedBy(wordPause) ?? wordPause;
                    totalDuration += ditDuration * ditLength;
                }

                void AddLetter(string morseDigraph)
                {
                    if (Morse.ToMorse.TryGetValue(morseDigraph, out string? val))
                    {
                        bool previousCharacterInWord = false;
                        foreach (char ditDash in val)
                        {
                            if (previousCharacterInWord) AddSpace(1);
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
                void AddFinishTime(string s)
                {
                    finishedTimes.Add(new StringFinishTime
                    {
                        s = s,
                        time = totalDuration + (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds
                    });
                }

                while (!_cts.Token.IsCancellationRequested)
                {
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

                        bool lastLetter = true;
                        foreach (char c in message.text)
                        {
                            if (c == ' ')
                            {
                                AddSpace(7);
                                lastLetter = false;
                                continue;
                            }

                            if (Morse.ToMorse.TryGetValue(c.ToString(), out string? val))
                            {
                                if (lastLetter) AddSpace(3);
                                AddLetter(c.ToString());
                                lastLetter = true;
                            }
                        }
                        AddFinishTime(message.text);
                        AddSpace(7);

                        if (currentProvider != null)
                        {
                            MessageSent?.Invoke(new MessageFromPlayer
                            {
                                type = PlayerMessageType.PlayStarted,
                                stringFinishedTimes = message.queueIt ? finishedTimes : null
                            });

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

