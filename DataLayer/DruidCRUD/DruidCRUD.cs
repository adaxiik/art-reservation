namespace DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using System.Reflection;
using System.Text;


public class DruidConnection : IDisposable
{
    private SqliteConnection _connection;
    public DruidConnection(string connectionString)
    {
        _connection = new SqliteConnection(connectionString);
        _connection.Open();
    }

    public void Dispose()
    {
        _connection.Close();
    }

    public void ExecuteNonQuery(string sql)
    {
        using(var transaction = _connection.BeginTransaction())
        {
            try
            {
                using (var command = new SqliteCommand(sql, _connection, transaction))
                {
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw new Exception("Error executing sql: " + sql, e);
            }
        }
    }

    public long? ExecuteScalar(string sql)
    {
        using (var command = new SqliteCommand(sql, _connection))
        {
            return (long?)command.ExecuteScalar();
        }
    }

    public int LastInsertRowId { get => (int)ExecuteScalar("SELECT last_insert_rowid()")!;}

    public SqliteCommand CreateCommand(string sql) => new SqliteCommand(sql, _connection);
}


public static class DruidCRUD
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TableNameAttribute : Attribute
    {
        public string Name { get; set; }
        public TableNameAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnNameAttribute : Attribute
    {
        public string Name { get; set; }
        public ColumnNameAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
        public PrimaryKeyAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
        public IgnoreAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignColumnAttribute : Attribute
    {
        public ForeignColumnAttribute()
        {
        }
    }

    static string GetTableName<T>()
    {
        var type = typeof(T);
        return type.GetCustomAttributes(false)
                   .OfType<TableNameAttribute>()
                   .Single()
                   .Name ?? type.Name;
    }
    static string GetTableName(Type t)
    {
        return t.GetCustomAttributes(false)
                .OfType<TableNameAttribute>()
                .Single()
                .Name ?? t.Name;
    }

    static string GetColumnName(PropertyInfo property)
    {
        return property.GetCustomAttributes(false)
                       .OfType<ColumnNameAttribute>()
                       .SingleOrDefault()?
                       .Name ?? property.Name;
    }

    public static bool IsIgnored(PropertyInfo property)
    {
        return property.GetCustomAttributes(false)
                       .OfType<IgnoreAttribute>()
                       .Any();
    }

    public static bool IsPrimaryKey(PropertyInfo property)
    {
        return property.GetCustomAttributes(false)
                       .OfType<PrimaryKeyAttribute>()
                       .Any();
    }

    static bool IsNullable(PropertyInfo property)
    {
        return Nullable.GetUnderlyingType(property.PropertyType) != null;
    }

    static bool IsList(PropertyInfo property)
    {
        return property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
    }

    static bool IsForeignColumn(PropertyInfo property)
    {
        return property.GetCustomAttributes(false)
                       .OfType<ForeignColumnAttribute>()
                       .Any();
    }

    public static bool IsEnum(PropertyInfo property)
    {
        return property.PropertyType.IsEnum;
    }
    public static string ToMD5(string input)
    {
        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
    
    static string GetForeignKeyName(PropertyInfo property)
    {
        var foreignKeyProperty = GetPrimaryKeyProperty(property.PropertyType);
        return $"{GetColumnName(property)}_Id";
    }

    static int? GetForeignKeyValue(object obj)
    {
        var primaryKeyProperty = GetPrimaryKeyProperty(obj.GetType());
        return (int?)primaryKeyProperty.GetValue(obj);
    }


    public static PropertyInfo GetPrimaryKeyProperty(Type type)
    {
        if (type.GetProperties().Where(IsPrimaryKey).Count() > 1)
            throw new Exception("Cannot have more than one primary key");
        
        if (type.GetProperties().Where(IsPrimaryKey).Count() == 1)
            return type.GetProperties().Where(IsPrimaryKey).Single();
        
        if (type.GetProperties().Where(p => p.Name == "Id" && p.PropertyType == typeof(int)).Count() == 1)
            return type.GetProperties().Where(p => p.Name == "Id" && p.PropertyType == typeof(int)).Single();
        
        throw new Exception("Cannot find primary key");
    }

    static string GetSqlType(Type type)
    {
        // https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/types
        if (type == typeof(int) || type == typeof(int?))
            return "INTEGER";
        else if (type == typeof(string))
            return "TEXT";
        else if (type == typeof(bool))
            return "INTEGER";
        else if (type == typeof(DateTime))
            return "TEXT";
        else if (type == typeof(decimal))
            return "TEXT";
        else if (type == typeof(double))
            return "REAL";
        else
            throw new Exception("Unknown type: " + type);
    }


    public static void CreateTable<T>(this DruidConnection connection)
    {
        var type = typeof(T);
        var tableName = GetTableName<T>();


        StringBuilder sb = new StringBuilder();
        StringBuilder foreignKeys = new StringBuilder();
        sb.Append($"CREATE TABLE IF NOT EXISTS {tableName} (");
        
        var primaryKeyProperty = GetPrimaryKeyProperty(type);
        var primaryKeyColumnName = GetColumnName(primaryKeyProperty);
        var primaryKeySqlType = GetSqlType(primaryKeyProperty.PropertyType);
        sb.Append($"{primaryKeyColumnName} {primaryKeySqlType} PRIMARY KEY AUTOINCREMENT NOT NULL, ");

        foreach (var property in type.GetProperties())
        {
            if (IsIgnored(property))
                continue;
            
            if (IsPrimaryKey(property))
                continue;
            
            if(IsList(property))
                continue;

            if (IsForeignColumn(property))
            {
                var foreignKeyProperty = GetPrimaryKeyProperty(property.PropertyType);
                var foreignKeyColumnName = $"{GetColumnName(property)}_Id";
                var foreignKeySqlType = GetSqlType(foreignKeyProperty.PropertyType);

                sb.Append($"{foreignKeyColumnName} {foreignKeySqlType} NOT NULL, ");
                foreignKeys.Append($"FOREIGN KEY({foreignKeyColumnName}) REFERENCES {GetTableName(property.PropertyType)}({GetColumnName(foreignKeyProperty)}), ");
                continue;
            }

            if (IsEnum(property))
            {
                var enumType = property.PropertyType;
                var enumName = enumType.Name;
                var enumValues = Enum.GetValues(enumType);

                List<string> enumValuesStr = new List<string>();
                foreach (var enumValue in enumValues)
                    enumValuesStr.Add($"'{enumValue}'");

                sb.Append($"{GetColumnName(property)} TEXT NOT NULL CHECK({GetColumnName(property)} IN ({string.Join(", ", enumValuesStr)})), ");
                continue;
            }
            
            string columnName = GetColumnName(property);
            string  sqlType = GetSqlType(property.PropertyType);
            bool isNullable = IsNullable(property);
            sb.Append($"{columnName} {sqlType} {(isNullable ? "NULL" : "NOT NULL")}, ");
        }
        sb.Append(foreignKeys);
        sb.Remove(sb.Length - 2, 2);
        sb.Append(");");

        connection.ExecuteNonQuery(sb.ToString());
    }

    public static void DropTableIfExists<T>(this DruidConnection connection)
    {
        var tableName = GetTableName<T>();
        var sql = $"DROP TABLE IF EXISTS {tableName};";
        connection.ExecuteNonQuery(sql);
    }



    public static void Insert(this DruidConnection connection, object item)
    {
        var type = item.GetType();
        var tableName = GetTableName(type);

        int paramCount = 0;
        StringBuilder sb = new StringBuilder();
        sb.Append($"INSERT INTO {tableName} (");


        foreach(var property in type.GetProperties())
        {
            if (IsIgnored(property))
                continue;
            
            if (IsPrimaryKey(property))
                continue;
            
            if(IsList(property))
                continue;

            if (IsForeignColumn(property))
            {
                sb.Append($"{GetForeignKeyName(property)}, ");
                paramCount++;
                continue;
            }


            sb.Append($"{GetColumnName(property)}, ");
            paramCount++;

        }
        sb.Remove(sb.Length - 2, 2);
        sb.Append(") VALUES (");

        sb.Append(string.Join(", ", Enumerable.Range(0, paramCount).Select(i => $"@{i}")));
        sb.Append(");");

        var command = connection.CreateCommand(sb.ToString());

        int paramIndex = 0;
        foreach(var property in type.GetProperties())
        {
            if (IsIgnored(property))
                continue;
            
            if (IsPrimaryKey(property))
                continue;
            
            if(IsList(property))
                continue;

            if (IsForeignColumn(property))
            {
                var foreignKeyProperty = GetPrimaryKeyProperty(property.PropertyType);
                var foreignKeyValue = property.GetValue(item);

                if (foreignKeyValue == null)
                    throw new Exception("Cannot insert null foreign key");

                int? foreignId = GetForeignKeyValue(foreignKeyValue);
                if (foreignId == null)
                {
                    Insert(connection, foreignKeyValue);
                    foreignId = GetForeignKeyValue(foreignKeyValue);
                }

                command.Parameters.AddWithValue($"@{paramIndex}", foreignId);
                paramIndex++;
                continue;
            }

            if (IsEnum(property))
            {
                var enumValue = property.GetValue(item);
                command.Parameters.AddWithValue($"@{paramIndex}", enumValue!.ToString());
                paramIndex++;
                continue;
            }

            command.Parameters.AddWithValue($"@{paramIndex}", property.GetValue(item));
            paramIndex++;
        }
        command.ExecuteNonQuery();

        var primaryKeyProperty = GetPrimaryKeyProperty(type);
        primaryKeyProperty.SetValue(item, connection.LastInsertRowId);                                                        
    }

    public static T? GetById<T>(this DruidConnection connection, int id)
    {
        var type = typeof(T);
        var tableName = GetTableName(type);

        StringBuilder sb = new StringBuilder();
        sb.Append($"SELECT * FROM {tableName} WHERE Id = @Id;");

        var command = connection.CreateCommand(sb.ToString());
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return default;

        var item = Activator.CreateInstance<T>();
        foreach(var property in type.GetProperties())
        {
            if (IsIgnored(property))
                continue;
            
            if (IsList(property))
                continue;
            
            if (IsForeignColumn(property))
            {
                var foreignKeyProperty = GetPrimaryKeyProperty(property.PropertyType);
                var foreignKeyValue = reader.GetInt32(reader.GetOrdinal($"{GetForeignKeyName(property)}"));

                MethodInfo method = typeof(DruidCRUD).GetMethod(MethodBase.GetCurrentMethod()!.Name)!;
                MethodInfo generic = method.MakeGenericMethod(property.PropertyType);
                var foreignItem = generic.Invoke(null, new object[] { connection, foreignKeyValue });

                property.SetValue(item, foreignItem);
                continue;
            }

            if (IsEnum(property))
            {
                var enumValue = reader.GetString(reader.GetOrdinal(GetColumnName(property)));
                var enumType = property.PropertyType;
                var enumValueObj = Enum.Parse(enumType, enumValue);
                property.SetValue(item, enumValueObj);
                continue;
            }

            var value = reader.GetValue(reader.GetOrdinal(GetColumnName(property)));
            if(IsNullable(property))
                value = Convert.ChangeType(value, Nullable.GetUnderlyingType(property.PropertyType)!);
            else
                value = Convert.ChangeType(value, property.PropertyType);
            property.SetValue(item, value);
        }

        return item;
    }

    public static List<T> GetByForeignObject<T>(this DruidConnection connection, object foreignObject)
    {
        var type = typeof(T);
        var tableName = GetTableName(type);

        var foreignKeyProperty = GetPrimaryKeyProperty(foreignObject.GetType());
        var foreignKeyValue = foreignKeyProperty.GetValue(foreignObject);


        PropertyInfo? myProperty = null;
        foreach(var property in type.GetProperties())
        {
            if (IsForeignColumn(property))
            {
                var myForeignKeyProperty = GetPrimaryKeyProperty(property.PropertyType);
                if (myForeignKeyProperty == foreignKeyProperty)
                {
                    myProperty = property;
                    break;
                }
            }
        }

        if (myProperty == null)
            throw new Exception("No foreign key found"); 

        StringBuilder sb = new StringBuilder();
        sb.Append($"SELECT ");
        foreach(var property in type.GetProperties())
        {
            if (IsIgnored(property))
                continue;
            
            if (IsList(property))
                continue;
            
            if (IsForeignColumn(property))
            {
                sb.Append($"{GetForeignKeyName(property)}, ");
                continue;
            }

            sb.Append($"{GetColumnName(property)}, ");
        }

        sb.Remove(sb.Length - 2, 2);
        sb.Append($" FROM {tableName} WHERE {GetForeignKeyName(myProperty)} = @Id;");

        Console.WriteLine(sb.ToString());

        var command = connection.CreateCommand(sb.ToString());
        command.Parameters.AddWithValue("@Id", foreignKeyValue);

        using var reader = command.ExecuteReader();
        List<T> items = new List<T>();

        while (reader.Read())
        {
            var item = Activator.CreateInstance<T>();
            foreach(var property in type.GetProperties())
            {
                if (IsIgnored(property))
                    continue;
                
                if (IsList(property))
                    continue;
                
                if (IsForeignColumn(property))
                {
                    if (property == myProperty)
                    {
                        property.SetValue(item, foreignObject);
                        continue;
                    }

                    var myForeignKeyProperty = GetPrimaryKeyProperty(property.PropertyType);
                    var myforeignKeyValue = reader.GetInt32(reader.GetOrdinal($"{GetForeignKeyName(property)}"));

                    MethodInfo method = typeof(DruidCRUD).GetMethod("GetById")!;
                    MethodInfo generic = method.MakeGenericMethod(property.PropertyType);
                    var foreignItem = generic.Invoke(null, new object[] { connection, myforeignKeyValue });

                    property.SetValue(item, foreignItem);
                    continue;
                }

                if (IsEnum(property))
                {
                    var enumValue = reader.GetString(reader.GetOrdinal(GetColumnName(property)));
                    var enumType = property.PropertyType;
                    var enumValueObj = Enum.Parse(enumType, enumValue);
                    property.SetValue(item, enumValueObj);
                    continue;
                }

                var value = reader.GetValue(reader.GetOrdinal(GetColumnName(property)));
                if(IsNullable(property))
                    value = Convert.ChangeType(value, Nullable.GetUnderlyingType(property.PropertyType)!);
                else
                    value = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(item, value);
            }

            items.Add(item);
        }

        return items;
    }

    

    public static List<T> GetAll<T>(this DruidConnection connection)
    {
        var type = typeof(T);
        var tableName = GetTableName(type);

        StringBuilder sb = new StringBuilder();
        sb.Append($"SELECT * FROM {tableName};");

        var command = connection.CreateCommand(sb.ToString());

        using var reader = command.ExecuteReader();
        List<T> items = new List<T>();

        while (reader.Read())
        {
            var item = Activator.CreateInstance<T>();
            foreach(var property in type.GetProperties())
            {
                if (IsIgnored(property))
                    continue;
                
                if (IsList(property))
                    continue;
                
                if (IsForeignColumn(property))
                {
                    var foreignKeyProperty = GetPrimaryKeyProperty(property.PropertyType);
                    var foreignKeyValue = reader.GetInt32(reader.GetOrdinal($"{GetForeignKeyName(property)}"));

                    MethodInfo method = typeof(DruidCRUD).GetMethod("GetById")!;
                    MethodInfo generic = method.MakeGenericMethod(property.PropertyType);
                    var foreignItem = generic.Invoke(null, new object[] { connection, foreignKeyValue });

                    property.SetValue(item, foreignItem);
                    continue;
                }

                if (IsEnum(property))
                {
                    var enumValue = reader.GetString(reader.GetOrdinal(GetColumnName(property)));
                    var enumType = property.PropertyType;
                    var enumValueObj = Enum.Parse(enumType, enumValue);
                    property.SetValue(item, enumValueObj);
                    continue;
                }

                var value = reader.GetValue(reader.GetOrdinal(GetColumnName(property)));
                if(IsNullable(property))
                    value = Convert.ChangeType(value, Nullable.GetUnderlyingType(property.PropertyType)!);
                else
                    value = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(item, value);
            }

            items.Add(item);
        }

        return items;
    }

    public static List<object> GetAll(this DruidConnection connection, Type type)
    {
        var tableName = GetTableName(type);

        StringBuilder sb = new StringBuilder();
        sb.Append($"SELECT * FROM {tableName};");

        var command = connection.CreateCommand(sb.ToString());

        using var reader = command.ExecuteReader();
        List<object> items = new List<object>();

        while (reader.Read())
        {
            var item = Activator.CreateInstance(type);
            foreach(var property in type.GetProperties())
            {
                if (IsIgnored(property))
                    continue;
                
                if (IsList(property))
                    continue;
                
                if (IsForeignColumn(property))
                {
                    var foreignKeyProperty = GetPrimaryKeyProperty(property.PropertyType);
                    var foreignKeyValue = reader.GetInt32(reader.GetOrdinal($"{GetForeignKeyName(property)}"));

                    MethodInfo method = typeof(DruidCRUD).GetMethod("GetById")!;
                    MethodInfo generic = method.MakeGenericMethod(property.PropertyType);
                    var foreignItem = generic.Invoke(null, new object[] { connection, foreignKeyValue });

                    property.SetValue(item, foreignItem);
                    continue;
                }

                if (IsEnum(property))
                {
                    var enumValue = reader.GetString(reader.GetOrdinal(GetColumnName(property)));
                    var enumType = property.PropertyType;
                    var enumValueObj = Enum.Parse(enumType, enumValue);
                    property.SetValue(item, enumValueObj);
                    continue;
                }

                var value = reader.GetValue(reader.GetOrdinal(GetColumnName(property)));
                if(IsNullable(property))
                    value = Convert.ChangeType(value, Nullable.GetUnderlyingType(property.PropertyType)!);
                else
                    value = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(item, value);
            }

            items.Add(item!);
        }

        return items;
    }

    public static bool AreSame(object obj1, object obj2)
    {
        if (obj1.GetType() != obj2.GetType())
            return false;

        var primaryKey1 = GetPrimaryKeyProperty(obj1.GetType());
        var primaryKey2 = GetPrimaryKeyProperty(obj2.GetType());

        var primaryKeyValue1 = primaryKey1.GetValue(obj1);
        var primaryKeyValue2 = primaryKey2.GetValue(obj2);

        return primaryKeyValue1!.Equals(primaryKeyValue2);
    }

    public static bool HasId(object obj)
    {
        var primaryKey = GetPrimaryKeyProperty(obj.GetType());
        var primaryKeyValue = primaryKey.GetValue(obj);

        if (primaryKeyValue == null)
            return false;

        return true;
    }
    


    public static List<T> GetByProperty<T>(this DruidConnection connection, string propertyName, object value)
    {
        var type = typeof(T);
        var tableName = GetTableName(type);

        var property = type.GetProperty(propertyName);
        if (property == null)
            throw new Exception("Property not found");

        StringBuilder sb = new StringBuilder();
        sb.Append($"SELECT * FROM {tableName} WHERE {GetColumnName(property)} = @Value;");

        var command = connection.CreateCommand(sb.ToString());
        command.Parameters.AddWithValue("@Value", value);

        using var reader = command.ExecuteReader();
        List<T> items = new List<T>();

        while (reader.Read())
        {
            var item = Activator.CreateInstance<T>();
            foreach(var prop in type.GetProperties())
            {
                if (IsIgnored(prop))
                    continue;
                
                if (IsList(prop))
                    continue;
                
                if (IsForeignColumn(prop))
                {
                    var foreignKeyProperty = GetPrimaryKeyProperty(prop.PropertyType);
                    var foreignKeyValue = reader.GetInt32(reader.GetOrdinal($"{GetForeignKeyName(prop)}"));

                    MethodInfo method = typeof(DruidCRUD).GetMethod("GetById")!;
                    MethodInfo generic = method.MakeGenericMethod(prop.PropertyType);
                    var foreignItem = generic.Invoke(null, new object[] { connection, foreignKeyValue });

                    prop.SetValue(item, foreignItem);
                    continue;
                }

                if (IsEnum(prop))
                {
                    var enumValue = reader.GetString(reader.GetOrdinal(GetColumnName(prop)));
                    var enumType = prop.PropertyType;
                    var enumValueObj = Enum.Parse(enumType, enumValue);
                    prop.SetValue(item, enumValueObj);
                    continue;
                }

                var val = reader.GetValue(reader.GetOrdinal(GetColumnName(prop)));
                if(IsNullable(prop))
                    val = Convert.ChangeType(val, Nullable.GetUnderlyingType(prop.PropertyType)!);
                else
                    val = Convert.ChangeType(val, prop.PropertyType);
                prop.SetValue(item, val);
            }

            items.Add(item);
        }

        return items;
    }


    public static void Delete<T>(this DruidConnection connection, T item)
    {
        var type = typeof(T);
        var tableName = GetTableName(type);

        var primaryKeyProperty = GetPrimaryKeyProperty(type);
        var primaryKeyValue = primaryKeyProperty.GetValue(item);

        StringBuilder sb = new StringBuilder();
        sb.Append($"DELETE FROM {tableName} WHERE {GetColumnName(primaryKeyProperty)} = @Id;");

        var command = connection.CreateCommand(sb.ToString());
        command.Parameters.AddWithValue("@Id", primaryKeyValue);

        command.ExecuteNonQuery();
    }

    public static void Update(this DruidConnection connection, object item)
    {
        var type = item.GetType();
        var tableName = GetTableName(type);

        var primaryKeyProperty = GetPrimaryKeyProperty(type);
        var primaryKeyValue = primaryKeyProperty.GetValue(item);

        StringBuilder sb = new StringBuilder();
        sb.Append($"UPDATE {tableName} SET ");

        var properties = type.GetProperties();
        foreach(var property in properties)
        {
            if (IsIgnored(property))
                continue;
            
            if (IsList(property))
                continue;
            
            if (IsPrimaryKey(property))
                continue;
            
            if (IsForeignColumn(property))
            {
                Update(connection, property.GetValue(item)!);
                sb.Append($"{GetForeignKeyName(property)} = @{GetForeignKeyName(property)}, ");
                continue;
            }
            
            if (IsEnum(property))
            {
                var enumValue = property.GetValue(item);
                sb.Append($"{GetColumnName(property)} = '{enumValue}', ");
                continue;
            }
            
            sb.Append($"{GetColumnName(property)} = @{GetColumnName(property)}, ");
        }

        sb.Remove(sb.Length - 2, 2);
        sb.Append($" WHERE {GetColumnName(primaryKeyProperty)} = @Id;");

        var command = connection.CreateCommand(sb.ToString());

        command.Parameters.AddWithValue("@Id", primaryKeyValue);

        foreach(var property in properties)
        {
            if (IsIgnored(property))
                continue;
            
            if (IsList(property))
                continue;
            
            if (IsPrimaryKey(property))
                continue;
            
            if (IsForeignColumn(property))
            {
                var valuee = GetPrimaryKeyProperty(property.PropertyType).GetValue(property.GetValue(item));
                command.Parameters.AddWithValue($"@{GetForeignKeyName(property)}", valuee);
                continue;
            }
            
            if (IsEnum(property))
                continue;
            
            var value = property.GetValue(item);
            command.Parameters.AddWithValue($"@{GetColumnName(property)}", value);
        }
        command.ExecuteNonQuery();
    }
}
