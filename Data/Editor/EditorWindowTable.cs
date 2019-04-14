using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace UUtils.Utilities.Data
{
    /// <summary>
    /// Edits a single table
    /// </summary>
    public class EditorWindowTable : EditorWindow, ISerializationCallbackReceiver
    {
        ////////////////////////////////////////////////////////////////////////

        #region View

        [System.Serializable]
        enum CurrentView
        {
            Convert,
            TableBrowse,
            TableStructure,
            EditRow
        }

        [SerializeField]
        private CurrentView currentView = CurrentView.TableBrowse;

        #endregion View

        ////////////////////////////////////////////////////////////////////////

        #region Vars

        /// <summary>
        /// Current scrolling position
        /// </summary>
        [SerializeField]
        private Vector2 scrollPosition;

        /// <summary>
        /// Current scrolling position for log
        /// </summary>
        [SerializeField]
        private Vector2 scrollPositionLog;

        /// <summary>
        /// Style used for displaying row values.
        /// Will center text inside.
        /// </summary>
        private GUIStyle style;

        /// <summary>
        /// Text distance for selecting a tableSO
        /// </summary>
        private float labelDistance;

        private GUIContent saveContent = new GUIContent(
            "Save JSON",
            "Saves the table inside of the currently loaded TableSO as a .json file"
        );

        private GUIContent saveContentReload = new GUIContent(
            "Save JSON And Reload",
            "Saves the table inside of the currently loaded TableSO as a .json file and reloads the editor."
        );

        #endregion Vars

        ////////////////////////////////////////////////////////////////////////

        #region Page

        private string[] optionsPerPageString = { "10", "25", "50", "100", "250" };

        private int[] optionsPerPageInt = { 10, 25, 50, 100, 250 };

        [SerializeField]
        private int currentPerPageValue = 10;

        [SerializeField]
        private int currentPerPageValuePrevious = 10;

        private int currentPage;

        /// <summary>
        /// How many pages before and after current page each will be shown
        /// </summary>
        private const int pagination = 5;

        #endregion Page

        ////////////////////////////////////////////////////////////////////////

        #region Log 

        private Color colorLogSuccess = Color.green;

        private Color colorLogFail = Color.red;

        /// <summary>
        /// Contains all log messages
        /// </summary>
        [SerializeField]
        private List<LogArgs> logs = new List<LogArgs>();

        private int logCount;

        #endregion Log

        ////////////////////////////////////////////////////////////////////////

        #region Table

        /// <summary>
        /// Used ONLY to load the table.
        /// </summary>
        [SerializeField]
        private TableSO loadTableSO;

        /// <summary>
        /// Editing table scriptable object if selected.
        /// This or _textTable can be used to set _table.
        /// Table.cs will ALWAYS be accessed directly through this reference
        /// and will never have a separate field for itself because of unity's serialization.
        /// Always reference an SO if you want to have data survive the reload and be able to edit that data,
        /// never the object inside of it.
        /// </summary>
        [SerializeField]
        private TableSO tableSO;

        /// <summary>
        /// Editing JSON serialized table if selected.
        /// This or _tableSO can be used to set _table.
        /// </summary>
        [SerializeField]
        private TextAsset textTable;

        #endregion Table

        ////////////////////////////////////////////////////////////////////////

        #region Import

        /// <summary>
        /// Can be used to import data into a new scriptable object
        /// </summary>
        [SerializeField]
        private TableSO importTable;

        #endregion Import

        ////////////////////////////////////////////////////////////////////////

        #region Edit Row

        /// <summary>
        /// Currently editing table row.
        /// Must store index and not reference due to serialization.
        /// Reference would be lost if editor is reloaded.
        /// </summary>
        [SerializeField]
        private int currentRowIndex = -1;

        #endregion Edit Row

        ////////////////////////////////////////////////////////////////////////

        #region New Column

        [SerializeField]
        private string newColumnName;

        #endregion New Column

        ////////////////////////////////////////////////////////////////////////

        #region Window

        [MenuItem("Tools/UUtils/Table Editor")]
        public static void ShowWindow()
        {
            GetWindow<EditorWindowTable>("Table");
        }

        private void OnGUI()
        {
            if (tableSO != null)
            {
                Undo.RecordObject(tableSO, "Undo");
            }

            SetStyle();

            CreateTableNavigationBar();

            SelectTableSO();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);

            DrawTableLayout();

            GUILayout.EndScrollView();

            if (tableSO != null)
            {
                EditorUtility.SetDirty(tableSO);
            }
        }

        private void Update()
        {
            Repaint();
        }

        private void OnEnable()
        {
            if (tableSO != null && tableSO.Table != null)
            {
                tableSO.Table.OnLogUpdate -= OnTableUpdate;
                tableSO.Table.OnLogUpdate += OnTableUpdate;
            }
        }

        private void OnDestroy()
        {
            if (tableSO != null && tableSO.Table != null)
            {
                tableSO.Table.OnLogUpdate -= OnTableUpdate;
            }
        }

        /// <summary>
        /// Unsubscribe before reloading editor.
        /// </summary>
        public void OnBeforeSerialize()
        {
            if (tableSO != null && tableSO.Table != null)
            {
                tableSO.Table.OnLogUpdate -= OnTableUpdate;
            }
        }

        /// <summary>
        /// Subscribe back to logs if table exists
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (tableSO != null && tableSO.Table != null)
            {
                tableSO.Table.OnLogUpdate += OnTableUpdate;
            }
        }

        #endregion Window

        ////////////////////////////////////////////////////////////////////////

        #region Style

        private void SetStyle()
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label);
                style.alignment = TextAnchor.MiddleCenter;
                style.wordWrap = true;
            }
        }

        #endregion Style

        ////////////////////////////////////////////////////////////////////////

        #region Bar & Layout

        private void CreateTableNavigationBar()
        {
            // Contains navigation buttons and log 
            EditorGUILayout.BeginHorizontal();

            GUIStyle _style = EditorStatics.GetBoxStyle(10, 0, 10, 10, 15, 15, 15, 15);
            EditorGUILayout.BeginVertical(_style);

            // Contains navigation buttons
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Convert", EditorStatics.Width_100))
            {
                LoadConvertInterface();
            }

            if (GUILayout.Button("Browse", EditorStatics.Width_100))
            {
                LoadTableBrowse();
            }

            if (GUILayout.Button("Structure", EditorStatics.Width_100))
            {
                LoadTableStructure();
            }

            // Contains navigation buttons 
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            DisplayLog();

            // Contains navigation buttons and log 
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (tableSO != null && tableSO && tableSO.Table != null)
            {
                if (currentView == CurrentView.TableBrowse)
                {
                    //DrawTableBrowseCreateRowField();
                }

                if (currentView == CurrentView.TableStructure)
                {
                    //DrawTableStructureCreateField();
                }
            }
        }

        private void LoadConvertInterface()
        {
            currentView = CurrentView.Convert;
        }

        private void LoadTableBrowse()
        {
            currentView = CurrentView.TableBrowse;
            currentRowIndex = -1;
        }

        private void LoadTableStructure()
        {
            currentView = CurrentView.TableStructure;
            currentRowIndex = -1;
        }

        private void DrawTableLayout()
        {
            if (currentView == CurrentView.Convert)
            {
                DrawInterfaceConvert();
                return;
            }

            if (tableSO == null || !tableSO || tableSO.Table == null)
            {
                return;
            }

            GUIStyle _style = EditorStatics.GetBoxStyle(10, 10, 0, 10, 15, 15, 15, 15);
            EditorGUILayout.BeginVertical(_style);

            if (currentView == CurrentView.TableBrowse)
            {
                DrawTableBrowseCreateRowField();
                DrawColumns();
                DrawRows();
            }

            else if (currentView == CurrentView.TableStructure)
            {
                DrawTableStructure();
            }

            else if (currentView == CurrentView.EditRow)
            {
                DrawTableRow();
            }

            EditorGUILayout.EndVertical();
        }

        #endregion Bar & Layout

        ////////////////////////////////////////////////////////////////////////

        #region Select SO

        private void SelectTableSO()
        {
            if (currentView == CurrentView.Convert)
            {
                return;
            }

            EditorGUILayout.Space();

            GUIStyle _style = EditorStatics.GetBoxStyle(10, 10, 0, 10, 15, 15, 15, 15, 390);
            EditorGUILayout.BeginVertical(_style);

            labelDistance = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;

            loadTableSO = (TableSO)EditorGUILayout.ObjectField(
                "Table SO",
                loadTableSO,
                typeof(TableSO),
                true,
                EditorStatics.Width_210
            );

            EditTableInfo();

            EditorGUIUtility.labelWidth = labelDistance;

            EditorGUILayout.Space();

            // Buttons
            EditorGUILayout.BeginHorizontal();

            EditTableLoad();

            InterfaceSave();

            // Buttons
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
        }

        private void EditTableInfo()
        {
            if (tableSO != null)
            {
                if (tableSO.Table != null)
                {
                    tableSO.Table.Name = EditorStatics.CreateTextField(
                        "Name",
                        "",
                        tableSO.Table.Name,
                        EditorStatics.Width_210
                    );
                }
            }
        }

        private void EditTableLoad()
        {
            if (GUILayout.Button("Load", EditorStatics.Width_70))
            {
                if (loadTableSO != null && loadTableSO)
                {
                    if (loadTableSO.Table != null)
                    {
                        // Unsubscribe from previous table
                        if (tableSO != null && tableSO && tableSO.Table != null)
                        {
                            tableSO.Table.OnLogUpdate -= OnTableUpdate;
                        }

                        tableSO = loadTableSO;
                        tableSO.Table.OnLogUpdate += OnTableUpdate;
                        LoadTableBrowse();

                        // Clear
                        loadTableSO = null;
                        CreateLog("Loaded table: " + tableSO.Table.Name, true);
                    }
                }
            }
        }

        #endregion Select SO

        ////////////////////////////////////////////////////////////////////////

        #region Convert

        private void DrawInterfaceConvert()
        {
            GUIStyle _style = EditorStatics.GetBoxStyle(10, 00, 0, 10, 15, 15, 15, 15, 350);
            EditorGUILayout.BeginVertical(_style);

            textTable = (TextAsset)EditorGUILayout.ObjectField(
                "JSON Table",
                textTable,
                typeof(TextAsset),
                true,
                EditorStatics.Width_300
            );

            importTable = (TableSO)EditorGUILayout.ObjectField(
                "Import SO",
                importTable,
                typeof(TableSO),
                true,
                EditorStatics.Width_300
            );

            if(GUILayout.Button("Import", EditorStatics.Width_70))
            {
                if(importTable == null || !importTable)
                {
                    CreateLog("Import table reference not set!", false);
                    return;
                }

                if(textTable != null && textTable)
                {
                    if(!string.IsNullOrEmpty(textTable.text))
                    {
                        try
                        {
                            Table table = JsonUtility.FromJson<Table>(textTable.text);
                            if (table != null)
                            {
                                importTable.Table = table;
                                EditorUtility.SetDirty(importTable);

                                importTable = null;
                                textTable = null;
                                CreateLog("JSON successfully loaded into the export table SO!", true);
                            }
                        }
                        catch (System.Exception _e)
                        {
                            CreateLog("Unable to parse JSON file as a Table!", false);
                        }
                    }

                    else
                    {
                        CreateLog("JSON file contains no data!", false);
                    }
                }

                else
                {
                    CreateLog("JSON file reference not set!", false);
                }
            }

            EditorGUILayout.EndVertical();
        }

        #endregion Convert

        ////////////////////////////////////////////////////////////////////////

        #region Table Browse

        private void DrawTableBrowseCreateRowField()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Insert Row", EditorStatics.Width_100))
            {
                tableSO.Table.InsertRow();
            }

            if (GUILayout.Button("Clear All", EditorStatics.Width_100))
            {
                string _title = "Clear All Rows";
                string _text = "Are you sure you want to remove all rows?";
                if (EditorUtility.DisplayDialog(_title, _text, "Confirm", "Cancel"))
                {
                    tableSO.Table.RemoveAllRows();
                    currentPage = 0;
                }
            }

            DrawPaginationBar();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        private void DrawPaginationBar()
        {
            GUILayout.FlexibleSpace();

            int _count = tableSO.Table.GetPageCount(currentPerPageValue);

            int _paginationStart = Mathf.Max(0, currentPage - pagination);
            int _paginationEnd = Mathf.Min(_count, currentPage + pagination);
            for (int i = _paginationStart; i < _paginationEnd; i++)
            {
                if(i != currentPage)
                {
                    if (GUILayout.Button((i + 1).ToString(), EditorStatics.Width_27))
                    {
                        currentPage = i;
                    }
                }
            }

            EditorStatics.CreateLabelField(string.Empty, EditorStatics.Width_30);

            labelDistance = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 60;

            currentPerPageValue = EditorStatics.CreateIntPopup(
                "Per Page",
                currentPerPageValue,
                optionsPerPageString,
                optionsPerPageInt,
                EditorStatics.Width_140
            );

            if(currentPerPageValuePrevious != currentPerPageValue)
            {
                currentPage = 0;
                currentPerPageValuePrevious = currentPerPageValue;
            }

            EditorGUIUtility.labelWidth = labelDistance;
        }

        private void DrawColumns()
        {
            GUIStyle _style = EditorStatics.GetBoxStyle(0, 0, 2, 2, 2, 2, 5, 5);
            EditorGUILayout.BeginVertical(_style);

            EditorGUILayout.BeginHorizontal();

            // Edit row (Width_50), insert (Width_50), remove (Width_50) width in DrawRows()
            EditorStatics.CreateLabelField("", EditorStatics.Width_150);
            // Spacing fix
            EditorStatics.CreateLabelField("", EditorStatics.Width_5);

            // Shift row up and down (Width_20) width in DrawRows()
            EditorStatics.CreateLabelField("", EditorStatics.Width_20);
            EditorStatics.CreateLabelField("", EditorStatics.Width_20);

            EditorStatics.CreateLabelField("#", EditorStatics.Width_50);

            int _count = tableSO.Table.ColumnCount;
            for (int _i = 0; _i < _count; _i++)
            {
                EditorStatics.CreateLabelField(
                    tableSO.Table.ColumnNames[_i].ShorterText(10, true),
                    EditorStatics.Width_80,
                    style
                );

                // Push the next one
                EditorStatics.CreateLabelField("", EditorStatics.Width_5);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawRows()
        {
            int _startIndex = currentPage * currentPerPageValue;
            List<TableRow> _rows = tableSO.Table.GetRows(_startIndex, _startIndex + currentPerPageValue, true);

            if(_rows == null)
            {
                return;
            }

            int _count = _rows.Count;
            for (int _i = 0; _i < _count; _i++)
            {
                TableRow _row = _rows[_i];
                int _rowIndex = _startIndex + _i;

                GUIStyle _style = EditorStatics.GetBoxStyle(0, 0, 0, 0, 2, 2, 2, 2, 0, 30);
                EditorGUILayout.BeginVertical(_style);

                EditorGUILayout.BeginHorizontal();

                // Insert a row before this one
                if (GUILayout.Button(new GUIContent("Insert", "Insert a row before this one"), EditorStatics.Width_50))
                {
                    tableSO.Table.InsertRow(_rowIndex);
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                // Insert a row before this one
                if (GUILayout.Button("Delete", EditorStatics.Width_50))
                {
                    tableSO.Table.RemoveRow(_rowIndex);
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                if (GUILayout.Button("Edit", EditorStatics.Width_50))
                {
                    EditTableRow(_rowIndex);
                }

                // Shift row to previous index
                if (GUILayout.Button(EditorStatics.StringArrowUp, EditorStatics.Width_20))
                {
                    int _nextIndex = _rowIndex - 1;
                    if (tableSO.Table.ShiftRow(_rowIndex, _nextIndex))
                    {
                        Debug.Log(string.Format("Shifted row up. From Index: {0}, To Index: {1}", _rowIndex, _nextIndex));
                    }
                }

                // Shift row to next index
                if (GUILayout.Button(EditorStatics.StringArrowDown, EditorStatics.Width_20))
                {
                    int _nextIndex = _rowIndex + 1;
                    if (tableSO.Table.ShiftRow(_rowIndex, _nextIndex))
                    {
                        Debug.Log(string.Format("Shifted row down. From Index: {0}, To Index: {1}", _rowIndex, _nextIndex));
                    }
                }

                // Display row index
                EditorStatics.CreateLabelField(_rowIndex.ToString(), EditorStatics.Width_50);

                GUIStyle _styleC = EditorStatics.GetBoxStyle(0, 0, 0, 0, 0, 0, 2, 2, 0, 26);
                _styleC.alignment = TextAnchor.MiddleCenter;
                style.wordWrap = true;

                int _columnCount = tableSO.Table.ColumnCount;
                for (int _j = 0; _j < _columnCount; _j++)
                {
                    ITableRowValue _column = _row.RowColumns[_j];

                    EditorStatics.CreateLabelField(
                        _column.Value.ShorterText(10, true),
                        EditorStatics.Width_80,
                        _styleC
                    );

                    // Push the next one
                    EditorStatics.CreateLabelField("", EditorStatics.Width_5);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
        }

        #endregion Table Browse

        ////////////////////////////////////////////////////////////////////////

        #region Structure

        private void DrawTableStructure()
        {
            DrawTableStructureCreateField();

            DrawTableStructureColumns();
        }

        private void DrawTableStructureCreateField()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Create", EditorStatics.Width_100))
            {
                if(tableSO.Table.CreateColumn(newColumnName))
                {
                    newColumnName = string.Empty;
                    GUI.FocusControl(null);
                }
            }

            // Spacing
            EditorStatics.CreateLabelField("", EditorStatics.Width_10);

            newColumnName = EditorStatics.CreateTextField(
                "Column Name",
                "",
                newColumnName,
                EditorStatics.Width_300
            );

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        private void DrawTableStructureColumns()
        {
            int _columnCount = tableSO.Table.ColumnCount;
            for (int _i = 0; _i < _columnCount; _i++)
            {
                if (_i != 0)
                {
                    EditorGUILayout.Space();
                }

                EditorGUILayout.BeginHorizontal();

                // Display row index
                EditorStatics.CreateLabelField(_i.ToString(), EditorStatics.Width_50);

                ITableColumn _column = tableSO.Table.GetColumnByIndex(_i);

                EditorStatics.CreateLabelField(
                    _column.ColumnName,
                    EditorStatics.Width_210
                );

                _column.ColumnNameEditor = EditorStatics.CreateTextField(
                    "",
                    "",
                    _column.ColumnNameEditor,
                    EditorStatics.Width_300
                );

                ///Calls update method which will invoke OnRowUpdated event
                if (GUILayout.Button("Update", EditorStatics.Width_80))
                {
                    if (!string.IsNullOrEmpty(_column.ColumnNameEditor))
                    {
                        if(tableSO.Table.UpdateColumnName(_column.ColumnNameEditor, _column))
                        {
                            _column.ColumnNameEditor = string.Empty;
                        }
                        GUI.FocusControl(null);
                    }
                }

                if (GUILayout.Button("Delete", EditorStatics.Width_80))
                {
                    tableSO.Table.RemoveColumn(_column);
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion Structure

        ////////////////////////////////////////////////////////////////////////

        #region Edit Table Row

        private void EditTableRow(int _index)
        {
            currentRowIndex = _index;

            currentView = CurrentView.EditRow;
        }

        private void DrawTableRow()
        {
            if (currentRowIndex < 0)
            {
                return;
            }

            TableRow _currentRow = tableSO.Table.GetRowByIndex(currentRowIndex);

            int _columnCount = tableSO.Table.ColumnCount;
            for (int _i = 0; _i < _columnCount; _i++)
            {
                if(_i != 0)
                {
                    EditorGUILayout.Space();
                }

                EditorGUILayout.BeginHorizontal();

                // Display row index
                EditorStatics.CreateLabelField(_i.ToString(), EditorStatics.Width_50);

                ITableRowValue _column = _currentRow.RowColumns[_i];

                EditorStatics.CreateLabelField(
                    _column.ColumnName,
                    EditorStatics.Width_210
                );

                _column.Value = EditorGUILayout.TextArea(
                    _column.Value,
                    GUILayout.MinHeight(100),
                    GUILayout.MaxWidth(460)
                );

                ///Calls update method which will invoke OnRowUpdated event
                if (GUILayout.Button("Update", EditorStatics.Width_80))
                {
                    _column.Update(_column.Value);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion Edit Table Row

        ////////////////////////////////////////////////////////////////////////

        #region Log

        private void DisplayLog()
        {
            GUILayout.FlexibleSpace();

            GUIStyle _style = EditorStatics.GetBoxStyle(20, 10, 10, 10, 15, 15, 15, 15, 600, 100);
            EditorGUILayout.BeginVertical(_style);

            scrollPositionLog = GUILayout.BeginScrollView(scrollPositionLog, false, false);

            logCount = logs.Count;
            for (int _i = logCount - 1; _i >= 0; _i--)
            {
                EditorStatics.CreateLabelField(
                    logs[_i].LogMessage,
                    EditorStatics.Width_500,
                    logs[_i].ActionStatus ? colorLogSuccess : colorLogFail
                );
            }

            GUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void OnTableUpdate(object _sender, LogArgs _args)
        {
            logs.Add(_args);
        }

        private void CreateLog(string _msg, bool _success)
        {
            logs.Add(new LogArgs(_msg, _success));
        }

        #endregion Log

        ////////////////////////////////////////////////////////////////////////

        #region Save

        private void InterfaceSave()
        {
            if(tableSO == null || tableSO.Table == null)
            {
                return;
            }

            if(GUILayout.Button(saveContent, EditorStatics.Width_100))
            {
                Save(false);
            }

            if (GUILayout.Button(saveContentReload, EditorStatics.Width_180))
            {
                Save(true);
            }
        }

        private void Save(bool _reload)
        {
            string _path = EditorUtility.SaveFilePanel(
                    "Save as JSON",
                    "",
                    "Table.json",
                    "json"
                );

            if (!string.IsNullOrEmpty(_path))
            {
                File.WriteAllText(_path, JsonUtility.ToJson(tableSO.Table));

                if(_reload)
                {
                    AssetDatabase.ImportAsset(_path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                }
            }
        }

        #endregion Save

        ////////////////////////////////////////////////////////////////////////
    }
}
