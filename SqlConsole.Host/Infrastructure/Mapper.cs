using System;
using System.Linq;
using System.Reflection;

namespace SqlConsole.Host.Infrastructure
{
    static class Mapper
    {
        class MapperImpl<TSource, TTarget>
           where TTarget : new()
        {
            
            internal static (PropertyInfo sourceproperty, PropertyInfo targetproperty)[] properties = (
                from targetproperty in typeof(TTarget).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                where targetproperty.PropertyType.IsSimpleType()
                join sourceproperty in typeof(TSource).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                on targetproperty.Name equals sourceproperty.Name
                select (sourceproperty, targetproperty)
            ).ToArray();
            public TTarget Map(TSource source)
            {
                var target = new TTarget();
                foreach ((var sourceproperty, var targetproperty) in properties)
                {
                    var sourcevalue = sourceproperty.GetValue(source);
                    var targetvalue = targetproperty.GetValue(target);
                    if (sourcevalue != null && !sourcevalue.Equals(targetvalue))
                        targetproperty.SetValue(target, sourcevalue);
                }
                return target;
            }
        }

        public static (PropertyInfo sourceproperty, PropertyInfo targetproperty)[] GetProperties<TSource, TTarget>() where TTarget : new()
            => MapperImpl<TSource, TTarget>.properties;

        public static TTarget Map<TSource, TTarget>(this TSource source)
            where TTarget: new()
        {
            return new MapperImpl<TSource, TTarget>().Map(source);
        }
    }
}