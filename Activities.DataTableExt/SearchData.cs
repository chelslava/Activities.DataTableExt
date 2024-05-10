using Activities.DataTableExt.Properties;
using BR.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

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

        // Использовать регулярное выражение
        [LocalizableScreenName(nameof(Resources.UseRegex_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.UseRegex_Description), typeof(Resources))]
        [IsRequired]
        public bool UseRegex { get; set; }

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

        // Метод выполнения активности
        public override void Execute(int? optionID)
        {
            try
            {
                // Инициализация списка результатов поиска
                SearchResults = new List<(int, int, object)>();

                // Преобразование таблицы в перечислимую коллекцию строк
                var rows = SourceTable.AsEnumerable();

                // Выполнение поиска
                if (SearchFirst)
                {
                    var result = SearchFirstMatch(rows);
                    if (result != (0, 0, null))
                    {
                        SearchResults.Add(result);
                        AnyMatch = true;
                    }
                    else
                    {
                        AnyMatch = false;
                    }
                }
                else
                {
                    var results = SearchAllMatches(rows);
                    if (results.Any())
                    {
                        SearchResults.AddRange(results);
                        AnyMatch = true;
                    }
                    else
                    {
                        AnyMatch = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка выполнения поиска: {ex.Message}");
            }
        }

        // Поиск первого совпадения
        private (int, int, object) SearchFirstMatch(IEnumerable<DataRow> rows)
        {
            foreach (var row in rows)
            {
                var columnIndex = row.Table.Columns
                    .Cast<DataColumn>()
                    .Select((col, index) => (col, index))
                    .Where(pair => IsMatch(row[pair.index]))
                    .Select(pair => pair.index)
                    .FirstOrDefault();

                if (columnIndex != -1)
                {
                    return (row.Table.Rows.IndexOf(row), columnIndex, row[columnIndex]);
                }
            }
            return (0, 0, null);
        }

        // Поиск всех совпадений
        private List<(int, int, object)> SearchAllMatches(IEnumerable<DataRow> rows)
        {
            var results = new List<(int, int, object)>();
            foreach (var row in rows)
            {
                var matchedColumns = row.Table.Columns
                    .Cast<DataColumn>()
                    .Select((col, index) => (col, index))
                    .Where(pair => IsMatch(row[pair.index]))
                    .ToList();

                foreach (var match in matchedColumns)
                {
                    results.Add((row.Table.Rows.IndexOf(row), match.index, row[match.index]));
                }
            }
            return results;
        }

        // Проверка совпадения значения с критерием поиска
        private bool IsMatch(object value)
        {
            if (value == null || Criteria == null) return false;

            // Использование регулярного выражения для поиска
            if (UseRegex)
            {
                var regex = new Regex(Criteria);
                return regex.IsMatch(value.ToString());
            }
            // Простое сравнение строк или использование "LIKE"
            else
            {
                // Реализация поддержки "*" и "%"
                var criterion = Criteria.Replace("*", ".*").Replace("%", ".*");
                return value.ToString().Contains(criterion);
            }
        }
    }
}