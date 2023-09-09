using System;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    [Serializable]
    [TrackBindingType(typeof(ExSignalReceiverBinding))]
    [TrackColor(0.39f, 0.24f, 0.49f)]
    public class ExSignalBindingTrack : MarkerTrack
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var result = ScriptPlayable<ExSignalMixerBehaviour>.Create(graph, inputCount);
            var behaviour = result.GetBehaviour();
            var director = go.GetComponent<PlayableDirector>();
            // 获取所有Mark
            var marks = new List<ExSignalEmitter>();
            foreach (var m in this.GetMarkers())
            {
                if (m is ExSignalEmitter emitter)
                {
                    behaviour.AddNotification(emitter);
                }
            }
            //初始化
            var signalReceiver = director.GetGenericBinding(this) as ExSignalReceiverBinding;
            behaviour.InitExSignalBehaviour(director, signalReceiver);

            return result;
        }

        public double GetMarkerMaxTime()
        {
            double maxTimer = 0;
            foreach (var m in this.GetMarkers())
            {
                if (m.time > maxTimer)
                {
                    maxTimer = m.time;
                }
            }
            return maxTimer;
        }

    }
}

