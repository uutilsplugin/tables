using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace UUtils.Utilities.Data
{
    [Serializable]
    public class Table
    {
        ////////////////////////////////////////////////////////////////////////

        #region Columns

        /// <summary>
        /// Assign this on your own if you want to manage your own table database.
        /// </summary>
        public int ID = -1;

        /// <summary>
        /// Table name.
        /// </summary>
        public string Name = string.Empty;

        #endregion Table

        ////////////////////////////////////////////////////////////////////////

        #region Columns

        /// <summary>
        /// All columns in a table. Each column contains row values.
        /// Row values from all columns must be combined to get an entire row at index.
        /// </summary>
        [SerializeField]
        private List<TableColumn> columns = new List<TableColumn>();
        public int ColumnCount 
        { 
            get
            {
                if(columns == null)
                {
                    return 0;
                }

                return columns.Count; 
            } 
        }

        /// <summary>
        /// Returns a list of all column names. Null if none exist.
        /// </summary>
        public List<string> ColumnNames 
        { 
            get 
            {
                if (columns == null)
                {
                    return null;
                }

                return columns.Select(_x => _x.ColumnName).ToList(); 
            }
        }

        #endregion Columns

        ////////////////////////////////////////////////////////////////////////

        #region Rows

        /// <summary>
        /// Managed by the Table.cs.
        /// EVeryy time a row is removed or added, this will be updated.
        /// This is done so you don't ever have to perform a count on how many rows there are.
        /// </summary>
        [SerializeField]
        private int rowCount;
        /// <summary>
        /// How many rows in a table.
        /// </summary>
        public int RowCount { get { return rowCount; }}

        #endregion Rows

        ////////////////////////////////////////////////////////////////////////

        #region Constructor

        public Table(int _id, string _name, List<string> _columnNames = null)
        {
            ID = _id;
            Name = _name;

            if(_columnNames != null)
            {
                int count = _columnNames.Count;
                for (int _i = 0; _i < count; _i++)
                {
                    CreateColumn(_columnNames[_i]);
                }
            }
        }

        #endregion Constructor

        ////////////////////////////////////////////////////////////////////////

        #region Pages

        /// <summary>
        /// How many pages of items does a table have
        /// </summary>
        /// <returns>The page count.</returns>
        /// <param name="_perPage">How many items to display per page.</param>
        public int GetPageCount(int _perPage)
        {
            if(RowCount == 0 || _perPage == 0)
            {
                return 0;
            }

            int _remainder = RowCount % _perPage > 0 ? 1 : 0;
            return RowCount / _perPage + _remainder;
        }

        #endregion Pages

        ////////////////////////////////////////////////////////////////////////

        #region Log - Editor Use Only

        #if UNITY_EDITOR

        /// <summary>
        /// Editor only event. 
        /// Subscribe to get messages about table changes.
        /// </summary>
        public event EventHandler<LogArgs> OnLogUpdate;

        #endif

        #endregion Log - Editor Use Only

        ////////////////////////////////////////////////////////////////////////

        #region Create

        /// <summary>
        /// Creates a column and adds all necessary rows to it.
        /// </summary>
        /// <returns>False if column with the name already exists</returns>
        public bool CreateColumn(string _name)
        {
            if(string.IsNullOrEmpty(_name))
            {
                #if UNITY_EDITOR
                string _msg = string.Format("Can't create a column. Column must have a name!");
                Debug.LogError(_msg);
                OnLogUpdate?.Invoke(null, new LogArgs(_msg, false));
                #endif
                              
                return false;
            }

            if (ColumnExists(_name))
            {
                #if UNITY_EDITOR
                string _msg = string.Format("Can't create a column. Column with name: {0}, already exists!", _name);
                Debug.LogError(_msg);
                OnLogUpdate?.Invoke(null, new LogArgs(_msg, false));
                #endif
                              
                return false;
            }

            // Create column
            TableColumn _column = new TableColumn(_name, ColumnCount);

            // Add to list
            columns.Add(_column);

            // Create rows in the column
            for (int _i = 0; _i < rowCount; _i++)
            {
                _column.CreateTableRow(string.Empty);
            }

            OnColumnCreated?.Invoke(null, new TableArgs(_column));

            #if UNITY_EDITOR
            string _message = string.Format("Created a column with name: {0}", _name);
            OnLogUpdate?.Invoke(null, new LogArgs(_message, true));
            #endif

            ReIndexColumns();

            return true;
        }

        public bool CreateColumn(string _name, int _index)
        {
            if(_index >= ColumnCount)
            {
                return false;
            }

            if (ColumnExists(_name))
            {
                #if UNITY_EDITOR
                string _msg = string.Format("Column with name: {0}, already exists!", _name);
                Debug.LogError(_msg);
                OnLogUpdate?.Invoke(null, new LogArgs(_msg, false));
                #endif

                return false;
            }

            // Create column
            TableColumn _column = new TableColumn(_name, _index);

            // Add to list
            columns.Insert(_index, _column);

            // Create rows in the column
            for (int _i = 0; _i < rowCount; _i++)
            {
                _column.CreateTableRow(string.Empty);
            }

            OnColumnCreated?.Invoke(null, new TableArgs(_column));

            #if UNITY_EDITOR
            string _message = string.Format("Created a column with name: {0}", _name);
            OnLogUpdate?.Invoke(null, new LogArgs(_message, true));
            #endif

            ReIndexColumns();

            return true;
        }

        public TableRow InsertRow()
        {
            // No columns existing
            if(ColumnCount == 0)
            {
                #if UNITY_EDITOR
                string _msg = string.Format("Attempted to insert a row when no columns exist!");
                Debug.LogError(_msg);
                OnLogUpdate?.Invoke(null, new LogArgs(_msg, false));
                #endif

                return null;
            }

            List<ITableRowValue> _columns = new List<ITableRowValue>();

            int _count = ColumnCount;
            for (int _i = 0; _i < _count; _i++)
            {
                ITableRowValue _rowValue = columns[_i].CreateTableRow(string.Empty);
                _columns.Add(_rowValue);
            }

            // Increment row count
            rowCount++;

            TableRow _row = new TableRow(_columns);
            OnRowInserted?.Invoke(null, new TableArgs(_row));

            #if UNITY_EDITOR
            string _message = string.Format("Row inserted!");
            OnLogUpdate?.Invoke(null, new LogArgs(_message, true));
            #endif

            return _row;
        }

        /// <summary>
        /// Create a row at index
        /// </summary>
        /// <returns>The row.</returns>
        /// <param name="_index">Index.</param>
        public TableRow InsertRow(int _index)
        {
            // No columns existing
            if (ColumnCount == 0)
            {
                #if UNITY_EDITOR
                string _msg = string.Format("Attempted to insert a row when no columns exist!");
                Debug.LogError(_msg);
                OnLogUpdate?.Invoke(null, new LogArgs(_msg, false));
                #endif

                return null;
            }

            if(_index >= rowCount)
            {
                #if UNITY_EDITOR
                string _msg = string.Format("Cannot insert a row! Index: {0} exceeds row count: {1}", _index, rowCount);
                Debug.LogError(_msg);
                OnLogUpdate?.Invoke(null, new LogArgs(_msg, false));
                #endif

                return null;
            }

            if(_index < 0)
            {
                #if UNITY_EDITOR
                string _msg = string.Format("Cannot insert a row! Negative index: {0}", _index);
                Debug.LogError(_msg);
                OnLogUpdate?.Invoke(null, new LogArgs(_msg, false));
                #endif

                return null;
            }

            List<ITableRowValue> _columns = new List<ITableRowValue>();

            int _count = ColumnCount;
            for (int _i = 0; _i < _count; _i++)
            {
                ITableRowValue rowValue = columns[_i].CreateTableRow(string.Empty, _index);
                _columns.Add(rowValue);
            }

            // Increment row count
            rowCount++;

            TableRow _row = new TableRow(_columns);
            OnRowInserted?.Invoke(null, new TableArgs(_row));

            #if UNITY_EDITOR
            string _message = string.Format("Row inserted. Index: {0}", _index);
            OnLogUpdate?.Invoke(null, new LogArgs(_message, true));
            #endif

            return _row;
        }

        /// <summary>
        /// Column was created
        /// </summary>
        public event EventHandler<TableArgs> OnColumnCreated;

        /// <summary>
        /// Row was created
        /// </summary>
        public event EventHandler<TableArgs> OnRowInserted;

        #endregion Create

        ////////////////////////////////////////////////////////////////////////

        #region Shift Rows

        /// <summary>
        /// Move a row to another index
        /// </summary>
        public bool ShiftRow(int _indexFrom, int _indexTo)
        {
            if(_indexFrom == _indexTo)
            {
                #if UNITY_EDITOR
                string _msg = string.Format("Cannot shift rows with same index: {0}", _indexFrom);
                Debug.LogError(_msg);
                OnLogUpdate?.Invoke(null, new LogArgs(_msg, false));
                #endif

                return false;
            }

            if(RowCount > -1)
            {
                if (_indexFrom < 0 || _indexFrom >= RowCount || _indexTo < 0 || _indexTo >= RowCount)
                {
                    #if UNITY_EDITOR
                    string _msg = string.Format(
                        "Cannot shift rows, index out of bounds! Index from: {0}, Index To: {1}, Row Count: {2}",
                        _indexFrom,
                        _indexTo,
                        RowCount
                    );

                    Debug.LogError(_msg);
                    OnLogUpdate?.Invoke(null, new LogArgs(_msg, false));
                    #endif

                    return false;
                }

                int _count = ColumnCount;
                for (int _i = 0; _i < _count; _i++)
                {
                    columns[_i].ShiftRows(_indexFrom, _indexTo);
                }

                OnRowShifted?.Invoke(null, new TableArgs(_indexFrom, _indexTo));

                #if UNITY_EDITOR
                string _message = string.Format("Row shifted. From Index: {0}, To Index: {1}", _indexFrom, _indexTo);
                OnLogUpdate?.Invoke(null, new LogArgs(_message, true));
                #endif

                return true;
            }

            return false;
        }

        /// <summary>
        /// Subscribe if you'd like to be aware if a row was shifted
        /// </summary>
        public event EventHandler<TableArgs> OnRowShifted;

        #endregion Shift Rows

        ////////////////////////////////////////////////////////////////////////

        #region Check

        private bool ColumnExists(string _name)
        {
            int _count = ColumnCount;
            for (int _i = 0; _i < _count; _i++)
            {
                if (columns[_i].ColumnName == _name)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ColumnAtIndexExists(int _index)
        {
            if (_index < ColumnCount && _index > -1)
            {
                return true;
            }

            return false;
        }

        #endregion Check

        ////////////////////////////////////////////////////////////////////////

        #region ReIndex Columns

        private void ReIndexColumns()
        {
            int count = ColumnCount;
            for (int i = 0; i < count; i++)
            {
                TableColumn _column = GetColumnByIndex(i) as TableColumn;
                _column?.ReIndex(i);
            }
        }

        #endregion ReIndex Columns

        ////////////////////////////////////////////////////////////////////////

        #region Remove

        public bool RemoveColumn(string _name)
        {
            ITableColumn _column = GetColumn(_name);
            if(_column != null)
            {
                OnColumnRemoved?.Invoke(null, new TableArgs(_column));
                columns.RemoveAt(_column.Index);

                #if UNITY_EDITOR
                string _msg = string.Format("Removed column with name: {0}", _name);
                OnLogUpdate?.Invoke(null, new LogArgs(_msg, true));
                #endif

                return true;
            }

            #if UNITY_EDITOR
            string _message = string.Format("Column with name: {0} doesn't exist!", _name);
            Debug.LogError(_message);
            OnLogUpdate?.Invoke(null, new LogArgs(_message, false));
            #endif

            ReIndexColumns();

            return false;
        }

        public void RemoveColumn(ITableColumn _column)
        {
            if (_column.Index >= columns.Count || _column.Index < 0)
            {
                #if UNITY_EDITOR
                string _message = string.Format("Can't remove column. Index: {0} is out of bounds!", _column);
                Debug.LogError(_message);
                OnLogUpdate?.Invoke(null, new LogArgs(_message, false));
                #endif

                return;
            }

            columns.RemoveAt(_column.Index);
            OnColumnRemoved?.Invoke(null, new TableArgs(_column));

            #if UNITY_EDITOR
            string _msg = string.Format("Removed column with name: {0}, index: {1}", _column.ColumnName, _column.Index);
            OnLogUpdate?.Invoke(null, new LogArgs(_msg, true));
            #endif

            ReIndexColumns();
        }

        /// <summary>
        /// Column was created
        /// </summary>
        public event EventHandler<TableArgs> OnColumnRemoved;

        /// <summary>
        /// Tries to remove a row from table. Will raise OnRowRemoved which will
        /// contain the last reference to the removed row
        /// </summary>
        /// <returns>True if removed, false if not.</returns>
        public bool RemoveRow(int _index)
        {
            if(_index >= RowCount)
            {
                #if UNITY_EDITOR
                string _message = string.Format("Can't remove row. Index: {0} is out of bounds!", _index);
                Debug.LogError(_message);
                OnLogUpdate?.Invoke(null, new LogArgs(_message, false));
                #endif

                return false;
            }

            List<ITableRowValue> _rows = new List<ITableRowValue>();
            TableRow _row = new TableRow(_rows);

            int _count = ColumnCount;
            for (int _i = 0; _i < _count; _i++)
            {
                // Remove a row value from column and add it to the removed rows
                _rows.Add(columns[_i].Remove(_index));
            }

            rowCount--;

            OnRowRemoved?.Invoke(null, new TableArgs(_row));

            #if UNITY_EDITOR
            string _msg = string.Format("Row removed. Index: {0}", _index);
            OnLogUpdate?.Invoke(null, new LogArgs(_msg, true));
            #endif

            return false;
        }

        /// <summary>
        /// A row was removed from the table.
        /// Event will contain that row in TableArgs.
        /// </summary>
        public event EventHandler<TableArgs> OnRowRemoved;

        public void RemoveAllRows()
        {
            for (int i = RowCount - 1; i >= 0; i--)
            {
                RemoveRow(i);
            }
        }

        #endregion Remove

        ////////////////////////////////////////////////////////////////////////

        #region Update

        public bool UpdateColumnName(string _newName, ITableColumn _column)
        {
            if(ColumnExists(_newName))
            {
                #if UNITY_EDITOR
                string _msg = string.Format("Can't rename column {0}. Column with name: {1}, already exists!", _column.ColumnName, _newName);
                Debug.LogError(_msg);
                OnLogUpdate?.Invoke(null, new LogArgs(_msg, false));
                #endif

                return false;
            }

            TableColumn _cast = _column as TableColumn;
            _cast?.UpdateName(_newName);

            return true;
        }

        #endregion Update

        ////////////////////////////////////////////////////////////////////////

        #region Get

        /// <summary>
        /// Find a row by index.
        /// </summary>
        /// <param name="_index"></param>
        /// <returns>Null if row not found.</returns>
        public TableRow GetRowByIndex(int _index)
        {
            if(_index >= rowCount)
            {
                return null;
            }

            List<ITableRowValue> _columns = new List<ITableRowValue>();

            int _count = ColumnCount;
            for (int _i = 0; _i < _count; _i++)
            {
                ITableRowValue _rowValue = columns[_i].GetRowValue(_index);
                _columns.Add(_rowValue);
            }

            TableRow _row = new TableRow(_columns);
            return _row;
        }

        /// <summary>
        /// Get all rows in the table
        /// </summary>
        /// <returns>List of rows.</returns>
        public List<TableRow> GetRows()
        {
            List<TableRow> _rows = new List<TableRow>();
            int _rowCount = RowCount;
            for (int _i = 0; _i < _rowCount; _i++)
            {
                TableRow _row = GetRowByIndex(_i);
                if (_row != null)
                {
                    _rows.Add(_row);
                }

                else
                {
                    #if UNITY_EDITOR
                    string _message = string.Format("Index: {0} is out of bounds!", _i);
                    Debug.LogError(_message);
                    OnLogUpdate?.Invoke(null, new LogArgs(_message, false));
                    #endif
                    return null;
                }
            }

            return _rows;
        }

        /// <summary>
        /// Get a number of rows starting from index
        /// </summary>
        /// <returns>The rows.</returns>
        /// <param name="_startIndex">Start index.</param>
        /// <param name="_count">Count.</param>
        /// <param name="_isPagination">True ONLY when you are displaying rows on a page, if false, will display error if trying to get too many rows.</param>
        public List<TableRow> GetRows(int _startIndex, int _count, bool _isPagination)
        {
            if(RowCount == 0)
            {
                return null;
            }

            if (!_isPagination && _startIndex >= RowCount)
            {
                #if UNITY_EDITOR
                string _message = string.Format("Can't select rows. Index: {0} is out of bounds!", _startIndex);
                Debug.LogError(_message);
                OnLogUpdate?.Invoke(null, new LogArgs(_message, false));
                #endif

                return null;
            }

            List<TableRow> _rows = new List<TableRow>();

            // Last item which should be added to the list
            int _endIndex = _startIndex + _count;
            int _rowCount = _count > RowCount ? RowCount : _count;
            for (int _i = _startIndex; _i < _rowCount; _i++)
            {
                TableRow _row = GetRowByIndex(_i);
                if(_row != null)
                {
                    _rows.Add(_row);
                }

                else
                {
                    #if UNITY_EDITOR
                    string _message = string.Format("Index: {0} is out of bounds!", _i);
                    Debug.LogError(_message);
                    OnLogUpdate?.Invoke(null, new LogArgs(_message, false));
                    #endif
                    return null;
                }
            }

            return _rows;
        }

        public ITableColumn GetColumn(string _name)
        {
            int _count = ColumnCount;
            for (int _i = 0; _i < _count; _i++)
            {
                if (columns[_i].ColumnName == _name)
                {
                    return columns[_i];
                }
            }

            return null;
        }

        public ITableColumn GetColumnByIndex(int _index)
        {
            if(!ColumnAtIndexExists(_index))
            {
                return null;
            }

            return columns[_index];
        }

        /// <summary>
        /// Get column index by searching for it with its name
        /// </summary>
        /// <param name="_name">Column name</param>
        /// <returns>-1 if column not found.</returns>
        public int GetColumnIndex(string _name)
        {
            int _count = ColumnCount;
            for (int _i = 0; _i < _count; _i++)
            {
                if(columns[_i].ColumnName == _name)
                {
                    return _i;
                }
            }

            return -1;
        }

        #endregion Get

        ////////////////////////////////////////////////////////////////////////

        #region Load Save Table

        /// <summary>
        /// Load a table from path using -> System.IO.File.ReadAllText
        /// </summary>
        public static Table LoadTable(string _url)
        {
            try
            {
                if (!string.IsNullOrEmpty(_url))
                {
                    string _text = System.IO.File.ReadAllText(_url);
                    if (!string.IsNullOrEmpty(_text))
                    {
                        return JsonUtility.FromJson(_text, typeof(Table)) as Table;
                    }
                }
            }
            catch (Exception _e)
            {
                Debug.LogError(_e.Message);
            }

            return null;
        }

        /// <summary>
        /// Load a table from unitys Application.persistentDataPath using -> System.IO.File.ReadAllText
        /// </summary>
        /// <param name="_tableName"></param>
        /// <returns></returns>
        public static Table LoadTablePersistentPath(string _tableName)
        {
            try
            {
                if (!string.IsNullOrEmpty(_tableName))
                {
                    string _shortPath = string.Empty;
                    #if UNITY_EDITOR
                    _shortPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    #else
                    _shortPath = Application.persistentDataPath;
                    #endif

                    string _fullPath = _shortPath + "/" + _tableName + ".json";
                    string _text = System.IO.File.ReadAllText(_fullPath);
                    if (!string.IsNullOrEmpty(_text))
                    {
                        return JsonUtility.FromJson(_text, typeof(Table)) as Table;
                    }
                }
            }
            catch (Exception _e)
            {
                Debug.LogError(_e.Message);
            }

            return null;
        }

        /// <summary>
        /// Save a table to a path using -> System.IO.File.WriteAllText
        /// </summary>
        public static bool SaveTable(string _url, Table _table)
        {
            try
            {
                System.IO.File.WriteAllText(_url, JsonUtility.ToJson(_table));
            }
            catch (Exception _e)
            {
                Debug.LogError(_e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save a table to the root of persistent data path using -> System.IO.File.WriteAllText
        /// Will save to desktop if in editor, so you can easily access the file.
        /// </summary>
        public static bool SaveTablePersistentPath(string _tableName, Table _table)
        {
            try
            {
                string _shortPath = string.Empty;
                #if UNITY_EDITOR
                _shortPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                #else
                _shortPath = Application.persistentDataPath;
                #endif

                string _fullPath = _shortPath + "/" + _tableName + ".json";
                System.IO.File.WriteAllText(_fullPath, JsonUtility.ToJson(_table));
            }
            catch (Exception _e)
            {
                Debug.LogError(_e.Message);
                return false;
            }

            return true;
        }

        #endregion Load Save Table

        ////////////////////////////////////////////////////////////////////////
    }
}
