namespace _Project.Scripts.Shared.Services.Audio
{
     using System;
     using System.Collections.Generic;
     using System.Threading;
     using Cysharp.Threading.Tasks;
     using _Project.Scripts.Shared.Pooling;
     using _Project.Scripts.Shared.Addressable;
     using UnityEngine;

     public sealed class AudioService : IAudioService,
          IDisposable
     {
          private sealed class ActiveAudio
          {
               public GameObject              Go;
               public AudioSource             Source;
               public CancellationTokenSource AutoRecycleCts;
               public bool                    IsMusic;
          }

          private readonly IAddressableService _addressable;
          private readonly string              _audioSourcePrefabKey;
          private readonly int                 _warmUpCount;

          private readonly Dictionary<string, AudioClip> _clipCache  = new();
          private readonly HashSet<string>               _loadedKeys = new();
          private readonly Dictionary<int, ActiveAudio>  _active     = new();

          private GameObject _audioSourcePrefab;
          private Transform  _sfxRoot;
          private int        _nextId  = 1;
          private int        _musicId = -1;
          private bool       _isMuted;

          public bool IsMuted => _isMuted;

          public AudioService(string audioSourcePrefabKey, int warmUpCount = 3, IAddressableService addressable = null)
          {
               _audioSourcePrefabKey = audioSourcePrefabKey;
               _warmUpCount          = Mathf.Max(1, warmUpCount);
               _addressable          = addressable;
          }

          public async UniTask<int> PlayAudio(string clipKey, float volume = 1f, bool loop = false, float pitch = 1f,
               CancellationToken                     ct = default)
          {
               await EnsureInitializedAsync(ct);

               var clip = await GetOrLoadClipAsync(clipKey, ct);

               if (clip == null) return -1;

               var go     = _audioSourcePrefab.Spawn(parent: _sfxRoot);
               var source = go.GetComponent<AudioSource>() ?? go.AddComponent<AudioSource>();

               source.clip   = clip;
               source.loop   = loop;
               source.pitch  = pitch;
               source.volume = Mathf.Clamp01(volume);
               source.mute   = _isMuted;

               source.Play();

               var id = _nextId++;

               var entry = new ActiveAudio
               {
                    Go      = go,
                    Source  = source,
                    IsMusic = false
               };

               _active[id] = entry;

               if (!loop)
               {
                    var duration = Mathf.Max(0.01f, clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch)));
                    var cts      = new CancellationTokenSource();
                    entry.AutoRecycleCts = cts;
                    AutoRecycleAsync(id, duration, cts.Token).Forget();
               }

               return id;
          }

          public async UniTask PlayOneShot(string clipKey, float volume = 1f, float pitch = 1f,
               CancellationToken                  ct = default)
          {
               await PlayAudio(clipKey, volume, loop: false, pitch: pitch, ct: ct);
          }

          public void StopAudio(int audioId)
          {
               if (!_active.TryGetValue(audioId, out var entry)) return;

               entry.AutoRecycleCts?.Cancel();
               entry.AutoRecycleCts?.Dispose();

               if (entry.Source != null) entry.Source.Stop();
               if (entry.Go     != null) entry.Go.Recycle();

               _active.Remove(audioId);

               if (_musicId == audioId) _musicId = -1;
          }

          public async UniTask PlayMusic(string clipKey, float volume = 1f, CancellationToken ct = default)
          {
               StopMusic();

               var id = await PlayAudio(clipKey, volume, loop: true, pitch: 1f, ct: ct);

               if (id < 0) return;

               if (_active.TryGetValue(id, out var entry))
               {
                    entry.IsMusic = true;
                    _musicId      = id;
               }
          }

          public void StopMusic()
          {
               if (_musicId < 0) return;

               StopAudio(_musicId);
          }

          public void Mute()
          {
               _isMuted = true;
               ApplyMuteState();
          }

          public void UnMute()
          {
               _isMuted = false;
               ApplyMuteState();
          }

          public void Dispose()
          {
               var ids = new List<int>(_active.Keys);
               foreach (var id in ids) StopAudio(id);

               foreach (var key in _loadedKeys) _addressable?.ReleaseAsset(key);

               _clipCache.Clear();
               _loadedKeys.Clear();
          }

          private async UniTask EnsureInitializedAsync(CancellationToken ct)
          {
               if (_audioSourcePrefab != null) return;

               var addr = _addressable
                       ?? ServiceLocator.Resolve<IAddressableService>();

               _audioSourcePrefab = await addr.LoadAssetAsync<GameObject>(_audioSourcePrefabKey, ct);

               if (_audioSourcePrefab == null)
                    throw new InvalidOperationException($"Audio prefab not found: {_audioSourcePrefabKey}");

               var rootGo = new GameObject("[AudioService_SFX]");
               UnityEngine.Object.DontDestroyOnLoad(rootGo);
               _sfxRoot = rootGo.transform;

               _audioSourcePrefab.WarmUp(_warmUpCount);
               _loadedKeys.Add(_audioSourcePrefabKey);
          }

          private async UniTask<AudioClip> GetOrLoadClipAsync(string clipKey, CancellationToken ct)
          {
               if (_clipCache.TryGetValue(clipKey, out var clip)) return clip;

               var addr = _addressable
                       ?? ServiceLocator.Resolve<IAddressableService>();

               clip = await addr.LoadAssetAsync<AudioClip>(clipKey, ct);

               if (clip == null) return null;

               _clipCache[clipKey] = clip;
               _loadedKeys.Add(clipKey);

               return clip;
          }

          private async UniTaskVoid AutoRecycleAsync(int id, float delaySec, CancellationToken ct)
          {
               try
               {
                    await UniTask.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken: ct);
               } catch (OperationCanceledException)
               {
                    return;
               }

               StopAudio(id);
          }

          private void ApplyMuteState()
          {
               foreach (var pair in _active)
               {
                    if (pair.Value?.Source != null) pair.Value.Source.mute = _isMuted;
               }
          }
     }
}
