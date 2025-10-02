namespace Service.OperationResult
{
	internal static class FindErrors
	{
		public static Error NotFound(long id) => new Error($"The entity with Id '{id}' was not found", ErrorType.NotFound);
		public static Error NotFound(long id, Type type) => new Error($"The {type.Name} entity with Id '{id}' was not found", ErrorType.NotFound);

		public static Error NotFoundByPredicate() => new Error("The entity was not found by condition", ErrorType.NotFound);
		public static Error NotFoundByPredicate(Type type) => new Error($"The {type.Name} entity was not found by condition", ErrorType.NotFound);

        public static Error NotFoundBySerialNumber(string serialNumber, Type type) => new Error($"There is not one {type.Name} entity with serial number - {serialNumber}", ErrorType.NotFound);

        public static Error NotFoundCollectionOfEntities(Type type) => new Error($"The collection of {type.Name} entities was not found", ErrorType.NotFound);

		public static Error NotFoundCollectionOfEntities(IEnumerable<long> missedIds, Type type) => new Error($"The collection of {type.Name} entities was not found. The missing ids: {string.Join(',', missedIds)}", ErrorType.NotFound);
	}

	internal static class ToolMovementErrors
	{
        public static Error NotEnoughToolsInStock(long toolId, long requested, long inStock) => new Error($"Not enough tools (toolId = {toolId}) in stock: requested - {requested}, in stock - {inStock}", ErrorType.Invalid);

		public static Error ExtraToolQuantity(long toolId) => new Error($"The quantity of tools (toolId = {toolId}) after returning exceeded total quantity", ErrorType.Invalid);
	}

	internal static class EntityErrors
	{
		public static Error SaveToDbError(Type type) => new Error($"Update to database error. Entity type: {type.Name}", ErrorType.InternalError);
	}

	internal static class LinkErrors
	{
		public static Error NotFoundLinkClass(long linkClassId) => new Error($"The link class with Id '{linkClassId}' was not found", ErrorType.NotFound);

		public static Error NotFoundLinkEntities(params long[] ids) => new Error($"The entities to link with Ids: '{string.Join(',', ids)}' were not found", ErrorType.NotFound);

		public static Error NotFoundEntityToLink(long id, string entityName) => new Error($"The {entityName} entity to link with Id: '{id}' was not found", ErrorType.NotFound);

		public static Error AlreadyExists() => new Error($"Link with current params already exists", ErrorType.Invalid);
	}
}
