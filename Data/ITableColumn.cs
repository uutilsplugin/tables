using System;
using System.Collections.ObjectModel;

namespace UUtils.Utilities.Data
{
    /// <summary>
    /// Interface for updating a single column in a table.
    /// Find out column index, name, get a row values for this column.
    /// </summary>
    public interface ITableColumn
    {
        ////////////////////////////////////////////////////////////////////////

        #region Variables

        /// <summary>
        /// Index in the column list
        /// </summary>
        /// <value>The index.</value>
        int Index { get; }

        string ColumnName { get; }

        #if UNITY_EDITOR
        /// <summary>
        /// Used ONLY when editing column name from editor window
        /// </summary>
        string ColumnNameEditor { get; set; }

        #endif

        int RowCount { get; }

        #endregion Variables

        ////////////////////////////////////////////////////////////////////////

        #region Update

        /// <summary>
        /// Invoked when columns name changes
        /// </summary>
        event EventHandler<ColumnArgs> OnColumnNameUpdate;

        #endregion Update

        ////////////////////////////////////////////////////////////////////////

        #region Get

        /// <summary>
        /// Get a single row in this column at index
        /// </summary>
        /// <param name="index">Row index</param>
        /// <returns>Row at index. Null if index is out of bounds.</returns>
        ITableRowValue GetRowValue(int index);

        /// <summary>
        /// Get a row by its value
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Null if row is not found.</returns>
        ITableRowValue GetRowByValue(string val);

        /// <summary>
        /// Get all rows in this column.
        /// </summary>
        ReadOnlyCollection<ITableRowValue> GetRows();

        #endregion Get

        ////////////////////////////////////////////////////////////////////////
    }
}
