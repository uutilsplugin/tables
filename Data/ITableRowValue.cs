using System;

namespace UUtils.Utilities.Data
{
    public interface ITableRowValue
    {
        /// <summary>
        /// Column to which this row value belong to
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        /// Parent column index
        /// </summary>
        /// <value>The index of the column.</value>
        int ColumnIndex { get; }

        /// <summary>
        /// This column rows value.
        /// Update directly through this property if it doesn't have
        /// event listeners.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Rows index in the column
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Updates rows value in its column. Raises event OnRowUpdated
        /// </summary>
        void Update(string val);

        /// <summary>
        /// Invoked once rows Value is updated using Update(string val) method
        /// </summary>
        event EventHandler<RowArgs> OnColumnRowValueUpdated;

        /// <summary>
        /// Rows index in column was changed
        /// </summary>
        event EventHandler<RowArgs> OnColumnRowIndexUpdated;
    }
}
