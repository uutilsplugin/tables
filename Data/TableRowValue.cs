using UnityEngine;
using System;

namespace UUtils.Utilities.Data
{
    [Serializable]
    public sealed class TableRowValue : ITableRowValue
    {
        ////////////////////////////////////////////////////////////////////////

        #region Variables

        [SerializeField]
        private string columnName = string.Empty;
        /// <summary>
        /// Set is used only from TableColumn.cs when its name is updated
        /// </summary>
        public string ColumnName { get { return columnName; } set { columnName = value; } }

        [SerializeField]
        private int columnIndex = -1;
        /// <summary>
        /// TODO
        /// Set is used only from TableColumn.cs when its index is updated
        /// </summary>
        public int ColumnIndex { get { return columnIndex; } set { columnIndex = value; } }

        [SerializeField]
        private string val = string.Empty;
        public string Value { get { return val; } set { val = value; } }

        [SerializeField]
        private int index = -1;
        public int Index { get { return index; } }

        #endregion Variables

        ////////////////////////////////////////////////////////////////////////

        #region Constructor

        public TableRowValue(string _columnName, int _columnIndex, string _value, int _index)
        {
            columnName = _columnName;
            columnIndex = _columnIndex;
            val = _value;
            index = _index;
        }

        #endregion Constructor

        ////////////////////////////////////////////////////////////////////////

        #region Update

        public void Update(string _val)
        {
            val = _val;
            OnColumnRowValueUpdated?.Invoke(null, new RowArgs(this));
        }

        public event EventHandler<RowArgs> OnColumnRowValueUpdated;

        /// <summary>
        /// Update row index
        /// </summary>
        public void UpdateIndex(int _index)
        {
            index = _index;
            OnColumnRowIndexUpdated?.Invoke(null, new RowArgs(this));
        }

        public event EventHandler<RowArgs> OnColumnRowIndexUpdated;

        #endregion Update

        ////////////////////////////////////////////////////////////////////////
    }
}
