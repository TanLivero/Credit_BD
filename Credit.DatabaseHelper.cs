// Credit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Credit.DatabaseHelper
using System.Data;
using Npgsql;

internal class DatabaseHelper
{
	private string connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=1234";

	public NpgsqlConnection GetConnection()
	{
		return new NpgsqlConnection(connectionString);
	}

	public int ExecuteNonQuery(string query, NpgsqlParameter[] parameters = null)
	{
		using NpgsqlConnection connection = GetConnection();
		connection.Open();
		using NpgsqlCommand command = new NpgsqlCommand(query, connection);
		if (parameters != null)
		{
			command.Parameters.AddRange(parameters);
		}
		return command.ExecuteNonQuery();
	}

	public DataTable ExecuteQuery(string query, NpgsqlParameter[] parameters = null)
	{
		DataTable dataTable = new DataTable();
		using (NpgsqlConnection connection = GetConnection())
		{
			connection.Open();
			using NpgsqlCommand command = new NpgsqlCommand(query, connection);
			if (parameters != null)
			{
				command.Parameters.AddRange(parameters);
			}
			using NpgsqlDataReader reader = command.ExecuteReader();
			dataTable.Load(reader);
		}
		return dataTable;
	}

	public object ExecuteScalar(string query, NpgsqlParameter[] parameters = null)
	{
		using NpgsqlConnection connection = GetConnection();
		connection.Open();
		using NpgsqlCommand command = new NpgsqlCommand(query, connection);
		if (parameters != null)
		{
			command.Parameters.AddRange(parameters);
		}
		return command.ExecuteScalar();
	}
}
