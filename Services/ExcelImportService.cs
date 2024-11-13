using System.Collections;
using ElAmir.Data;
using OfficeOpenXml;

public class ExcelImportService
{
    private readonly AppDbContext _context;

    public ExcelImportService(AppDbContext context)
    {
        _context = context;
    }

    // Generic method to insert data into the correct DbSet
    public async Task InsertDataIntoDatabase<T>(List<T> data) where T : class
    {
        try
        {
            var dbSet = _context.Set<T>();  // Get the DbSet of the specific entity type
            dbSet.AddRange(data);           // Add the records to the DbSet

            // Save the changes to the database
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inserting data: {ex.Message}");
            throw;
        }
    }

    // Method to import data from Excel and insert into the database
    public async Task ImportExcelData(MemoryStream stream, Type entityType)
    {
        try
        {
            // Create a list to hold the data with the correct entity type
            var dataListType = typeof(List<>).MakeGenericType(entityType);
            var dataList = (IList)Activator.CreateInstance(dataListType);

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];  // Assuming data is in the first worksheet
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                // Read the header row to map the Excel columns to entity properties
                var headerRow = new List<string>();
                for (int col = 1; col <= colCount; col++)
                {
                    headerRow.Add(worksheet.Cells[1, col].Text);
                }

                // Read each subsequent row and map the values to the entity
                for (int row = 2; row <= rowCount; row++)  // Starting from 2 because row 1 is the header
                {
                    var entityInstance = Activator.CreateInstance(entityType);

                    for (int col = 1; col <= colCount; col++)
                    {
                        string columnName = headerRow[col - 1];
                        var property = entityType.GetProperty(columnName);
                        if (property != null)
                        {
                            var cellValue = worksheet.Cells[row, col].Text;

                            // Handle null or empty values
                            if (string.IsNullOrWhiteSpace(cellValue))
                            {
                                if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                                {
                                    property.SetValue(entityInstance, null);  // Set to null for nullable types
                                }
                                else
                                {
                                    property.SetValue(entityInstance, Activator.CreateInstance(property.PropertyType)); // Default value for non-nullable types
                                }
                            }
                            else
                            {
                                // Handle specific type conversion for common types like DateTime, int, etc.
                                try
                                {
                                    if (property.PropertyType == typeof(int))
                                    {
                                        property.SetValue(entityInstance, int.TryParse(cellValue, out int intValue) ? intValue : default);
                                    }
                                    else if (property.PropertyType == typeof(DateTime))
                                    {
                                        property.SetValue(entityInstance, DateTime.TryParse(cellValue, out DateTime dateTimeValue) ? dateTimeValue : default);
                                    }
                                    else if (property.PropertyType == typeof(decimal))
                                    {
                                        property.SetValue(entityInstance, decimal.TryParse(cellValue, out decimal decimalValue) ? decimalValue : default);
                                    }
                                    else if (property.PropertyType == typeof(bool))
                                    {
                                        property.SetValue(entityInstance, bool.TryParse(cellValue, out bool boolValue) ? boolValue : default);
                                    }
                                    else if (property.PropertyType == typeof(string))
                                    {
                                        property.SetValue(entityInstance, cellValue);  // String can be directly set
                                    }
                                    else
                                    {
                                        // Use Convert.ChangeType for other types, including complex ones
                                        property.SetValue(entityInstance, Convert.ChangeType(cellValue, property.PropertyType));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error setting property {columnName}: {ex.Message}");
                                }
                            }
                        }
                    }

                    // Add the created instance of the entity to the list
                    dataList.Add(entityInstance);
                }

                // Explicitly pass the correct type to InsertDataIntoDatabase
                var method = typeof(ExcelImportService)
                    .GetMethod("InsertDataIntoDatabase")
                    .MakeGenericMethod(entityType); // Dynamically make the generic method

                // Invoke the method with the correct dataList
                await (Task)method.Invoke(this, new object[] { dataList });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing Excel data: {ex.Message}");
            throw;
        }
    }
}
