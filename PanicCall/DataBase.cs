using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.Net.Json;
//using System.Data.SQLite;

namespace PanicCall
{
    [Serializable]

    class DataBase
    {
        public string host_address{set; get;}
        public string database { set; get; }
        public string id { set; get; }
        public string pwd { set; get; }
        public string DDNS { set; get; }
        
        public string local { set; get; }
        public bool isConnect = false;

        [field: NonSerialized]
        private MySqlConnection mySql_conn;
        

        public DataBase()
        {

        }

        //~추가: DB 연결
        public bool connect()
        {
            if (string.IsNullOrEmpty(host_address))
            {
                isConnect = false;
                return false;
            }

            string strConn = "Server=" + host_address + ";Database=" + database + ";Uid=" + id + ";Pwd=" + pwd + ";Connection Timeout=1;"; 

            mySql_conn = new MySqlConnection(strConn);
            try
            {
                mySql_conn.Open();
                if (mySql_conn.Ping())
                {
                    mySql_conn.Close();
                    isConnect = true;
                    return true;
                }
                else
                {
                    mySql_conn.Close();
                    isConnect = false;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB 연결 에러");
                return false;
            }
        }


        //쿼리를 전송
        public bool send_sql(string sql)
        {
            try
            {
                if(mySql_conn.State == ConnectionState.Closed)
                    mySql_conn.Open();

                if (mySql_conn.Ping() == false)
                {
                    mySql_conn.Close();
                    return false;
                }

                MySqlCommand cmd = new MySqlCommand(sql, mySql_conn);
                cmd.ExecuteNonQuery();

                mySql_conn.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        

        //기능별 전송할 쿼리들
        //추가
        public bool add_data(string user_phone_number, string user_name, string user_address, string local_name, string local_ddns, string user_car_num, string user_code)
        {
            
             string sql = @"INSERT INTO user_data(user_phone_number, user_name, user_address, local_name, local_ddns, user_car_num, user_code) 
                            VALUES('" + user_phone_number + "','" + user_name + "','" + user_address + "','" + local_name + "','" + local_ddns + "','" + user_car_num + "','" + user_code + "')";

             return send_sql(sql);
        }
        //변경
        public bool adjust(string user_phone_number, string user_name, string user_address, string local_name, string local_ddns, string user_car_num, string user_code)
        {
 
            string sql = @"UPDATE user_data 
                           SET user_phone_number='" + user_phone_number + "', user_name='" + user_name + "', user_address='" + user_address + "', local_name='" + local_name + "', local_ddns='" + local_ddns + "', user_car_num='" + user_car_num + "', user_code='" + user_code + 
                        "' WHERE user_code ='" + user_code + "'";

            return send_sql(sql);
        }
        //삭제 - 전화번호로
        public bool deleteByPhoneNumber(string user_phone_number)
        {
            string sql = "DELETE FROM user_data WHERE user_phone_number='" + user_phone_number + "'";

            return send_sql(sql);
        }
        //삭제 - 유저코드로
        public bool deleteByUserCode(string user_code)
        {
            string sql = "DELETE FROM user_data WHERE user_code='" + user_code + "'";

            return send_sql(sql);
        }


        //데이터 찾기 번호 또는 유저코드로 찾음 (기본 유저코드)
        public DataSet search(string user_phone_number, string user_code)
        {
            DataSet data_set = new DataSet();

            string sql;
            if (String.IsNullOrEmpty(user_code))
                sql = @"SELECT * 
                        FROM user_data
                        WHERE user_phone_number LIKE '%" + user_phone_number + "%'";
            else
                sql = @"SELECT * 
                    FROM user_data
                    WHERE user_code='" + user_code + "'";
            

            try
            {
                if (mySql_conn.State == ConnectionState.Closed)
                    mySql_conn.Open();

                if (mySql_conn.Ping() == false)
                {
                    mySql_conn.Close();
                    return null;
                }
                MySqlDataAdapter adpt = new MySqlDataAdapter(sql, mySql_conn);
                adpt.Fill(data_set, "user_data");

                mySql_conn.Close();

                return data_set;

            }
            catch
            {
                return null;
            }
            
        }

        //APP DB 로그인 요청 왔을때 DB 로그인.
        public string dbLogin(string user_code)
        {
            DataSet data_set = new DataSet();
            User_Data_ _user_data = new User_Data_();

            string sql = "SELECT *FROM user_data WHERE user_code='" + user_code + "'";

            try
            {
                if (mySql_conn.State == ConnectionState.Closed)
                    mySql_conn.Open();

                if (mySql_conn.Ping() == false)
                {
                    mySql_conn.Close();
                    return null;
                }
                MySqlDataAdapter adpt = new MySqlDataAdapter(sql, mySql_conn);
                adpt.Fill(data_set, "user_data");

                // User_Data 형식으로 담아줌
                _user_data.User_Code = data_set.Tables[0].Rows[0]["user_code"].ToString();
                _user_data.User_Phone_Num = data_set.Tables[0].Rows[0]["user_phone_number"].ToString();
                _user_data.User_Name = data_set.Tables[0].Rows[0]["user_name"].ToString();
                _user_data.User_Address = data_set.Tables[0].Rows[0]["useR_address"].ToString();
                _user_data.local_name = data_set.Tables[0].Rows[0]["local_name"].ToString();
                _user_data.local_Ddns = data_set.Tables[0].Rows[0]["local_ddns"].ToString();
                _user_data.User_Car_Num = data_set.Tables[0].Rows[0]["user_car_num"].ToString();

                mySql_conn.Close();

                return Create_Json_Obj(_user_data);

            }
            catch (Exception e)
            {
                return "";
            }
        }

        //데이터 중복 검사
        public bool duplicate_inspection(string user_code)
        {
            bool result = true;
            DataSet data_set = new DataSet();
            try
            {
                mySql_conn.Open();

                if (mySql_conn.Ping() == true)
                {

                    String sql = "SELECT * FROM user_data";
                    MySqlDataAdapter adpt = new MySqlDataAdapter(sql, mySql_conn);
                    adpt.Fill(data_set, "user_data");

                    if (data_set.Tables.Count > 0)
                    {
                        foreach (DataRow r in data_set.Tables[0].Rows)
                        {
                            if (r["user_code"] == user_code)
                            {
                                result = false;
                            }
                        }
                    }

                    mySql_conn.Close();
                    return result;
                }
                else
                {
                    mySql_conn.Close();
                    return result;
                }

            }
            catch (Exception e)
            {
                return result;
            }
        }

        public String Create_Json_Obj(User_Data_ _user_data)
        {
            JsonObjectCollection json_col = new JsonObjectCollection();
            json_col.Add(new JsonStringValue("result", "true"));
            json_col.Add(new JsonStringValue("jlocalname", _user_data.local_name));
            json_col.Add(new JsonStringValue("jphone", _user_data.User_Phone_Num));
            json_col.Add(new JsonStringValue("jusercode", _user_data.User_Code));
            json_col.Add(new JsonStringValue("jname", _user_data.User_Name));
            json_col.Add(new JsonStringValue("jlocalddns", _user_data.local_Ddns));
            json_col.Add(new JsonStringValue("jaddress", _user_data.User_Address));
            json_col.Add(new JsonStringValue("jusercarnum", _user_data.User_Car_Num));

            JsonArrayCollection json_array = new JsonArrayCollection();
            json_array.Name = "myinfo";
            json_array.Add(json_col);

            String Final_Json_String = "{" + json_array.ToString() + "}";

            return Final_Json_String;

        }
    }
}
