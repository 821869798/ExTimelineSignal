
namespace UnityEngine.Timeline
{
    public class ExSignalReceiver : IExSignalReceiver
    {
        public void OnSignal(ExSignalEmitter signalEmitter)
        {
            Debug.Log("ExSginal:" + signalEmitter.name);
        }

    }

}


