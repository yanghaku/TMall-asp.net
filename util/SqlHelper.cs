using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace TMall.util {
	public class SqlHelper {
		private SqlHelper() { }

		private static readonly string connStr = ConfigurationManager.ConnectionStrings["Conn"].ConnectionString;

		// 返回受影响的行数, 主要用于insert,update,delete等
		public static int ExecuteNoQuery(string sqlString,params SqlParameter[] sqlParameters) {
			SqlConnection conn = new SqlConnection(connStr);//数据库连接对象
			SqlCommand sqlcmd = new SqlCommand(sqlString, conn);//command 对象
			if(sqlParameters != null && sqlParameters.Length > 0) {
				sqlcmd.Parameters.AddRange(sqlParameters);//添加执行的参数
			}
			try {
				conn.Open();// 建立连接
				return sqlcmd.ExecuteNonQuery();
			}catch(Exception) {
				return 0;
			} finally {
				conn.Close();// 最后关闭连接
			}
		}

		// 返回查询表格的第一行第一列, 如查询个数的问题
		public static object ExecuteScalar(string sqlString,params SqlParameter[] sqlParameters) {
			SqlConnection conn = new SqlConnection(connStr);//数据库连接对象
			SqlCommand sqlcmd = new SqlCommand(sqlString, conn);//command 对象
			if (sqlParameters != null && sqlParameters.Length > 0) {
				sqlcmd.Parameters.AddRange(sqlParameters);//添加执行的参数
			}
			try {
				conn.Open();// 建立连接
				return sqlcmd.ExecuteScalar();//执行
			} catch (Exception) {
				return null;
			} finally {
				conn.Close();// 最后关闭连接
			}
		}

		// 返回查询的表格
		public static SqlDataReader ExecuteTable(string sqlString, params SqlParameter[] sqlParameters) {
			SqlConnection conn = new SqlConnection(connStr);//数据库连接对象
			SqlCommand sqlcmd = new SqlCommand(sqlString, conn);//command 对象
			if (sqlParameters != null && sqlParameters.Length > 0) {
				sqlcmd.Parameters.AddRange(sqlParameters);//添加执行的参数
			}
			try {
				conn.Open();// 建立连接
				return sqlcmd.ExecuteReader(CommandBehavior.CloseConnection);//关闭SqlDataReader的时候自动关闭连接
			} catch (Exception) {
				return null;
			}
		}

	}
}