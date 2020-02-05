using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using MicroOrm.Dapper.Repositories.SqlGenerator;
using MicroOrm.Dapper.Repositories.SqlGenerator.Filters;

namespace MicroOrm.Dapper.Repositories
{
    /// <summary>
    ///     Base ReadOnlyRepository
    /// </summary>
    public partial class ReadOnlyDapperRepository<TEntity> : IReadOnlyDapperRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ReadOnlyDapperRepository(IDbConnection connection)
        {
            Connection = connection;
            FilterData = new FilterData();
            SqlGenerator = new SqlGenerator<TEntity>();
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public ReadOnlyDapperRepository(IDbConnection connection, ISqlGenerator<TEntity> sqlGenerator)
        {
            Connection = connection;     
            FilterData = new FilterData();
            SqlGenerator = sqlGenerator;
        }

        /// <inheritdoc />
        public IDbConnection Connection { get; set; }
        
        /// <inheritdoc />
        public FilterData FilterData { get; set; }
        
        /// <inheritdoc />
        public ISqlGenerator<TEntity> SqlGenerator { get; }

        private static string GetProperty(Expression expression, Type type)
        {
            var field = (MemberExpression) expression;

            var prop = type.GetProperty(field.Member.Name);
            TypeInfo declaringType = type.GetTypeInfo();
            TableAttribute tableAttribute = declaringType.GetCustomAttribute<TableAttribute>();
            string tableName = tableAttribute != null ? tableAttribute.Name : declaringType.Name;
            
            if (prop.GetCustomAttribute<NotMappedAttribute>() != null) 
                return string.Empty;
            
            string name = prop.GetCustomAttribute<ColumnAttribute>()?.Name ?? prop.Name;
            return $"{tableName}.{name}";
        }
        
        /// <inheritdoc />
        public void Dispose()
        {
            Connection.Dispose();
            Connection = null;
            if (FilterData == null) return;
            FilterData.LimitInfo = null;
            if (FilterData.OrderInfo != null)
            {
                FilterData.OrderInfo.Columns.Clear();
                FilterData.OrderInfo.Columns = null;
                FilterData.OrderInfo = null;
            }
            if (FilterData.SelectInfo != null)
            {
                FilterData.SelectInfo.Columns.Clear();
                FilterData.SelectInfo.Columns = null;
                FilterData.SelectInfo = null;
            }

            FilterData = null;
        }
        
    }
}
