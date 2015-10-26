
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using DBContextGenerator;

namespace WebDeploy.Repository
{
     public class RepositoryBase  
    {
        private readonly System.Data.Entity.DbContext _context = null;
        public static string ConnectionString = "";


        public RepositoryBase()
        {
             
            _context = new DeployEntities();


            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                DbConnection conn = _context.Database.Connection;
                ConnectionString = conn.ConnectionString;
            }

        }

        /// <summary>
        /// 获取数据库上下文
        /// </summary>
        /// <returns></returns>
        public System.Data.Entity.DbContext DbContext
        {
            get { return _context; }
        }

        public int Count<T>() where T : class, new()
        {
            return _context.Set<T>().Count();
        }



        /// <summary>
        /// 插入实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T Add<T>(T entity) where T : class, new()
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            _context.Set<T>().Add(entity);
            _context.SaveChanges();
            return entity;
        }
        /// <summary>
        /// 删除实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public void Remove<T>(T entity) where T : class, new()
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            _context.Set<T>().Remove(entity);
            _context.SaveChanges();
        }
        /// <summary>
        /// 删除实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValues"></param>
        public void RemoveByPrimaryKey<T>(params object[] keyValues) where T : class, new()
        {
            T o = FindByPrimaryKey<T>(keyValues);
            if (o != null)
                Remove(o);
        }
        /// <summary>
        /// 更新实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T Update<T>(T entity) where T : class, new()
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (_context.Entry<T>(entity).State != EntityState.Modified)
                _context.Entry<T>(entity).State = EntityState.Modified;

            _context.SaveChanges();
            return entity;
        }
        /// <summary>
        /// 查找实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        public T FindByPrimaryKey<T>(params object[] keyValues) where T : class, new()
        {
            return _context.Set<T>().Find(keyValues);
        }
        /// <summary>
        /// 查找实体对象集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> FindAll<T>() where T : class, new()
        {
            return _context.Set<T>().ToList();
        }
        /// <summary>
        /// 执行查询，并返回查询的结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="sqlParams"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public List<T> ExecuteQuery<T>(CommandType commandType, string commandText, SqlParameter[] sqlParams, SqlTransaction transaction) where T : class, new()
        {
            SqlConnection connection = NewConnection(transaction);
            try
            {
                using (SqlCommand command = CreateSqlCommand(connection, commandType, commandText, sqlParams,
                        transaction))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        return Read<T>(reader);
                    }
                }
            }
            finally
            {
                CloseConnection(connection, transaction);
            }

        }
        /// <summary>
        /// 从SqlDataReader中读取结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        private List<T> Read<T>(SqlDataReader reader) where T : class, new()
        {
            string[] fieldNameList = new string[reader.FieldCount];
            for (int i = 0; i < fieldNameList.Length; i++)
                fieldNameList[i] = reader.GetName(i);


            List<T> results = new List<T>();

            PropertyInfo[] properties = typeof(T).GetProperties();
            if (properties.Length > 0)
            {

                while (reader.Read())
                {

                    T o = Activator.CreateInstance<T>();
                    results.Add(o);

                    for (int j = 0; j < properties.Length; j++)
                    {
                        PropertyInfo p = properties[j];
                        string fieldName = "";
                        Array.ForEach(fieldNameList, field =>
                        {
                            if (field.Equals(p.Name, StringComparison.OrdinalIgnoreCase))
                                fieldName = field;
                        });
                        if (!string.IsNullOrWhiteSpace(fieldName) && p.CanWrite && !DBNull.Value.Equals(reader[fieldName]))
                            p.SetValue(o, reader[fieldName], null);

                    }
                }
            }
            else
            {

                while (reader.Read())
                {
                    if (reader.FieldCount > 0 && !DBNull.Value.Equals(reader[0]))
                        results.Add((T)reader[0]);
                }

            }
            return results;
        }
        /// <summary>
        /// 创建和初始化SqlCommand
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="sqlParams"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private SqlCommand CreateSqlCommand(SqlConnection connection, CommandType commandType, string commandText, SqlParameter[] sqlParams, SqlTransaction transaction)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("commandText");
            SqlCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            if (sqlParams != null && sqlParams.Length > 0)
                command.Parameters.AddRange(sqlParams);

            if (transaction != null)
            {
                if (transaction.Connection == null)
                {
                    throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                }
                command.Transaction = transaction;
            }
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return command;
        }
        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。 忽略其他列或行
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="sqlParams"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public object ExecuteScalar(CommandType commandType, string commandText, SqlParameter[] sqlParams, SqlTransaction transaction)
        {
            SqlConnection connection = NewConnection(transaction);
            try
            {
                using (SqlCommand command = CreateSqlCommand(connection, commandType, commandText, sqlParams,
                        transaction))
                {
                    return command.ExecuteScalar();
                }
            }
            finally
            {
                CloseConnection(connection, transaction);
            }
        }
        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="sqlParams"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(CommandType commandType, string commandText, SqlParameter[] sqlParams, SqlTransaction transaction)
        {
            SqlConnection connection = NewConnection(transaction);
            try
            {
                using (SqlCommand command = CreateSqlCommand(connection, commandType, commandText, sqlParams,
                        transaction))
                {
                    return command.ExecuteNonQuery();
                }
            }
            finally
            {
                CloseConnection(connection, transaction);
            }

        }
        private SqlConnection NewConnection(SqlTransaction transaction)
        {
            SqlConnection connection = null;
            if (transaction == null)
                connection = new SqlConnection(ConnectionString);
            else if (transaction.Connection != null)
                connection = transaction.Connection;
            else
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            return connection;
        }
        private void CloseConnection(SqlConnection connection, SqlTransaction transaction)
        {
            if (transaction == null && connection != null && connection.State == ConnectionState.Open)
                connection.Close();
        }
        /// <summary>
        /// 执行查询，并返回查询的结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        public List<T> ExecuteQuery<T>(CommandType commandType, string commandText, SqlParameter[] sqlParams) where T : class, new()
        {
            return ExecuteQuery<T>(commandType, commandText, sqlParams, null);
        }
        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。 忽略其他列或行
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        public object ExecuteScalar(CommandType commandType, string commandText, SqlParameter[] sqlParams)
        {
            return ExecuteScalar(commandType, commandText, sqlParams, null);
        }
        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(CommandType commandType, string commandText, SqlParameter[] sqlParams)
        {
            return ExecuteNonQuery(commandType, commandText, sqlParams, null);
        }
        /// <summary>
        /// 执行查询，并返回查询的结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="sqlParams"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public List<T> ExecuteQuery<T>(string commandText, SqlParameter[] sqlParams, SqlTransaction tran)
            where T : class, new()
        {
            return ExecuteQuery<T>(CommandType.Text, commandText, sqlParams, tran);
        }
        /// <summary>
        /// 执行查询，并返回查询的结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        public List<T> ExecuteQuery<T>(string commandText, params SqlParameter[] sqlParams) where T : class, new()
        {
            return ExecuteQuery<T>(CommandType.Text, commandText, sqlParams);

        }
        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。 忽略其他列或行
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="sqlParams"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public Object ExecuteScalar(string commandText, SqlParameter[] sqlParams, SqlTransaction tran)
        {
            return ExecuteScalar(CommandType.Text, commandText, sqlParams, tran);
        }
        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。 忽略其他列或行
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        public Object ExecuteScalar(string commandText, params SqlParameter[] sqlParams)
        {
            return ExecuteScalar(CommandType.Text, commandText, sqlParams);
        }
        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="sqlParams"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string commandText, SqlParameter[] sqlParams, SqlTransaction tran)
        {
            return ExecuteNonQuery(CommandType.Text, commandText, sqlParams, tran);
        }
        /// <summary>
        /// 对连接执行 Transact-SQL 语句并返回受影响的行数
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="sqlParams"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string commandText, params SqlParameter[] sqlParams)
        {
            return ExecuteNonQuery(CommandType.Text, commandText, sqlParams);
        }


        /// <summary>
        /// 批量数据入库。注意：dataTable的表结构一定要与物理表的表结构一致，否则批量数据复制可能失败
        /// </summary>
        /// <param name="dataTable">内存表</param>
        /// <param name="destTableName">物理表表名，为空时取表实体类类型名</param>
        public void BulkCopy(DataTable dataTable, string destTableName)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return;

            if (string.IsNullOrWhiteSpace(destTableName))
                throw new ArgumentNullException("destTableName");

            string connString = ConnectionString;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlBulkCopy sbc = new SqlBulkCopy(conn);
                sbc.BatchSize = 100;
                sbc.DestinationTableName = destTableName;
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    string colName = dataTable.Columns[i].ColumnName;
                    if (colName != "PKID")
                        sbc.ColumnMappings.Add(colName, colName);
                }
                sbc.WriteToServer(dataTable);
            }
        }
        /// <summary>
        /// 获取集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderByDesc"></param>
        /// <returns></returns>
        public List<T> GetList<T>(System.Linq.Expressions.Expression<Func<T, bool>> where = null, System.Linq.Expressions.Expression<Func<T, object>> orderBy = null, bool orderByDesc = false) where T : class
        {
            if (where == null)
            {
                if (orderBy == null)
                    return _context.Set<T>().ToList();
                if (orderByDesc)
                    return _context.Set<T>().OrderByDescending(orderBy).ToList();
                return _context.Set<T>().OrderBy(orderBy).ToList();
            }
            if (orderBy == null)
                return _context.Set<T>().Where(where).ToList();
            if (orderByDesc)
                return _context.Set<T>().Where(where).OrderByDescending(orderBy).ToList();
            return _context.Set<T>().Where(where).OrderBy(orderBy).ToList();
        }
        /// <summary>
        /// 获取集合（分页）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="orderBy"></param>
        /// <param name="orderByDesc"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public List<T> GetListPager<T>(System.Linq.Expressions.Expression<Func<T, object>> orderBy, bool orderByDesc = false, int pageIndex = 1, int pageSize = 10, System.Linq.Expressions.Expression<Func<T, bool>> where = null) where T : class
        {
            if (orderBy == null)
                throw new ArgumentNullException("orderBy");

            int skip = (pageIndex - 1) * pageSize;
            int take = pageSize;

            if (where == null)
            {
                if (orderByDesc)
                    return _context.Set<T>().OrderByDescending(orderBy).Skip(skip).Take(take).ToList();
                return _context.Set<T>().OrderBy(orderBy).Skip(skip).Take(take).ToList();
            }
            if (orderByDesc)
                return _context.Set<T>().Where(where).OrderByDescending(orderBy).Skip(skip).Take(take).ToList();
            return _context.Set<T>().Where(where).OrderBy(orderBy).Skip(skip).Take(take).ToList();
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="sqlParams"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public DataSet ExecuteDataSet(CommandType commandType, string commandText, SqlParameter[] sqlParams, SqlTransaction transaction)
        {
            SqlConnection connection = NewConnection(transaction);
            try
            {

                using (SqlCommand command = CreateSqlCommand(connection, commandType, commandText, sqlParams,
                        transaction))
                {
                    DataSet ds = new DataSet();
                    new SqlDataAdapter(command).Fill(ds);
                    return ds;
                }
            }
            finally
            {
                CloseConnection(connection, transaction);
            }
        }

    }
}
