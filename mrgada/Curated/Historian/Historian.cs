using Npgsql;
using static Mrgada.Historian;

public static partial class Mrgada
{
    public static partial class Historian
    {
        public class HistorianDb
        {
            public string Name;
            public HistorianDb(string Name)
            {
                this.Name = Name;
            }
        }
        static string GetConnectionString(string dbName) =>
            $"Host=192.168.64.124;Username=postgres;Password=mare;Database={dbName}";
        public class Tag<T>
        {
            string _name;
            string _egu;
            HistorianDb _historianDb;
            string _dbType;
            public Tag(HistorianDb historianDb, string name, string egu)
            {
                _name = name;
                _egu = egu;
                _historianDb = historianDb;

                if (typeof(T) == typeof(double)) _dbType = "DOUBLE PRECISION";
                else if (typeof(T) == typeof(int)) _dbType = "INTEGER";
                else if (typeof(T) == typeof(string)) _dbType = "TEXT";
                else if (typeof(T) == typeof(bool)) _dbType = "BOOLEAN";
                else if (typeof(T) == typeof(DateTime)) _dbType = "TIMESTAMPTZ";
                else throw new Exception("Unsupported data type");

                Initialize();
            }
            public void Initialize()
            {
                using var conn = new NpgsqlConnection(GetConnectionString(_historianDb.Name));
                conn.Open();
                // Create the tags table if it doesn't exist
                var createTableQuery = $@"
                CREATE TABLE IF NOT EXISTS {_name} (
                    id SERIAL,
                    timestamp TIMESTAMPTZ NOT NULL,
                    value {_dbType} NOT NULL,
                    PRIMARY KEY (id, timestamp) -- Include 'timestamp' in the primary key
                );
                SELECT create_hypertable('{_name}', 'timestamp', if_not_exists => TRUE);
            ";
                using (var cmd = new NpgsqlCommand(createTableQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
            public void Historize(T value, DateTime dateTime)
            {
                if (value == null) throw new Exception($"Value was null for tag {_name}, db {_historianDb.Name}");
                using var conn = new NpgsqlConnection(GetConnectionString(_historianDb.Name));
                conn.Open();
                var insertQuery = $@"
                    INSERT INTO {_name} (timestamp, value)
                    VALUES (@timestamp, @value);
                ";
                using (var cmd = new NpgsqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("timestamp", dateTime.ToUniversalTime());
                    cmd.Parameters.AddWithValue("value", value);
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
            public Dictionary<DateTime, T> RetrieveTimeSeries(DateTime startTime, DateTime endTime)
            {
                Dictionary<DateTime, T> dictionary = new();

                using var conn = new NpgsqlConnection(GetConnectionString(_historianDb.Name));
                conn.Open();

                // Define the query to retrieve tags within the specified time range and tag name
                var retrieveQuery = $@"
                    SELECT timestamp, value
                    FROM {_name}
                    WHERE timestamp >= @start_time
                      AND timestamp <= @end_time
                    ORDER BY timestamp;
                ";

                using var cmd = new NpgsqlCommand(retrieveQuery, conn);
                cmd.Parameters.AddWithValue("start_time", startTime);
                cmd.Parameters.AddWithValue("end_time", endTime);

                using var reader = cmd.ExecuteReader();

                Console.WriteLine($"Retrieving data for tag: {_name} from database: {_historianDb.Name}");
                while (reader.Read())
                {
                    DateTime TimeStamp = reader.GetDateTime(0);
                    T Value = default(T);
                    if (typeof(T) == typeof(double))
                    {
                        Value = (T)Convert.ChangeType(reader.GetDouble(1), typeof(T));
                    }
                    dictionary.Add(TimeStamp, Value);
                    Console.WriteLine($"Tag: {_name}, Timestamp: {TimeStamp.ToString()}, Value: {Value.ToString()}");
                }

                conn.Close();

                return dictionary;
            }
        }
    }
}