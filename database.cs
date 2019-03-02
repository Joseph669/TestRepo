//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
////using System.Data.SqlClient;
////using MySql.Data.MySqlClient;

//namespace MyDatabase
//{
//    class database
//    {
//        private void button1_Click(object sender, EventArgs e)
//        {
//            MySqlConnection myconn = null;
//            MySqlCommand mycom = null;
//            myconn = new MySqlConnection("Host =localhost;Port=3366;Database=student;Username=root;Password=123");
//            myconn.Open();
//            mycom = myconn.CreateCommand();
//            mycom.CommandText = "SELECT *FROM student1";
//            MySqlDataAdapter adap = new MySqlDataAdapter(mycom);
        
//            myconn.Close();
//        }
//    }
//}
