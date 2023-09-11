using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public class ExSignalMixerBehaviour : PlayableBehaviour
    {
        /// <summary>
        /// 是否是倒播
        /// </summary>
        private bool isRewind;

        /// <summary>
        /// PlayableDirector
        /// </summary>
        private PlayableDirector director;
        private IExSignalReceiver signalReceiver;

        public void InitExSignalBehaviour(PlayableDirector director, IExSignalReceiver signalReceiver)
        {
            this.director = director;
            this.signalReceiver = signalReceiver;
            CheckEvaluatePlayType();
        }

        #region Singal Mananger

        /// <summary>
        /// 所有的Mark
        /// </summary>
        readonly List<NotificationEntry> m_Notifications = new List<NotificationEntry>();

        double m_PreviousTime;
        bool m_NeedSortNotifications;

        struct NotificationEntry
        {
            public double time;
            public ExSignalEmitter payload;
            public bool notificationFired;
            public NotificationFlags flags;

            public bool triggerInEditor
            {
                get { return (flags & NotificationFlags.TriggerInEditMode) != 0; }
            }
            public bool prewarm
            {
                get { return (flags & NotificationFlags.Retroactive) != 0; }
            }
            public bool triggerOnce
            {
                get { return (flags & NotificationFlags.TriggerOnce) != 0; }
            }
        }

        public void AddNotification(ExSignalEmitter payload)
        {
            var time = payload.time;
            var notificationOptionProvider = payload as INotificationOptionProvider;
            m_Notifications.Add(new NotificationEntry
            {
                time = time,
                payload = payload,
                flags = notificationOptionProvider?.flags ?? NotificationFlags.Retroactive,
            });
            m_NeedSortNotifications = true;
        }


        void SortNotifications()
        {
            if (m_NeedSortNotifications)
            {
                m_Notifications.Sort((x, y) => x.time.CompareTo(y.time));
                m_NeedSortNotifications = false;
            }
        }

        static bool CanRestoreNotification(NotificationEntry e, FrameData info, double currentTime, double previousTime)
        {
            if (e.triggerOnce)
                return false;
            if (info.timeLooped)
                return true;

            //case 1111595: restore the notification if the time is manually set before it
            return previousTime > currentTime && currentTime <= e.time;
        }

        void TriggerNotificationsInRange(double start, double end, FrameData info, Playable playable, bool checkState)
        {
            if (start <= end)
            {
                var playMode = Application.isPlaying;
                for (var i = 0; i < m_Notifications.Count; i++)
                {
                    var e = m_Notifications[i];
                    if (e.notificationFired && (checkState || e.triggerOnce))
                        continue;

                    var notificationTime = e.time;
                    if (e.prewarm && notificationTime < end && (e.triggerInEditor || playMode))
                    {
                        Trigger_internal(playable, info.output, ref e);
                        m_Notifications[i] = e;
                    }
                    else
                    {
                        if (notificationTime < start || notificationTime > end)
                            continue;

                        if (e.triggerInEditor || playMode)
                        {
                            Trigger_internal(playable, info.output, ref e);
                            m_Notifications[i] = e;
                        }
                    }
                }
            }
        }

        // CanRestoreNotificationReverse 反向播放时，是否可以恢复通知
        static bool CanRestoreNotificationReverse(NotificationEntry e, FrameData info, double currentTime, double previousTime)
        {
            if (e.triggerOnce)
                return false;
            if (info.timeLooped)
                return true;

            //case 1111595: restore the notification if the time is manually set before it
            return previousTime < currentTime && currentTime >= e.time;
        }

        // TriggerNotificationsInRangeReverse 反向播放触发
        void TriggerNotificationsInRangeReverse(double start, double end, FrameData info, Playable playable, bool checkState)
        {
            if (start >= end)
            {
                var playMode = Application.isPlaying;
                for (var i = 0; i < m_Notifications.Count; i++)
                {
                    var e = m_Notifications[i];
                    if (e.notificationFired && (checkState || e.triggerOnce))
                        continue;

                    var notificationTime = e.time;
                    if (e.prewarm && notificationTime > end && (e.triggerInEditor || playMode))
                    {
                        Trigger_internal(playable, info.output, ref e);
                        m_Notifications[i] = e;
                    }
                    else
                    {
                        if (notificationTime > start || notificationTime < end)
                            continue;

                        if (e.triggerInEditor || playMode)
                        {
                            Trigger_internal(playable, info.output, ref e);
                            m_Notifications[i] = e;
                        }
                    }
                }
            }
        }

        void Trigger_internal(Playable playable, PlayableOutput output, ref NotificationEntry e)
        {
            //output.PushNotification(playable, e.payload);
            this.signalReceiver?.OnSignal(e.payload);
            e.notificationFired = true;
        }

        void Restore_internal(ref NotificationEntry e)
        {
            e.notificationFired = false;
        }
        #endregion

        public override void OnGraphStart(Playable playable)
        {
            //有可能存在 手动倒播到一半 且触发了一些clip，即停止
            //然后再重新正常播放 所以统一在正常播放开始时，重置clip状态
            if (director.initialTime != 0 && director.state == PlayState.Paused)
            {
                director.initialTime = 0;
            }
            SortNotifications();
            var currentTime = playable.GetTime();
            for (var i = 0; i < m_Notifications.Count; i++)
            {
                // case 1257208 - when a timeline is _resumed_, only reset notifications after the resumed time
                if (m_Notifications[i].time > currentTime && !m_Notifications[i].triggerOnce)
                {
                    var notification = m_Notifications[i];
                    notification.notificationFired = false;
                    m_Notifications[i] = notification;
                }
            }
            m_PreviousTime = currentTime;
            base.OnGraphStart(playable);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {

            //此时为手动播放 在手动播放接口中 正播为-1 倒放为duration
            if (director.initialTime != 0 && director.state == PlayState.Paused)
            {
                CheckEvaluatePlayType();
            }

            SortNotifications();
            var currentTime = playable.GetTime();
            if (isRewind)
            {
                TriggerNotificationsInRangeReverse(m_PreviousTime, currentTime, info,
                                       playable, true);

                for (var i = 0; i < m_Notifications.Count; ++i)
                {
                    var e = m_Notifications[i];
                    if (e.notificationFired && CanRestoreNotificationReverse(e, info, currentTime, m_PreviousTime))
                    {
                        Restore_internal(ref e);
                        m_Notifications[i] = e;
                    }
                }
            }
            else
            {

                TriggerNotificationsInRange(m_PreviousTime, currentTime, info,
                       playable, true);

                for (var i = 0; i < m_Notifications.Count; ++i)
                {
                    var e = m_Notifications[i];
                    if (e.notificationFired && CanRestoreNotification(e, info, currentTime, m_PreviousTime))
                    {
                        Restore_internal(ref e);
                        m_Notifications[i] = e;
                    }
                }
            }

            m_PreviousTime = currentTime;

            base.ProcessFrame(playable, info, playerData);
        }

        /// <summary>
        /// 检查手动播放的播放时序
        /// </summary>
        void CheckEvaluatePlayType()
        {
            if (director.initialTime > 0)
            {
                isRewind = false;
            }
            else if (director.initialTime < 0)
            {
                isRewind = true;
            }
        }

    }
}
