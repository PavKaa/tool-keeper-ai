namespace Service.Exceptions
{
	public abstract class AppException : Exception
	{
		public abstract int StatusCode { get; }
		public abstract string ErrorType { get; }

		protected AppException(string message) : base(message) { }
	}

	public class NotFoundException : AppException
	{
		public override int StatusCode => 404;
		public override string ErrorType => "NotFound";

		public NotFoundException(string entityName, object id)
			: base($"{entityName} with id {id} was not found") { }
	}

	public class DatabaseException : AppException
	{
		public override int StatusCode => 500;
		public override string ErrorType => "DatabaseError";
        public Type EntityType { get; }

        public DatabaseException(string message, Type entityType) : base(message) 
		{
			EntityType = entityType;
		}
	}
}
