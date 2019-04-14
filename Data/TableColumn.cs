using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using System;

namespace UUtils.Utilities.Data
{
    [Serializable]
    public sealed class TableColumn : ITableColumn
    {
        ////////////////////////////////////////////////////////////////////////

        #region Variables

        [SerializeField]
        private int index = -1;
        /// <summary>
        /// Index in the column list
        /// </summary>
        /// <value>The index.</value>
        public int Index { get { return index; } }

        [SerializeField]
        private string columnName = string.Empty;
        public string ColumnName { get { return columnName; } }

        #if UNITY_EDITOR
        public string ColumnNameEditor { get; set; }
        #endif

        [SerializeField]
        private List<TableRowValue> rows = new List<TableRowValue>();
        public int RowCount { get { return rows.Count; } }

        #endregion Variables

        ////////////////////////////////////////////////////////////////////////

        #region Constructor

        public TableColumn(string _name, int _index)
        {
            columnName = _name;
            index = _index;
        }

        #endregion Constructor

        ////////////////////////////////////////////////////////////////////////

        #region Update

        /// <summary>
        /// Should only be used from the table so it checks if there are other
        /// columns with new name.
        /// Updates column name. Raises OnColumnNameUpdate.
        /// </summary>
        public void UpdateName(string _name)
        {
            columnName = _name;

            int _count = RowCount;
            for (int _i = 0; _i < _count; _i++)
            {
                rows[_i].ColumnName = columnName;
            }

            OnColumnNameUpdate?.Invoke(null, new ColumnArgs(this));
        }

        public event EventHandler<ColumnArgs> OnColumnNameUpdate;

        public void ShiftRows(int _indexFrom, int _indexTo)
        {
            TableRowValue _row = rows[_indexFrom];
            rows.RemoveAt(_indexFrom);
            rows.Insert(_indexTo, _row);

            ReIndexRows();
        }

        /// <summary>
        /// Updates every rows index after rows were shifted
        /// </summary>
        private void ReIndexRows()
        {
            int _count = RowCount;
            for (int _i = 0; _i < _count; _i++)
            {
                rows[_i].UpdateIndex(_i);
            }
        }

        public void ReIndex(int _index)
        {
            index = _index;
        }

        #endregion Update

        ////////////////////////////////////////////////////////////////////////

        #region Create

        /// <summary>
        /// Creates and returns the created row
        /// </summary>
        public ITableRowValue CreateTableRow(string _value)
        {
            TableRowValue _rowVal = new TableRowValue(columnName, index, _value, RowCount);
            rows.Add(_rowVal);
            return _rowVal;
        }

        /// <summary>
        /// Creates a row at index and returns it.
        /// </summary>
        public ITableRowValue CreateTableRow(string _value, int _index)
        {
            if (_index >= RowCount)
            {
                return null;
            }

            TableRowValue _rowVal = new TableRowValue(columnName, index, _value, RowCount);
            rows.Insert(_index, _rowVal);
            ReIndexRows();

            return _rowVal;
        }

        /// <summary>
        /// Creates a table row
        /// </summary>
        public void AddTableRow(string _value)
        {
            TableRowValue _rowVal = new TableRowValue(columnName, index, _value, RowCount);
            rows.Add(_rowVal);
        }

        /// <summary>
        /// Creates a row at index
        /// </summary>
        public void AddTableRow(string _value, int _index)
        {
            if (_index >= RowCount)
            {
                return;
            }

            TableRowValue _rowVal = new TableRowValue(columnName, index, _value, RowCount);
            rows.Add(_rowVal);

            ReIndexRows();
        }

        #endregion Create

        ////////////////////////////////////////////////////////////////////////

        #region Remove

        /// <summary>
        /// Removes a row at index and return the row
        /// </summary>
        public ITableRowValue Remove(int _index)
        {
            ITableRowValue _row = rows[_index];
            rows.RemoveAt(_index);
            return _row;
        }

        #endregion Remove

        ////////////////////////////////////////////////////////////////////////

        #region Check

        private bool RowAtIndexExists(int _index)
        {
            if (_index < RowCount && _index > -1)
            {
                return true;
            }

            return false;
        }

        #endregion Check

        ////////////////////////////////////////////////////////////////////////

        #region Get

        public ITableRowValue GetRowValue(int _index)
        {
            if(!RowAtIndexExists(_index))
            {
                return null;
            }

            return rows[_index];
        }

        public ITableRowValue GetRowByValue(string _val)
        {
            int _count = RowCount;
            for (int _i = 0; _i < _count; _i++)
            {
                if(rows[_i].Value == _val)
                {
                    return rows[_i];
                }
            }

            return null;
        }

        public ReadOnlyCollection<ITableRowValue> GetRows()
        {
            List<ITableRowValue> _rows = new List<ITableRowValue>(rows);
            return new ReadOnlyCollection<ITableRowValue>(_rows);
        }

        #endregion Get

        ////////////////////////////////////////////////////////////////////////
    }
}
