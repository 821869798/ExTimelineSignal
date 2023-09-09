using System;
using System.Collections.Generic;

namespace UnityEngine.Timeline
{
    public class ExSignalReceiverBinding : MonoBehaviour, IExSignalReceiver
    {

        private Dictionary<string, Action<ExSignalReceiverBinding, ExSignalEmitter>> m_BingdingMap = new Dictionary<string, Action<ExSignalReceiverBinding, ExSignalEmitter>>();

        public void RegisterAction(string name, Action<ExSignalReceiverBinding, ExSignalEmitter> action)
        {
            if (string.IsNullOrEmpty(name) || action == null)
                return;
            if (!m_BingdingMap.ContainsKey(name))
            {
                m_BingdingMap.Add(name, null);
            }
            m_BingdingMap[name] = m_BingdingMap[name] + action;
        }

        public void UnRegisterAction(string name, Action<ExSignalReceiverBinding, ExSignalEmitter> action)
        {
            if (string.IsNullOrEmpty(name) || action == null)
                return;
            if (!m_BingdingMap.ContainsKey(name))
            {
                return;
            }
            m_BingdingMap[name] = m_BingdingMap[name] - action;
        }

        public bool InvokeAction(ExSignalEmitter signalEmitter)
        {
            if (m_BingdingMap.TryGetValue(signalEmitter.name, out var action) && action != null)
            {
                action.Invoke(this, signalEmitter);
                return true;
            }
            return false;
        }

        public void ClearAllAction()
        {
            m_BingdingMap.Clear();
        }

        public void OnSignal(ExSignalEmitter signalEmitter)
        {
            InvokeAction(signalEmitter);
        }
    }
}