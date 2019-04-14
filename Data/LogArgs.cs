using System;

namespace UUtils.Utilities.Data
{
    /// <summary>
    /// Args for editing table while in editor window.
    /// Must be serializable so they survive editor script reloading.
    /// </summary>
    [Serializable]
    public class LogArgs : EventArgs
    {
        ////////////////////////////////////////////////////////////////////////

        #region Log - Editor Use Only

        #if UNITY_EDITOR

        /// <summary>
        /// Log message
        /// </summary>
        public string LogMessage { get; private set; }

        /// <summary>
        /// Action was successful or failed
        /// </summary>
        public bool ActionStatus { get; private set; }

        public LogArgs(string _logMessage, bool _actionStatus)
        {
            LogMessage = _logMessage;
            ActionStatus = _actionStatus;
        }

        #endif

        #endregion Log - Editor Use Only

        ////////////////////////////////////////////////////////////////////////
    }

}