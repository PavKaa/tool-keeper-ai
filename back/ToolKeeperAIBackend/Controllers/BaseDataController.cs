using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Service.Abstraction;
using Service.Model;
using System.Linq.Expressions;
using ToolKeeperAIBackend.Extensions;

namespace ToolKeeperAIBackend.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class BaseDataController<T, TDto, TPatchDto> : ControllerBase
        where T : BaseModel
        where TDto : class
        where TPatchDto : class
    {
        protected readonly IBaseEntityService<T, TDto> _service;
        protected readonly ILogger<BaseDataController<T, TDto, TPatchDto>> _logger;
        protected readonly IMapper _mapper;

        protected BaseDataController(IBaseEntityService<T, TDto> service, IMapper mapper, ILogger<BaseDataController<T, TDto, TPatchDto>> logger)
        {
            _service = service;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public virtual ActionResult<IQueryable<T>> GetAll() => Ok(_service.Get());

        [HttpGet("{key:long}")]
        public virtual async Task<IActionResult> GetById([FromRoute] long key)
        {
            var result = await _service.GetByExpressionAsEnumerableAsync(GetPredicatToFindById(key));

            return this.FromResult(result);
        }

        [HttpDelete("{key:long}")]
        public virtual async Task<IActionResult> Delete([FromRoute] long key)
        {
            var result = await _service.DeleteAsync(key);

            return this.FromResult(result);
        }

        [HttpPost]
        public virtual async Task<ActionResult> Create([FromBody] TDto dto)
        {
            var result = await _service.CreateAsync(dto);

            if (!result.IsSuccess)
                return this.FromResult(result);

            var query = await _service.GetByExpressionAsEnumerableAsync(GetPredicatToFindById(result.Data!));

            return this.FromResult(query);
        }

        [HttpPatch("{key:long}")]
        public virtual async Task<IActionResult> Change([FromRoute] long key, [FromBody] TPatchDto updateDto)
        {
            var dto = _mapper.Map<TDto>(updateDto);

            var result = await _service.UpdateAsync(key, dto);

            if (!result.IsSuccess)
                return this.FromResult(result);

            var query = await _service.GetByExpressionAsEnumerableAsync(GetPredicatToFindById(key));

            return this.FromResult(query);
        }

        protected virtual Expression<Func<T, bool>> GetPredicatToFindById(long key) => entity => entity.Id == key;

        protected virtual Expression<Func<T, bool>> GetPredicatToFindById(T entity) => dbEntity => dbEntity.Id == entity.Id;
    }
}
