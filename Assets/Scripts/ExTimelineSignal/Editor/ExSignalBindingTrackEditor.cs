using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Assets.Scripts.ExTimelineSignal.Editor
{
    [CustomTimelineEditor(typeof(ExSignalBindingTrack))]
    public class ExSignalBindingTrackEditor : TrackEditor
    {
        public override void OnCreate(TrackAsset track, TrackAsset copiedFrom)
        {
            track.CreateCurves("FakeCurves");
            var maxTimer = (track as ExSignalBindingTrack).GetMarkerMaxTime() + 0.01f;
            track.curves.SetCurve(string.Empty, typeof(GameObject), "m_FakeCurve", AnimationCurve.Linear(0, 0, (float)maxTimer, 0));
            base.OnCreate(track, copiedFrom);
        }

        public override void OnTrackChanged(TrackAsset track)
        {
            base.OnTrackChanged(track);
            var maxTimer = (track as ExSignalBindingTrack).GetMarkerMaxTime() + 0.01f;
            track.curves.SetCurve(string.Empty, typeof(GameObject), "m_FakeCurve", AnimationCurve.Linear(0, 0, (float)maxTimer, 0));
        }
    }
}
