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

        //Inserts value into specific table. 'info' can contain either a password or a score. 
        //Depending on what 'table' ('users' or 'scores') to insert to, 'info' will reflect the appropriate value.
        //'user' is the username
        private void insertValue(String table, String user, String info)
        {
            String insert;
            if(table.Equals("users")) 
            {
                insert = String.Format("INSERT INTO {0}('name', 'pass') VALUES('{1}', '{2}')", table, user, info);
            }
            else if (table.Equals("scores"))
            {
                insert = String.Format("INSERT INTO {0}('name', 'totalscore', 'localhighscore') VALUES('{1}', '{2}', '0')", table, user, info);
            }
            else
            {
                Console.WriteLine("DATABASE MANAGER ERROR: INSERT");
                throw new Exception();
            }

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
        private void update(String table, String user, String info)
        {
            String update = "";
            String person = "";
            if(table.Equals("scores"))
            {
                person = String.Format("name = '{0}'", user);
                update = String.Format("totalscore = '{0}'", info);
            }
            else if (table.Equals("scores2"))
            {
                table = "scores";
                person = String.Format("name = '{0}'", user);
                update = String.Format("localhighscore = '{0}'", info);
            }
            else if (table.Equals("users"))
            {
                person = String.Format("name = '{0}'", user);
                update = String.Format("pass = '{0}'", info);
            }
            String statement = String.Format("UPDATE {0} SET {1} WHERE {2};", table, update, person);

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

            String query = String.Format("SELECT name, pass FROM users WHERE name='{0}'", user);
            DataTable result = runQuery(query);

            if (result.Rows.Count > 0)
            {
                String u = result.Rows[0]["name"].ToString();
                String p = result.Rows[0]["pass"].ToString();

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
        
        //Run query to get the player's score
        //Convert player's score to an integer
        //Add that score to the score that you got in the query
        //Update database with the cumulitive score
        //Return new score
        public int updateScore(String user, int score)
        {
            String query = String.Format("SELECT name, totalscore, localhighscore FROM scores WHERE name='{0}'", user);
            DataTable result = runQuery(query);
            String s = result.Rows[0]["totalscore"].ToString();
            String l = result.Rows[0]["localhighscore"].ToString();
            int si = Int32.Parse(s);
            int la = Int32.Parse(l);
            int newScore = si + score;

            update("scores", user, newScore.ToString());

            if (score > si)
            {
                update("scores2", user, score.ToString());
            }

            return newScore;
        }

        public DataTable grabScoreBoard()
        {
            String query = String.Format("SELECT name, totalscore, localhighscore FROM scores ORDER BY totalscore DESC LIMIT 5");
            return runQuery(query);
        }
    }
}
