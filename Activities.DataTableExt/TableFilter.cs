using BR.Core.Attributes;
using Activities.DataTableExt.Properties;
using System.Data;

namespace Namespace_DataTableExt
{
    [LocalizableScreenName(nameof(Resources.TableFilter_ScreenName), typeof(Resources))]
    [BR.Core.Attributes.Path("DataTableExt")]
    public class TableFilter : BR.Core.Activity
    {
        [LocalizableScreenName(nameof(Resources.InputTable_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.InputTable_Description), typeof(Resources))]
        [IsRequired]
        public System.Data.DataTable InputTable {get; set;} 
        
        [LocalizableScreenName(nameof(Resources.Query_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.Query_Description), typeof(Resources))]
        [IsRequired]
        public System.String Query {get; set;} 
        
        [LocalizableScreenName(nameof(Resources.OutTable_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.OutTable_Description), typeof(Resources))]
        [IsOut]
        public System.Data.DataTable OutTable { get; set; }

        public override void Execute(int? optionID)
        {
            // Проверка наличия входных данных
            if (InputTable == null)
            {
                throw new ArgumentNullException(nameof(InputTable), "Входная таблица не задана.");
            }

            if (string.IsNullOrEmpty(Query))
            {
                throw new ArgumentNullException(nameof(Query), "Запрос не задан.");
            }

            // Применяем фильтрацию
            OutTable = ApplyFilter(Query, InputTable);
        }

        // Функция для применения фильтрации к таблице данных
        private DataTable ApplyFilter(string sqlQuery, DataTable table)
        {
            // Клонируем структуру таблицы
            DataTable filteredTable = table.Clone();

            // Применяем фильтр к таблице с помощью LINQ
            DataRow[] filteredRows = table.Select(sqlQuery);
            var filteredQuery = filteredRows.AsEnumerable();

            // Импортируем строки в новую таблицу
            filteredQuery.ToList().ForEach(row => filteredTable.ImportRow(row));

            return filteredTable;
        }
    }
}
