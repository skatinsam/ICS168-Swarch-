using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;


namespace SwarchServer
{
    class DatabaseManager
    {
        private string database;
        

        //Initalize Database
        public DatabaseManager()
        {
            //Insert name of database here
            database = "Data Source=UserDatabase.db";

            //Create new MD5 object for hashing
            
        }
        
        //BASIC DATABASE FUNCTIONS THAT ARE NEEDED

        //Run a query on the database
        //  Open the connection to the database 
        //  Send a command message to the database
        //  Run the query 
        //  Close connection
        private DataTable runQuery(string query)
        {
            DataTable table = new DataTable();

            try
            {
                SQLiteConnection connection = new SQLiteConnection(database);
                connection.Open();
            
                SQLiteCommand command = new SQLiteCommand(connection); 
                command.CommandText = query;

                SQLiteDataReader read = command.ExecuteReader();
                table.Load(read);

                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("DATABASE MANAGER ERROR: QUERY");
                throw new Exception(e.Message);
            }
            return table;
        }

        //Inserts value into specific table
        private void insertValue(String table, String user, String info)
        {

            String insert = String.Format("INSERT INTO {0}('{1}') VALUES({2});", table, user, info);

            try
            {
                SQLiteConnection connect = new SQLiteConnection(database);
                connect.Open();

                SQLiteCommand command = new SQLiteCommand(connect);
                command.CommandText = insert;
                command.ExecuteNonQuery();

                connect.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("DATABASE MANAGER ERROR: INSERT");
                throw new Exception(e.Message);
            }
        }

        //Updates table with new info given
        private void update(String table, String user, String info, String where)
        {
            String update = String.Format(" {0} = '{1}',", user, info);
            String statement = String.Format("UPDATE {0} SET {1} WHERE {2};", table, update, where);

            try
            {
                SQLiteConnection connect = new SQLiteConnection(database);
                connect.Open();

                SQLiteCommand command = new SQLiteCommand(connect);
                command.CommandText = statement;
                command.ExecuteNonQuery();

                connect.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("DATABASE MANAGER ERROR: UPDATE");
                throw new Exception(e.Message);
            }
        }

        //SWARCH SPECIFIC FUNCTIONS

        //Run a query to check if the user and password are valid
        //If user and password are correct
        //  They are connected
        //Else they are rejected
        //If user not found
        //  User and password are inserted into the database
        public string connect(String user, String password)
        {

            String query = String.Format("SELECT name, pass FROM users WHERE name={0}", user);
            DataTable result = runQuery(query);

            if (result.Rows.Count > 0)
            {
                string u = result.Rows[0]["name"].ToString();
                string p = result.Rows[0]["pass"].ToString();

                if (user.Equals(u))
                {
                    if (password.Equals(p))
                    {
                        return "connect";
                    }
                    else
                    {
                        return "fail";
                    }
                }
            }
            else
            {
                insertValue("users", user, password);
                insertValue("scores", user, "0");
            }
            return "added";
        }

        
    }
}
