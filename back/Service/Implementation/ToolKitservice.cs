using AutoMapper;
using Service.Abstraction;
using Service.Db;
using Service.Dto.Create;
using Service.Model;

namespace Service.Implementation
{
	public class ToolKitService : BaseService<ToolKit, ToolKitDto>, IToolKitService
	{
        public ToolKitService(IMapper mapper, ToolKeeperDbContext dbContext) : base(dbContext, mapper)
        {
            
        }
    }
}
