using System.Reflection;
using ElAmir.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace ElAmir.Helpers
{
    public static class ExcelHelper
    {
        public static List<BaseEntity> ProcessExcelFile<T>(Stream stream) where T : BaseEntity, new()
        {
            var result = new List<BaseEntity>();

            using (var package = new ExcelPackage(stream))  // Assuming you're using EPPlus for reading Excel
            {
                var worksheet = package.Workbook.Worksheets[0];  // Get the first worksheet
                var rowCount = worksheet.Dimension.Rows;
                var columnCount = worksheet.Dimension.Columns;

                // Get the headers from the first row (assuming the first row is the header)
                var headers = new List<string>();
                for (int col = 1; col <= columnCount; col++)
                {
                    headers.Add(worksheet.Cells[1, col].Text.Trim());
                }

                // Loop through the rows, starting from row 2 (after header)
                for (int row = 2; row <= rowCount; row++)  
                {
                    var entity = new T();  // Create a new instance of the entity type

                    // Loop through each column in the row and map to the corresponding property
                    for (int col = 1; col <= columnCount; col++)
                    {
                        var header = headers[col - 1];  // Get header name
                        var property = typeof(T).GetProperty(header);  // Find the corresponding property by header name

                        if (property != null)
                        {
                            // Set the value of the property dynamically
                            var cellValue = worksheet.Cells[row, col].Text;
                            var convertedValue = Convert.ChangeType(cellValue, property.PropertyType);
                            property.SetValue(entity, convertedValue);
                        }
                    }

                    result.Add(entity);  // Add to the result list
                }
            }

            return result;
        }


        public static async Task InsertDataWithStoredProcedureAsync<T>(AppDbContext context, string storedProcedure, List<T> data) where T : BaseEntity
        {
            foreach (var record in data)
            {
                // Dynamically build the SQL string for the stored procedure
                var parametersList = record.GetType()
                    .GetProperties()
                    .Select(prop => $"@{prop.Name}")
                    .ToList();
                var sql = $"EXEC {storedProcedure} {string.Join(", ", parametersList)}";

                // Create and populate the SqlParameter list dynamically
                var parameters = record.GetType()
                    .GetProperties()
                    .Select(property => new SqlParameter($"@{property.Name}", property.GetValue(record) ?? DBNull.Value))
                    .ToList();

                // Execute the stored procedure with the dynamic SQL and parameters
                await context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
            }
        }
    }
}
