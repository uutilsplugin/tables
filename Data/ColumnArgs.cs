using System;

namespace UUtils.Utilities.Data
{
    public class ColumnArgs : EventArgs
    {
        ////////////////////////////////////////////////////////////////////////

        #region Variables

        /// <summary>
        /// Updated column
        /// </summary>
        public ITableColumn Column { get; private set; }

        #endregion Variables

        ////////////////////////////////////////////////////////////////////////

        #region Constructor

        public ColumnArgs(ITableColumn _column)
        {
            Column = _column;
        }

        #endregion Constructor

        ////////////////////////////////////////////////////////////////////////
    }
}
