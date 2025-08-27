
namespace Shared.Common.Base
{
    public class BaseResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T? Value { get; set; }
        public Error Error { get; set; }
        public BaseResponse(bool isSuccess, T? value = default, Error error = default!)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }
        /// <summary>
        /// Creates a successful response with a default value for the data.
        /// </summary>
        /// <returns>A <see cref="BaseResponse{T}"/> instance representing a successful operation, with the default value of type
        /// <typeparamref name="T"/> as the data.</returns>
        public static BaseResponse<T> Success() => new BaseResponse<T>(true);

        /// <summary>
        /// Creates a successful response containing the specified value.
        /// </summary>
        /// <param name="value">The value to include in the response.</param>
        /// <returns>A <see cref="BaseResponse{T}"/> instance representing a successful operation, containing the provided value.</returns>
        public static BaseResponse<T> Success(T value) => new BaseResponse<T>(true, value);
        /// <summary>
        /// Creates a failed response with the specified error.
        /// </summary>
        /// <param name="error">The error that describes the reason for the failure. Cannot be null.</param>
        /// <returns>A <see cref="BaseResponse{T}"/> instance representing a failed operation, with the specified error and a
        /// default value for the result.</returns>
        public static BaseResponse<T> Failure(Error error) => new BaseResponse<T>(false, default, error);
    }
}
