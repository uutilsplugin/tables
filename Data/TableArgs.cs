using System;

namespace UUtils.Utilities.Data
{
    public class TableArgs : EventArgs
    {
        ////////////////////////////////////////////////////////////////////////

        #region Created/Removed Column

        /// <summary>
        /// Created/Removed column
        /// </summary>
        public ITableColumn Column { get; private set; }

        /// <summary>
        /// A column was created/removed
        /// </summary>
        public TableArgs(ITableColumn _column)
        {
            Column = _column;
        }

        #endregion Added Column

        ////////////////////////////////////////////////////////////////////////

        #region Created/Removed Row

        /// <summary>
        /// Create/Removed row
        /// </summary>
        public TableRow Row { get; private set; }

        /// <summary>
        /// A row was created/removed
        /// </summary>
        public TableArgs(TableRow _row)
        {
            Row = _row;
        }

        #endregion Created Row

        ////////////////////////////////////////////////////////////////////////

        #region Shifted Row

        /// <summary>
        /// Index from which a row is moved
        /// </summary>
        /// <value>The index shifted from.</value>
        public int IndexShiftedFrom { get; private set; }

        /// <summary>
        /// Index to which a row is moved to
        /// </summary>
        public int IndexShiftedTo { get; private set; }

        /// <summary>
        /// A row was created/removed
        /// </summary>
        public TableArgs(int _indexShiftedFrom, int _indexShiftedTo)
        {
            IndexShiftedFrom = _indexShiftedFrom;
            IndexShiftedTo = _indexShiftedTo;
        }

        #endregion Shifted Row

        ////////////////////////////////////////////////////////////////////////
    }
}
