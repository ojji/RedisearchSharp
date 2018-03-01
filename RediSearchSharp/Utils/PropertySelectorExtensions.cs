using System;
using System.Linq.Expressions;

namespace RediSearchSharp.Utils
{
    public static class PropertySelectorExtensions
    {
        public static string GetMemberName<TEntity, TProperty>(this Expression<Func<TEntity, TProperty>> propertySelector)
        {
            if (!(propertySelector.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException("Property selector is not a member expression");
            }

            var memberInfo = memberExpression.Member;

#if NETSTANDARD2_0 || NET45 || NET46
                if (memberInfo.ReflectedType != typeof(TEntity) &&
                    !typeof(TEntity).IsAssignableFrom(memberInfo.ReflectedType))
                {
                    throw new ArgumentException("Property selector does not refer to a property of the entity.");
                }
#endif
            return memberInfo.Name;
        }
    }
}