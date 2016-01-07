using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SQLManager
{
    public class SQLManager
    {

        public string connectionString { get; set; }

        public SQLManager(string connection)
        {
            connectionString = connection;
        }

        public SqlDataReader getDataReader(string text)
        {
            SqlDataReader dr;
            try
            {
                SqlConnection sqlConnection = new SqlConnection(connectionString);
                
                    sqlConnection.Open();
                    using (SqlCommand command = new SqlCommand(text, sqlConnection))
                    {
                        command.CommandType = System.Data.CommandType.Text;
                        dr = command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                    }
                
                return dr;
            }
            catch
            {
                throw;
            }
        }

        public void doExecuteNonQuery(string text)
        {
            try
            {
                SqlConnection sqlConnection = new SqlConnection(connectionString);

                sqlConnection.Open();
                using (SqlCommand command = new SqlCommand(text, sqlConnection))
                {
                    command.CommandType = System.Data.CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                throw;
            }
        }

    }
}
