using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[RequireComponent(typeof(PlayableDirector))]
public class TimelineTest : MonoBehaviour
{
    static TimelineTest()
    {
        // 需要全局注册一次
        ExSignalTrack.SetReceiverCreator((_) => new ExSignalReceiver());
    }

    PlayableDirector pd;
    void Start()
    {
        pd = this.GetComponent<PlayableDirector>();
        var receiver = this.GetComponent<ExSignalReceiverBinding>();
        if (receiver != null)
        {
            receiver.RegisterAction("SignalBind", OnSinal);
        }

        Test1();
    }

    [ContextMenu("Play Timeline")]
    void Test1()
    {
        ExPlay(pd, false).Forget();
    }

    public static async UniTask ExPlay(PlayableDirector playableDirector, bool unscaleTime = false)
    {
        playableDirector.initialTime = -1;
        playableDirector.time = 0;
        var duration = playableDirector.duration;
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

    private void OnSinal(ExSignalReceiverBinding signalReceiverBinding, ExSignalEmitter signalEmitter)
    {
        Debug.Log("SinalBindingTrack:" + signalEmitter.name);
    }

}
