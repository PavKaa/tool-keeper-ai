namespace Service.OperationResult
{
	public class Result
	{
		protected Result(bool isSuccess, Error error)
		{
			if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
			{
				throw new ArgumentException("Invalid error", nameof(error));
			}

			IsSuccess = isSuccess;
			Error = error;
		}

		public bool IsSuccess { get; }

		public Error? Error { get; }

		public static Result Success() => new Result(true, Error.None);

		public static Result Failure(Error error) => new Result(false, error);
	}

	public class Result<T> : Result
	{
		public Result() : base(true, Error.None)
		{

		}

		protected Result(bool isSuccess, Error error, T? data) : base(isSuccess, error)
		{
			Data = data;
		}

		public T? Data { get; }

		public static Result<T> Success(T data) => new Result<T>(true, Error.None, data);

		public static Result<T> Failure(Error error, T? data = default) => new Result<T>(false, error, data);
	}

	public sealed record Error(string Description, ErrorType Type)
	{
		public static readonly Error None = new Error(string.Empty, ErrorType.None);

		public string ConvertToStatusCode()
		{
			switch (Type)
			{
				case ErrorType.NotFound:
					return "NotFound";
				case ErrorType.Invalid:
					return "BadRequest";
				case ErrorType.InternalError:
					return "InternalError";
				default:
					throw new Exception("An unhandled result has occurred as a result of a service call.");
			}
		}
	}

	public enum ErrorType
	{
		None,
		Invalid,
		NotFound,
		InternalError
	}
}
