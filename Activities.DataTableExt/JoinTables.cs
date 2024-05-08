using Activities.DataTableExt.Properties;
using BR.Core.Attributes;
using System;
using System.Data;
using System.Linq;

namespace Namespace_DataTableExt
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

            // Выполняем операцию объединения таблиц
            ResultTable = JoinTablesInternal();
        }

        // Метод для выбора типа объединения и вызова соответствующего метода
        private DataTable JoinTablesInternal()
        {
            switch (JoinType.ToLower())
            {
                case "inner":
                    return InnerJoin();
                case "left":
                    return LeftJoin();
                case "right":
                    return RightJoin();
                case "full":
                    return FullOuterJoin();
                default:
                    throw new ArgumentException("Недопустимое значение для типа объединения.");
            }
        }

        // Метод для выполнения внутреннего объединения таблиц
        private DataTable InnerJoin()
        {
            var query = from row1 in Table1.AsEnumerable()
                        join row2 in Table2.AsEnumerable() on row1[JoinCondition] equals row2[JoinCondition]
                        select JoinRows(row1, row2);

            return query.CopyToDataTable();
        }

        // Метод для выполнения левого объединения таблиц
        private DataTable LeftJoin()
        {
            var query = from row1 in Table1.AsEnumerable()
                        join row2 in Table2.AsEnumerable() on row1[JoinCondition] equals row2[JoinCondition] into joined
                        from row2 in joined.DefaultIfEmpty()
                        select JoinRows(row1, row2);

            return query.CopyToDataTable();
        }

        // Метод для выполнения правого объединения таблиц
        private DataTable RightJoin()
        {
            var query = from row2 in Table2.AsEnumerable()
                        join row1 in Table1.AsEnumerable() on row2[JoinCondition] equals row1[JoinCondition] into joined
                        from row1 in joined.DefaultIfEmpty()
                        select JoinRows(row1, row2);

            return query.CopyToDataTable();
        }

        // Метод для выполнения полного внешнего объединения таблиц
        private DataTable FullOuterJoin()
        {
            var query = from row1 in Table1.AsEnumerable()
                        join row2 in Table2.AsEnumerable() on row1[JoinCondition] equals row2[JoinCondition] into joined
                        from row2 in joined.DefaultIfEmpty()
                        select JoinRows(row1, row2);

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

            // Копируем значения из первой таблицы
            foreach (DataColumn column in Table1.Columns)
            {
                newRow[column.ColumnName] = row1[column.ColumnName];
            }

            // Копируем значения из второй таблицы
            foreach (DataColumn column in Table2.Columns)
            {
                // Если столбец с таким именем уже существует в результирующей таблице, пропускаем его
                if (!ResultTable.Columns.Contains(column.ColumnName))
                {
                    ResultTable.Columns.Add(column.ColumnName, column.DataType);
                }
                newRow[column.ColumnName] = row2[column.ColumnName];
            }

            return newRow;
        }
    }
}