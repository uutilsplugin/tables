using System.Collections.Generic;
using UnityEngine;

namespace UUtils.Utilities.Data
{
    /// <summary>
    /// Examples of all use cases for Tables API.
    /// </summary>
    public class ExampleTables : MonoBehaviour
    {
        ////////////////////////////////////////////////////////////////////////

        #region Vars

        [Header("Scriptable Object and saving/loading")]

        [SerializeField]
        private TableSO _tableSO;

        #endregion Vars

        ////////////////////////////////////////////////////////////////////////

        #region Methods

        /// <summary>
        /// Initialize the table with an ID, name, and columns
        /// </summary>
        public void InitializeTable()
        {
             int _tableID = -1;
             string _tableName = "Table";
             List<string> _columnNames = new List<string> { "ID", "Name" };

            _tableSO.Table = new Table(_tableID, _tableName, _columnNames);
        }

        public void SaveTable()
        {
            Table.SaveTable("Name", _tableSO.Table);
        }

        #endregion Methods

        ////////////////////////////////////////////////////////////////////////

        #region Column

        /// <summary>
        /// Create a column with a name.
        /// </summary>
        public void CreateColumn()
        {
            string _name = "Name";

            if (_tableSO.Table.CreateColumn(_name))
            {
                //
            }
        }

        /// <summary>
        /// Create a column with a name at index.
        /// </summary>
        public void CreateColumnAtIndex()
        {
            string _name = "Name";
            int _index = 3;
            if (_tableSO.Table.CreateColumn(_name, _index))
            {
                //
            }
        }

        /// <summary>
        /// Get a column by its name
        /// </summary>
        public void GetColumn()
        {
            string _name = "ID";
            ITableColumn _column = _tableSO.Table.GetColumn(_name);
        }

        /// <summary>
        /// Get a column by name and find a single row with value
        /// </summary>
        public void GetColumnAndASingleRowValue()
        {
            string _name = "Name";
            string _value = "Character With A Name";
            ITableColumn _column = _tableSO.Table.GetColumn(_name);
            ITableRowValue _row = _column.GetRowByValue(_value);
        }

        #endregion Column

        ////////////////////////////////////////////////////////////////////////

        #region Row

        /// <summary>
        /// Insert a row into the table
        /// </summary>
        public void InsertRow()
        {
            TableRow _row = _tableSO.Table.InsertRow();
        }

        /// <summary>
        /// Insert a row into the table
        /// </summary>
        public void InsertRowAtIndex()
        {
            int _index = 3;
            TableRow _row = _tableSO.Table.InsertRow(_index);
        }

        /// <summary>
        /// Get a row and update a column value for that row
        /// </summary>
        public void GetRowAndUpdateValue()
        {
            // Get a row at index
            int _index = -1;
            TableRow _row = _tableSO.Table.GetRowByIndex(_index);
            if (_row != null)
            {
                // Get a column in the row you wish to update by its name
                ITableRowValue _column = _row.GetColumn("Name");
                // update with a value
                // if an object subscribed to updates, it will be notified
                _column?.Update("New Value");
            }
        }

        public void GetRows()
        {
            List<TableRow> _rows = _tableSO.Table.GetRows();
        }

        #endregion Row

        ////////////////////////////////////////////////////////////////////////

        #region Subscribe

        /// <summary>
        /// Subscribe to all rows for value changes
        /// </summary>
        public void SubscribeToRowUpdates()
        {
            List<TableRow> _rows = _tableSO.Table.GetRows();
            int _count = _rows.Count;
            for (int _i = 0; _i < _count; _i++)
            {
                int _columnCount = _rows[_i].RowColumns.Count;
                for (int _j = 0; _j < _columnCount; _j++)
                {
                    _rows[_i].RowColumns[_j].OnColumnRowValueUpdated += OnColumnRowValueUpdated;
                }
            }
        }

        private void OnColumnRowValueUpdated(object _sender, RowArgs _args)
        {
            string.Format(
                "Row updated. Row index: {0}, Column: {1}, New Value: {2}",
                _args.Row.Index,
                _args.Row.ColumnName,
                _args.Row.Value
            );
        }

        #endregion Subscribe

        ////////////////////////////////////////////////////////////////////////

    }
}
