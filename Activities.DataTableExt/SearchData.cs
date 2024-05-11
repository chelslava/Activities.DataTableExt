using System.Data;
using System.Text.RegularExpressions;
using BR.Core.Attributes;
using Activities.DataTableExt.Properties;

namespace Activities.DataTableExt
{
    // Активность для поиска данных в таблице по заданным критериям
    [LocalizableScreenName(nameof(Resources.TableSearch_ScreenName), typeof(Resources))]
    [BR.Core.Attributes.Path("DataTableExt")]
    public class TableSearch : BR.Core.Activity
    {
        // Входная таблица
        [LocalizableScreenName(nameof(Resources.SourceTable_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.SourceTable_Description), typeof(Resources))]
        [IsRequired]
        public DataTable SourceTable { get; set; }

        // Критерии поиска
        [LocalizableScreenName(nameof(Resources.Criteria_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.Criteria_Description), typeof(Resources))]
        [IsRequired]
        public string Criteria { get; set; }

        // Новое свойство для указания колонки или списка колонок, по которым следует проводить поиск
        [LocalizableScreenName(nameof(Resources.ColumnsToSearch_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.ColumnsToSearch_Description), typeof(Resources))]
        public IEnumerable<string> ColumnsToSearch { get; set; }

        // Использовать регулярное выражение
        [LocalizableScreenName(nameof(Resources.UseRegex_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.UseRegex_Description), typeof(Resources))]
        [IsRequired]
        public bool UseRegex { get; set; }

        // Учитывать регистр символов при поиске
        [LocalizableScreenName(nameof(Resources.IgnoreCase_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.IgnoreCase_Description), typeof(Resources))]
        public bool IgnoreCase { get; set; }

        // Многострочный режим при поиске
        [LocalizableScreenName(nameof(Resources.Multiline_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.Multiline_Description), typeof(Resources))]
        public bool Multiline { get; set; }

        // Искать первое найденное соответствие
        [LocalizableScreenName(nameof(Resources.SearchFirst_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.SearchFirst_Description), typeof(Resources))]
        [IsRequired]
        public bool SearchFirst { get; set; }

        // Результаты поиска
        [LocalizableScreenName(nameof(Resources.SearchResults_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.SearchResults_Description), typeof(Resources))]
        [IsOut]
        public List<(int RowIndex, int ColumnIndex, object Value)> SearchResults { get; set; }

        // Было ли найдено хотя бы одно соответствие
        [LocalizableScreenName(nameof(Resources.AnyMatch_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.AnyMatch_Description), typeof(Resources))]
        [IsOut]
        public bool AnyMatch { get; set; }

        // Метод выполнения активности
        public override void Execute(int? optionID)
        {
            try
            {
                SearchResults = new List<(int, int, object)>();

                if (SearchFirst)
                {
                    ExecuteSearchFirst();
                }
                else
                {
                    ExecuteSearchAll();
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки или выброс исключения
                Console.WriteLine($"Ошибка выполнения поиска: {ex.Message}");
            }
        }

        // Выполнение поиска первого совпадения
        private void ExecuteSearchFirst()
        {
            var rows = SourceTable.AsEnumerable();
            var result = rows.Select(GetFirstMatchedColumnIndex)
                             .FirstOrDefault(columnIndex => columnIndex != -1);
            if (result != -1)
            {
                var row = rows.First();
                SearchResults.Add((row.Table.Rows.IndexOf(row), result, row[result]));
                AnyMatch = true;
            }
            else
            {
                AnyMatch = false;
            }
        }

        // Выполнение поиска всех совпадений
        private void ExecuteSearchAll()
        {
            var rows = SourceTable.AsEnumerable();
            var columnsToSearch = GetColumnsToSearch(); // Получаем колонки для поиска
            var results = rows.SelectMany(row => GetMatchedColumns(row, columnsToSearch))
                              .ToList(); // Убрали лишний вызов Select, чтобы сохранить все столбцы
            SearchResults.AddRange(results);
            AnyMatch = SearchResults.Any();
        }

        // Получение колонок для поиска в зависимости от переданных имен
        private IEnumerable<DataColumn> GetColumnsToSearch()
        {
            if (ColumnsToSearch == null) // Если параметр не задан, ищем по всем колонкам
            {
                return SourceTable.Columns.Cast<DataColumn>();
            }
            else
            {
                // Получаем колонки по именам
                return ColumnsToSearch.Select(columnName => SourceTable.Columns[columnName]);
            }
        }


        // Получение индекса первой совпавшей колонки
        private int GetFirstMatchedColumnIndex(DataRow row)
        {
            return row.Table.Columns
                .Cast<DataColumn>()
                .Select((col, index) => (col, index))
                .FirstOrDefault(pair => IsMatch(row[pair.index])).index;
        }

        // Получение списка всех совпавших колонок
        private IEnumerable<(int RowIndex, int ColumnIndex, object Value)> GetMatchedColumns(DataRow row, IEnumerable<DataColumn> columnsToSearch)
        {
            return columnsToSearch
                .Select(col => (RowIndex: row.Table.Rows.IndexOf(row),
                                ColumnIndex: col.Ordinal,
                                Value: row[col]))
                .Where(pair => IsMatch(pair.Value));
        }

        // Проверка совпадения значения с критерием поиска
        private bool IsMatch(object value)
        {
            if (string.IsNullOrWhiteSpace(Criteria) || value == null)
                return false;

            if (UseRegex)
            {
                var options = RegexOptions.None;
                if (IgnoreCase)
                    options |= RegexOptions.IgnoreCase;
                if (Multiline)
                    options |= RegexOptions.Multiline;

                var regex = new Regex(Criteria, options);
                return regex.IsMatch(value.ToString());
            }
            else
            {
                var criterion = Criteria.Replace("*", ".*").Replace("%", ".*");
                return value.ToString().Contains(criterion);
            }
        }
    }
}