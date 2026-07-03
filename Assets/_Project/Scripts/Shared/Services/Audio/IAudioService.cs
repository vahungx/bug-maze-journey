namespace _Project.Scripts.Shared.Services.Audio
{
     using System.Threading;
     using Cysharp.Threading.Tasks;

     public interface IAudioService
     {
          bool IsMuted { get; }

          UniTask<int> PlayAudio(string clipKey, float volume = 1f, bool loop = false, float pitch = 1f,
               CancellationToken        ct = default);

          UniTask PlayOneShot(string clipKey, float volume = 1f, float pitch = 1f, CancellationToken ct = default);

          void StopAudio(int audioId);

          UniTask PlayMusic(string clipKey, float volume = 1f, CancellationToken ct = default);

          void StopMusic();

          void Mute();

          void UnMute();
     }
}
