using UnityEngine;

/// <summary>
/// Tables API namespace.
/// </summary>
namespace UUtils.Utilities.Data
{
    /// <summary>
    /// Contains a Table field.
    /// Permanent storage.
    /// Don't forget, changes made to this during gameplay in a build are not
    /// saved to the scriptable object so you have to serialize the table and
    /// reload it if you intend to make changes.
    /// </summary>
    [CreateAssetMenu(fileName = "Table_", menuName = "UUtils/Table")]
    public class TableSO : ScriptableObject
    {
        ////////////////////////////////////////////////////////////////////////

        #region Table

        /// <summary>
        /// Initialized empty table.
        /// </summary>
        public Table Table = new Table(-1, string.Empty);

        #endregion Table

        ////////////////////////////////////////////////////////////////////////
    }
}
