using Cysharp.Threading.Tasks;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public static class TimelineUtil
    {
        public static async UniTask PlayAsync(this PlayableDirector playableDirector, bool unscaleTime = false, bool startWithCurrent = false)
        {
            playableDirector.initialTime = 1;
            var duration = playableDirector.duration;
            if (!startWithCurrent)
            {
                playableDirector.time = 0;
            }
            playableDirector.Evaluate();
            await UniTask.Yield();

            while (playableDirector.time < duration)
            {
                var deltaTime = unscaleTime ? Time.unscaledDeltaTime : Time.deltaTime;
                playableDirector.time = playableDirector.time + deltaTime;
                if (playableDirector.time > duration)
                {
                    playableDirector.time = duration;
                }
                playableDirector.Evaluate();
                await UniTask.Yield();
            }
            playableDirector.Stop();
        }

        public static async UniTask RewindAsync(this PlayableDirector playableDirector, bool unscaleTime = false, bool startWithCurrent = false)
        {

            var duration = playableDirector.duration;
            playableDirector.initialTime = -1;
            if (!startWithCurrent)
            {
                playableDirector.time = duration;
            }
            playableDirector.Evaluate();
            await UniTask.Yield();

            while (playableDirector.time > 0)
            {
                var deltaTime = unscaleTime ? Time.unscaledDeltaTime : Time.deltaTime;
                playableDirector.time = playableDirector.time - deltaTime;
                if (playableDirector.time < 0)
                {
                    playableDirector.time = 0;
                }
                playableDirector.Evaluate();
                await UniTask.Yield();
            }
            playableDirector.Stop();
        }

    }
}
