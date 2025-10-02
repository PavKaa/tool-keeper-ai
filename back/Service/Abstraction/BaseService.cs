using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Service.Db;
using Service.Exceptions;
using Service.OperationResult;
using System.Linq.Expressions;

namespace Service.Abstraction
{
	public abstract class BaseService<TEntity, TDto> : IBaseEntityService<TEntity, TDto>
		where TDto : class
		where TEntity : class
	{
		protected ToolKeeperDbContext _dbContext;
		protected DbSet<TEntity> _dbSet;
		protected readonly IMapper _mapper;

		protected BaseService(ToolKeeperDbContext dbContext, IMapper mapper)
        {
			_dbContext = dbContext;
			_dbSet = dbContext.Set<TEntity>();
			_mapper = mapper;	
        }

		// TODO: restricment for amount of querying entities (through injected settings maybe)
		public virtual IEnumerable<TEntity> Get()
		{
			return _dbSet.AsNoTracking().ToList();
		}

		public virtual async Task<Result<TEntity>> GetByIdAsync(long id)
		{
			var entity = await _dbSet.FindAsync(id);

			return entity == null 
				? Result<TEntity>.Failure(FindErrors.NotFound(id, typeof(TEntity)))
				: Result<TEntity>.Success(entity);
		}

		public virtual async Task<Result<IEnumerable<TEntity>>> GetByExpressionAsEnumerableAsync(Expression<Func<TEntity, bool>> predicate)
		{
			var data = await _dbSet.Where(predicate).AsNoTracking().ToListAsync();

			return data == null || data.Count == 0 
				? Result<IEnumerable<TEntity>>.Failure(FindErrors.NotFoundByPredicate(typeof(TEntity)))
				: Result<IEnumerable<TEntity>>.Success(data);
		}

		public virtual async Task<Result<TEntity>> CreateAsync(TDto dto)
		{
			try
			{
				var newEntity = _mapper.Map<TEntity>(dto);

				_dbSet.Add(newEntity);

				var added = await _dbContext.SaveChangesAsync();

				return added > 0 
					? Result<TEntity>.Success(newEntity)
					: Result<TEntity>.Failure(EntityErrors.SaveToDbError(typeof(TEntity)));
			}
			catch (DbUpdateException ex)
			{
				throw new DatabaseException($"Failed to create entity: {ex.Message}", typeof(TEntity));
			}
		}

		public virtual async Task<Result> UpdateAsync(long id, TDto dto)
		{
			var getResult = await GetByIdAsync(id);

			if (!getResult.IsSuccess)
				return Result.Failure(FindErrors.NotFound(id));

			var dbEntity = getResult.Data!;

			try
			{
				_mapper.Map(dto, dbEntity);

				var updated = await _dbContext.SaveChangesAsync();

				return updated > 0
					? Result.Success()
					: Result.Failure(EntityErrors.SaveToDbError(typeof(TEntity)));
			}
			catch (DbUpdateException ex)
			{
				throw new DatabaseException($"Failed to update entity: {ex.Message}", typeof(TEntity));
			}
		}

		public virtual async Task<Result> DeleteAsync(long id)
		{
			var getResult = await GetByIdAsync(id);

			if (!getResult.IsSuccess)
				return Result.Failure(FindErrors.NotFound(id, typeof(TEntity)));

			var entity = getResult.Data!;

			try
			{
				_dbSet.Remove(entity);
				var deleted = await _dbContext.SaveChangesAsync();

				return deleted > 0
					? Result.Success()
					: Result.Failure(EntityErrors.SaveToDbError(typeof(TEntity)));
			}
			catch (DbUpdateException ex)
			{
				throw new DatabaseException($"Failed to delete entity: {ex.Message}", typeof(TEntity));
			}
		}

	}
}
