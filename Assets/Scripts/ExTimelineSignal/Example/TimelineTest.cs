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

        Test().Forget();
    }

    async UniTask Test()
    {
        await this.pd.PlayAsync();
        await UniTask.Delay(1000);
        await this.pd.RewindAsync();
        Debug.Log("Play Timeline Complete");
    }

    [ContextMenu("Play Timeline")]
    void TestPlayAsync()
    {
        this.pd.PlayAsync().Forget();
    }

    [ContextMenu("Play Timeline reverse")]
    void TestRewindAsync()
    {
        this.pd.RewindAsync().Forget();
    }

    private void OnSinal(ExSignalReceiverBinding signalReceiverBinding, ExSignalEmitter signalEmitter)
    {
        Debug.Log("SinalBindingTrack:" + signalEmitter.name);
    }

}
