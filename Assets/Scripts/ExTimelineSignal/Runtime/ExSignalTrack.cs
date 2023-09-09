using System;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    [Serializable]
    [TrackColor(0.39f, 0.24f, 0.49f)]
    public class ExSignalTrack : MarkerTrack
    {
        static Func<PlayableDirector, IExSignalReceiver> receiverCreator;

        /// <summary>
        /// 需要业务层去注册
        /// </summary>
        /// <param name="creator"></param>
        public static void SetReceiverCreator(Func<PlayableDirector, IExSignalReceiver> creator)
        {
            receiverCreator = creator;
        }

        private IExSignalReceiver receiver;

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
            receiver = receiverCreator?.Invoke(director) ?? null;
            behaviour.InitExSignalBehaviour(director, receiver);

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

