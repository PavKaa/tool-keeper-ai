using Microsoft.AspNetCore.Mvc;
using Service.OperationResult;

namespace ToolKeeperAIBackend.Extensions
{
    public static class ControllerExtensions
    {
        public static ActionResult FromResult<T>(this ControllerBase controller, Result<T> result, ILogger logger)
        {
            if (result.IsSuccess)
                return controller.Ok(result.Data);
            else
            {
                logger.LogWarning(message: "Non successful response", result.Error);

                return GetErrorResult(controller, result.Error!);
            }
        }

        public static ActionResult FromResult(this ControllerBase controller, Result result)
        {
            if (result.IsSuccess)
                return controller.Ok();
            else
                return GetErrorResult(controller, result.Error!);
        }

        private static ActionResult GetErrorResult(ControllerBase controller, Error error)
        {
            var errorDescription = error.Description;

            switch (error.Type)
            {
                case ErrorType.NotFound:
                    return controller.NotFound(errorDescription);
                case ErrorType.Invalid:
                    return controller.BadRequest(errorDescription);
                case ErrorType.InternalError:
                    return controller.StatusCode(StatusCodes.Status500InternalServerError, errorDescription);
                default:
                    throw new Exception("An unhandled result has occurred as a result of a service call.");
            }
        }
    }
}
