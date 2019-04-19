using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace MemoDrops.SQL
{
	public class DataAccess
	{
		public string ExecuteQuery(string connectionString, string query)
		{
			string queryString = query;
			string result = string.Empty;
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				SqlCommand command = new SqlCommand(queryString, connection);
				// command.Parameters.AddWithValue("@tPatSName", "Your-Parm-Value");
				connection.Open();
				try
				{
					SqlDataReader reader = command.ExecuteReader();
					StringBuilder sb = new StringBuilder();
					string th = string.Empty;
					try
					{
						List<string> columns = new List<string>();
						for (int i = 0; i < reader.FieldCount; i++)
						{
							th += "<th>" + reader.GetName(i) + "</th>";
							columns.Add(reader.GetName(i));
						}
						while (reader.Read())
						{
							sb.Append("<tr>");
							for (int i = 0; i < reader.FieldCount; i++)
							{
								sb.Append("<td>" + (reader.IsDBNull(i) ? string.Empty : reader.GetValue(i).ToString()) + "</td>");
							}
							sb.AppendLine("</tr>");
						}
					}
					finally
					{
						// Always call Close when done reading.
						reader.Close();
					}
					return DateTime.Now + "<br /><table><th>" + th + "</th>" + sb + "</table>";
				}
				catch(Exception e)
				{
					return DateTime.Now + ">>> " + e.Message + (e.InnerException != null ? e.InnerException.Message : string.Empty);
				}
			}
			return result;
		}
	}
}
