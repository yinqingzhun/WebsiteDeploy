using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WebDeploy.Utils
{
    public class ObjectCopier
    {
        /// <summary>
        /// 从指定对象复制一个指定类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>

        public static T Copy<T>(object source) where T : class,new()
        {
            if (source == null)
                throw new ArgumentNullException("source");


            T dst = Activator.CreateInstance<T>();

            List<PropertyInfo> sourcePropertyInfoList = source.GetType().GetProperties().ToList<PropertyInfo>();
            List<PropertyInfo> destinationPropertyInfoList = typeof(T).GetProperties().ToList<PropertyInfo>();
            foreach (PropertyInfo dstProperty in destinationPropertyInfoList)
            {

                bool existProperty = sourcePropertyInfoList.Any(i => dstProperty.Name == i.Name);
                try
                {
                    PropertyInfo sourceProperty = source.GetType().GetProperty(dstProperty.Name);
                    if (existProperty && dstProperty.CanWrite && sourceProperty.CanRead)
                    {
                        dstProperty.SetValue(dst, sourceProperty.GetValue(source, null), null);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error("复制对象失败", ex);
                }
            }

            return dst;
        }

    }
}
