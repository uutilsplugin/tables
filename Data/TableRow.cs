using System.Collections.Generic;
using System.Linq;

namespace UUtils.Utilities.Data
{
    /// <summary>
    /// Contains all columns in a single row
    /// </summary>
    public class TableRow
    {
        ////////////////////////////////////////////////////////////////////////

        #region Variables

        /// <summary>
        /// Column to which this row value belong to
        /// </summary>
        public List<ITableRowValue> RowColumns { get; private set; }

        /// <summary>
        /// Rows index in the table.
        /// Returns -1 if RowColumns are missing.
        /// </summary>
        public int Index 
        {
            get
            {
                int _count = RowColumns.Count;
                for (int _i = 0; _i < _count; _i++)
                {
                    if(RowColumns[_i] != null)
                    {
                        return RowColumns[_i].Index;
                    }
                }

                return -1;
            }
        
        }

        #endregion Variables

        ////////////////////////////////////////////////////////////////////////

        #region Constructor

        /// <summary>
        /// Create a table row with columns
        /// </summary>
        /// <param name="_columns"></param>
        public TableRow(List<ITableRowValue> _columns)
        {
            RowColumns = _columns;
        }

        #endregion Constructor

        ////////////////////////////////////////////////////////////////////////

        #region Get

        public ITableRowValue GetColumn(string _name)
        {
            int count = RowColumns.Count;
            for (int i = 0; i < count; i++)
            {
                if(RowColumns[i].ColumnName == _name)
                {
                    return RowColumns[i];
                }
            }

            return null;
        }

        #endregion Get

        ////////////////////////////////////////////////////////////////////////

        #region Check Columns

        public bool HasAllColumnsEmpty()
        {
            return RowColumns.Count(_x => _x.Value == string.Empty) == RowColumns.Count;
        }

        #endregion Check Columns

        ////////////////////////////////////////////////////////////////////////
    }
}
