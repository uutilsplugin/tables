using System;

namespace UUtils.Utilities.Data
{
    public class RowArgs : EventArgs
    {
        ////////////////////////////////////////////////////////////////////////

        #region Variables

        /// <summary>
        /// Updated row
        /// </summary>
        public ITableRowValue Row { get; private set; }

        #endregion Variables

        ////////////////////////////////////////////////////////////////////////

        #region Constructor

        public RowArgs(ITableRowValue _row)
        {
            Row = _row;
        }

        #endregion Constructor

        ////////////////////////////////////////////////////////////////////////

    }
}
