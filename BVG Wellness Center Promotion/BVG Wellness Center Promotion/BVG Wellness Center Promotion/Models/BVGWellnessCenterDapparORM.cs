using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace BVG_Wellness_Center_Promotion.Models
{
    public class StringArrayHandler : SqlMapper.TypeHandler<string[]>
    {
        public override void SetValue(IDbDataParameter parameter, string[] value)
        {
            parameter.Value = value == null || value.Length == 0 ? (object)DBNull.Value : string.Join(",", value);
        }

        public override string[] Parse(object value)
        {
            if (value == null || value is DBNull) return new string[0];
            return value.ToString().Split(',');
        }
    }

    public class BVGWellnessCenterDapparORM
    {
        static BVGWellnessCenterDapparORM()
        {
            SqlMapper.AddTypeHandler(new StringArrayHandler());
        }

        //public static string connectionString;


        public static string connectionStrings = System.Configuration.ConfigurationManager.ConnectionStrings["ConnetionString"].ConnectionString;

        public static string connectionString;
        public static void SetConnection(string Con_Server, string Con_UserId, string Con_Password, string Con_Database)
        {
            string connectionString1 = @"Data Source=" + Con_Server + ";uid=" + Con_UserId + ";pwd=" + Con_Password + ";Initial Catalog=" + Con_Database + ";Integrated Security=false; Connection Timeout=0";
            HttpContext.Current.Session["NewConnetionString"] = connectionString1;
        }

        internal static void SetConnection()
        {
            connectionString = HttpContext.Current.Session["NewConnetionString"].ToString();
            throw new NotImplementedException();
        }


        //public static void SetConnection()
        //{
        //    connectionString = ConfigurationManager.ConnectionStrings["ConnetionString"].ConnectionString;
        //    //connectionString = @"Data Source=192.168.1.170;uid=essl1;pwd=Dk@12345;Initial Catalog=BVGLifeScience;Integrated Security=false;";
        //}

        //Excute Sp For Update Delete and Insert
        public static object ExcuteSp(string ProcedureName, DynamicParameters Param = null)
        {
            try
            {
                connectionString = HttpContext.Current.Session["NewConnetionString"].ToString();
                SqlConnection sqlcon = new SqlConnection(connectionString);
                sqlcon.Open();
                return sqlcon.Execute(ProcedureName, Param, commandType: CommandType.StoredProcedure);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static IEnumerable<T> ReturnList<T>(string procedureName, DynamicParameters param = null)
        {
            //connectionString = HttpContext.Current.Session["NewConnetionString"].ToString();

            using (SqlConnection sqlcon = new SqlConnection(GetConnectionString()))
            {
                sqlcon.Open();
                return sqlcon.Query<T>(procedureName, param, commandType: CommandType.StoredProcedure);
            }
        }
       
        //Get Dynamic List
        public static dynamic DynamicList(string procedureName, DynamicParameters param = null)
        {
            connectionString = HttpContext.Current.Session["NewConnetionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                return sqlcon.Query(procedureName, param, commandType: CommandType.StoredProcedure);
            }
        }

        //Excute Query
        public static dynamic ExecuteQuery(string Query)
        {
          //  connectionString = HttpContext.Current.Session["NewConnetionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(GetConnectionString()))
            {
                sqlcon.Open();
                var data = sqlcon.Query(Query);
                return data;

            }
        }
         //First Or deafult
         public static int MaxId(string Query)
        {
            connectionString = HttpContext.Current.Session["NewConnetionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                
                return sqlcon.QueryFirstOrDefault<int>(Query);

            }
        }

        //Convert Image Into Base64
        public static string ConvertImageToBase64(string filePath)
        {
            connectionString = HttpContext.Current.Session["NewConnetionString"].ToString();
            if (System.IO.File.Exists(filePath))
            {
                byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);
                string base64String = Convert.ToBase64String(imageBytes);
                return $"data:image/jpeg;base64,{base64String}";
            }
            return null; // Or return a default Base64 string for a placeholder image
        }


        public static void ExecuteWithoutReturn(string procedureName, DynamicParameters param = null)
        {
            connectionString = HttpContext.Current.Session["NewConnetionString"].ToString();
            using (SqlConnection sqlcon = new SqlConnection(connectionString))
            {
                sqlcon.Open();
                sqlcon.Execute(procedureName, param, commandType: CommandType.StoredProcedure);
            }
        }

        private static string GetConnectionString()
        {
            if (HttpContext.Current?.Session != null && HttpContext.Current.Session["NewConnetionString"] != null)
            {
                return HttpContext.Current.Session["NewConnetionString"].ToString();
            }
            // जर सेशन नसेल, तर Web.config मधून कनेक्शन स्ट्रिंग घ्या
            return System.Configuration.ConfigurationManager.ConnectionStrings["ConnetionString"].ConnectionString;
        }


        public static SqlMapper.GridReader QueryMultiple(string procedureName, DynamicParameters param = null)
        {
            connectionString = HttpContext.Current.Session["NewConnetionString"].ToString();

            var sqlcon = new SqlConnection(connectionString);
            sqlcon.Open();

            return sqlcon.QueryMultiple(
                procedureName,
                param,
                commandType: CommandType.StoredProcedure
            );
        }

    }
}