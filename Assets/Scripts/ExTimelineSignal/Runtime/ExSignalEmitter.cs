namespace UnityEngine.Timeline
{
    public class ExSignalEmitter : Marker
    {
        [SerializeField] bool m_Retroactive;
        [SerializeField] bool m_EmitOnce;

        /// <summary>
        /// Use retroactive to emit the signal if playback starts after the SignalEmitter time.
        /// </summary>
        public bool retroactive
        {
            get { return m_Retroactive; }
            set { m_Retroactive = value; }
        }

        /// <summary>
        /// Use emitOnce to emit this signal once during loops.
        /// </summary>
        public bool emitOnce
        {
            get { return m_EmitOnce; }
            set { m_EmitOnce = value; }
        }

        public NotificationFlags flags
        {
            get
            {
                return (retroactive ? NotificationFlags.Retroactive : default(NotificationFlags)) |
                    (emitOnce ? NotificationFlags.TriggerOnce : default(NotificationFlags)) |
                    NotificationFlags.TriggerInEditMode;
            }
        }

    }
}