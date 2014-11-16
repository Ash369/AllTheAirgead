/*  http://blogs.msdn.com/b/jpsanders/archive/2014/07/09/walkthrough-attaching-an-azure-sql-database-to-you-net-backend.aspx    */

using AutoMapper;
using AutoMapper.Impl;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Tables;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.OData;

namespace alltheairgeadmobileService.Models
{

    //Utilities for Hooking up models
    public static class MySqlFuncs
    {
        [DbFunction("SqlServer", "STR")]
        public static string StringConvert(long number)
        {
            return number.ToString();
        }
        [DbFunction("SqlServer", "LTRIM")]
        public static string LTRIM(string s)
        {
            return s == null ? null : s.TrimStart();
        }
        // Can only be used locally.
        public static long LongParse(string s)
        {
            long ret;
            long.TryParse(s, out ret);
            return ret;
        }
    }

    public class SimpleMappedEntityDomainManager<TData, TModel>
        : MappedEntityDomainManager<TData, TModel>
        where TData : class, ITableData, new()
        where TModel : class
    {
        private Expression<Func<TModel, object>> dbKeyProperty;

        public SimpleMappedEntityDomainManager(DbContext context,
            HttpRequestMessage request, ApiServices services,
            Expression<Func<TModel, object>> dbKeyProperty)
            : base(context, request, services)
        {
            this.dbKeyProperty = dbKeyProperty;
        }
        public override SingleResult<TData> Lookup(string id)
        {
            return this.LookupEntity(GeneratePredicate(id));
        }
        public override async Task<TData> UpdateAsync(string id, Delta<TData> patch)
        {
            return await this.UpdateEntityAsync(patch, ConvertId(id));
        }
        public override Task<bool> DeleteAsync(string id)
        {
            return this.DeleteItemAsync(ConvertId(id));
        }

        private static Expression<Func<TModel, bool>> GeneratePredicate(string id)
        {
            var m = Mapper.FindTypeMapFor<TModel, TData>();
            var pmForId = m.GetExistingPropertyMapFor(new PropertyAccessor(typeof(TData).GetProperty("Id")));
            var keyString = pmForId.CustomExpression;
            var predicate = Expression.Lambda<Func<TModel, bool>>(
                Expression.Equal(keyString.Body, Expression.Constant(id)),
                keyString.Parameters[0]);
            return predicate;
        }

        private object ConvertId(string id)
        {
            var m = Mapper.FindTypeMapFor<TData, TModel>();
            var keyPropertyAccessor = GetPropertyAccessor(this.dbKeyProperty);
            var pmForId = m.GetExistingPropertyMapFor(new PropertyAccessor(keyPropertyAccessor));
            TData tmp = new TData() { Id = id };
            var convertedId = pmForId.CustomExpression.Compile().DynamicInvoke(tmp);
            return convertedId;
        }

        private PropertyInfo GetPropertyAccessor(Expression exp)
        {
            if (exp.NodeType == ExpressionType.Lambda)
            {
                var lambda = exp as LambdaExpression;
                return GetPropertyAccessor(lambda.Body);
            }
            else if (exp.NodeType == ExpressionType.Convert)
            {
                var convert = exp as UnaryExpression;
                return GetPropertyAccessor(convert.Operand);
            }
            else if (exp.NodeType == ExpressionType.MemberAccess)
            {
                var propExp = exp as System.Linq.Expressions.MemberExpression;
                return propExp.Member as PropertyInfo;
            }
            else
            {
                throw new InvalidOperationException("Unexpected expression node type: " + exp.NodeType);
            }
        }
    }
}