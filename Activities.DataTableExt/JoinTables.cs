using Activities.DataTableExt.Properties;
using BR.Core.Attributes;
using System.Data;

namespace Activities.DataTableExt
{
    // Активность для объединения двух таблиц данных
    [LocalizableScreenName("", typeof(Resources))]
    [BR.Core.Attributes.Path("DataTableExt")]
    public class JoinTables : BR.Core.Activity
    {
        // Входная таблица 1
        [LocalizableScreenName(nameof(Resources.Table1_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.Table1_Description), typeof(Resources))]
        [IsRequired]
        public DataTable Table1 { get; set; }

        // Входная таблица 2
        [LocalizableScreenName(nameof(Resources.Table2_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.Table2_Description), typeof(Resources))]
        [IsRequired]
        public DataTable Table2 { get; set; }

        // Тип объединения (Inner, Left, Right, Full)
        [LocalizableScreenName(nameof(Resources.JoinType_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.JoinType_Description), typeof(Resources))]
        [IsRequired]
        public string JoinType { get; set; }

        // Условие объединения
        [LocalizableScreenName(nameof(Resources.JoinCondition_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.JoinCondition_Description), typeof(Resources))]
        [IsRequired]
        public string JoinCondition { get; set; }

        // Результирующая таблица
        [LocalizableScreenName(nameof(Resources.ResultTable_ScreenName), typeof(Resources))]
        [LocalizableDescription(nameof(Resources.ResultTable_Description), typeof(Resources))]
        [IsOut]
        public DataTable ResultTable { get; set; }

        // Метод выполнения активности
        public override void Execute(int? optionID)
        {
            // Проверяем наличие входных данных
            if (Table1 == null || Table2 == null)
            {
                throw new ArgumentNullException("Table1 или Table2", "Необходимо задать обе таблицы для объединения.");
            }

            if (string.IsNullOrEmpty(JoinType))
            {
                throw new ArgumentNullException("JoinType", "Необходимо задать тип объединения (Inner, Left, Right, Full).");
            }

            // Инициализируем ResultTable перед выполнением операции объединения
            ResultTable = new DataTable();

            // Выполняем операцию объединения таблиц
            ResultTable = JoinType.ToLower() switch
            {
                "inner" => InnerJoin(),
                "left" => LeftJoin(),
                "right" => RightJoin(),
                "full" => FullOuterJoin(),
                _ => throw new ArgumentException("Недопустимое значение для типа объединения."),
            };
        }

        // Метод для выполнения внутреннего объединения таблиц
        private DataTable InnerJoin()
        {
            // Выполняем внутреннее объединение таблиц
            var query = from row1 in Table1.AsEnumerable()
                        join row2 in Table2.AsEnumerable() on row1[JoinCondition] equals row2[JoinCondition]
                        select JoinRows(row1, row2);

            return query.CopyToDataTable();
        }

        // Метод для выполнения левого объединения таблиц
        private DataTable LeftJoin()
        {
            // Выполняем левое объединение таблиц
            var query = from row1 in Table1.AsEnumerable()
                        join row2 in Table2.AsEnumerable() on row1[JoinCondition] equals row2[JoinCondition] into joined
                        from row2 in joined.DefaultIfEmpty()
                        select JoinRows(row1, row2);

            return query.CopyToDataTable();
        }

        // Метод для выполнения правого объединения таблиц
        private DataTable RightJoin()
        {
            // Выполняем правое объединение таблиц
            var query = from row2 in Table2.AsEnumerable()
                        join row1 in Table1.AsEnumerable() on row2[JoinCondition] equals row1[JoinCondition] into joined
                        from row1 in joined.DefaultIfEmpty()
                        select JoinRows(row1, row2);

            return query.CopyToDataTable();
        }

        // Метод для выполнения полного внешнего объединения таблиц
        private DataTable FullOuterJoin()
        {
            // Выполняем полное внешнее объединение таблиц
            var query = from row1 in Table1.AsEnumerable()
                        join row2 in Table2.AsEnumerable() on row1[JoinCondition] equals row2[JoinCondition] into joined
                        from row2 in joined.DefaultIfEmpty()
                        select JoinRows(row1, row2);

            // Добавляем строки из Table2, которые не были найдены при первом объединении
            query = query.Union(from row2 in Table2.AsEnumerable()
                                join row1 in Table1.AsEnumerable() on row2[JoinCondition] equals row1[JoinCondition] into joined
                                from row1 in joined.DefaultIfEmpty()
                                where row1 == null
                                select JoinRows(row1, row2));

            return query.CopyToDataTable();
        }

        // Метод для объединения строк из двух таблиц в одну строку
        private DataRow JoinRows(DataRow row1, DataRow row2)
        {
            DataRow newRow = ResultTable.NewRow();

            // Копируем значения из первой таблицы, если row1 не равен null
            if (row1 != null)
            {
                foreach (DataColumn column in Table1.Columns)
                {
                    if (!ResultTable.Columns.Contains(column.ColumnName))
                    {
                        ResultTable.Columns.Add(column.ColumnName, column.DataType);
                    }

                    newRow[column.ColumnName] = row1[column.ColumnName];
                }
            }

            // Копируем значения из второй таблицы, если row2 не равен null
            if (row2 != null)
            {
                foreach (DataColumn column in Table2.Columns)
                {
                    if (!ResultTable.Columns.Contains(column.ColumnName))
                    {
                        ResultTable.Columns.Add(column.ColumnName, column.DataType);
                    }

                    newRow[column.ColumnName] = row2[column.ColumnName];
                }
            }

            return newRow;
        }

    }
}
