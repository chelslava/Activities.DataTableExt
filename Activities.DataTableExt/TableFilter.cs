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
            // �������� ������� ������� ������
            if (InputTable == null)
            {
                throw new ArgumentNullException(nameof(InputTable), "������� ������� �� ������.");
            }

            if (string.IsNullOrEmpty(Query))
            {
                throw new ArgumentNullException(nameof(Query), "������ �� �����.");
            }

            // ��������� ����������
            OutTable = ApplyFilter(Query, InputTable);
        }

        // ������� ��� ���������� ���������� � ������� ������
        private DataTable ApplyFilter(string sqlQuery, DataTable table)
        {
            // ��������� ��������� �������
            DataTable filteredTable = table.Clone();

            // ��������� ������ � ������� � ������� LINQ
            DataRow[] filteredRows = table.Select(sqlQuery);
            var filteredQuery = filteredRows.AsEnumerable();

            // ����������� ������ � ����� �������
            filteredQuery.ToList().ForEach(row => filteredTable.ImportRow(row));

            return filteredTable;
        }
    }
}
