using Service.OperationResult;
using System.Linq.Expressions;

namespace Service.Abstraction
{
	public interface IBaseEntityService<TEntity, TDto>
		where TEntity : class
		where TDto : class
	{
		IEnumerable<TEntity> Get();

		Task<Result<TEntity>> GetByIdAsync(long id);

		Task<Result<IEnumerable<TEntity>>> GetByExpressionAsEnumerableAsync(Expression<Func<TEntity, bool>> predicate);

		Task<Result<TEntity>> CreateAsync(TDto dto);

		Task<Result> UpdateAsync(long id, TDto dto);

		Task<Result> DeleteAsync(long id);
	}
}
