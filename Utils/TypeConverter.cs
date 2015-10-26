using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WebDeploy.Utils
{
    public class TypeConverter
    {
        /// <summary>
        /// 将内存表中的数据导入到泛型集合中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datatable"></param>
        /// <returns></returns>
        public static IList<T> ConvertToList<T>(DataTable datatable)
            where T : class, new()
        {
            if (datatable == null || datatable.Rows.Count == 0)
                return new List<T>(0);

            List<T> results = new List<T>();

            PropertyInfo[] ps = typeof(T).GetProperties();
            for (int i = 0; i < datatable.Rows.Count; i++)
            {
                DataRow row = datatable.Rows[i];
                T o = Activator.CreateInstance<T>();
                results.Add(o);

                for (int j = 0; j < ps.Length; j++)
                {
                    PropertyInfo p = ps[j];
                    if (p.CanWrite && !DBNull.Value.Equals(row[p.Name]))
                    {
                        p.SetValue(o, row[p.Name], null);
                    }

                }
            }
            return results;
        }

        /// <summary>
        /// 将泛型集合转换为内存表。注意：集合中对象的属性类型最好与数据库数据类型对应，否则可能转换失败
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static DataTable ConvertToDataTable<T>(IEnumerable<T> collection)
            where T : class, new()
        {
            DataTable table = new DataTable();
            PropertyInfo[] ps = typeof(T).GetProperties().Where(p => p.GetGetMethod() != null && p.GetSetMethod() != null && !p.GetGetMethod().IsVirtual && !p.GetSetMethod().IsVirtual).ToArray();
            Array.ForEach(ps, p =>
            {
                table.Columns.Add(p.Name);
            });

            if (collection != null && collection.Any())
            {
                foreach (T c in collection)
                {
                    DataRow row = table.NewRow();
                    table.Rows.Add(row);
                    Array.ForEach(ps, p =>
                    {
                        row[p.Name] = p.GetValue(c, null);
                    });

                }
            }

            return table;
        }

        /// <summary>
        /// 将词典转换为内存表
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static DataTable ConvertToDataTable(string[] fields, Type[] types,
            List<Dictionary<String, object>> collection)
        {
            if (fields == null)
                throw new ArgumentNullException("fields");

            DataTable table = new DataTable();

            if (collection == null || collection.Count == 0 || fields == null
                || fields.Length == 0 || types == null || types.Length != fields.Length)
                return table;

            for (int i = 0; i < fields.Length; i++)
            {
                table.Columns.Add(fields[i], types[i]);
            };

            if (collection != null && collection.Any())
            {
                foreach (var c in collection)
                {
                    DataRow row = table.NewRow();
                    table.Rows.Add(row);
                    Array.ForEach(fields, field =>
                    {
                        row[field] = c[field];
                    });

                }
            }

            return table;
        }

        /// <summary>
        /// 将集合转换为指定类型的泛型集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IEnumerable<T> Convert<T>(IEnumerable collection)
        {
            foreach (var c in collection)
            {
                yield return (T)c;
            }
        }

    }
}
