using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Domain
{
    // Для операций, которые просто выполняют действие (добавление, удаление, обновление)
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }

        // Защита от дурака
        protected Result(bool isSuccess, string error)
        {
            if (isSuccess && !string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Успешный результат не может содержать ошибку.");

            if (!isSuccess && string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Неудачный результат должен содержать описание ошибки.");

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, string.Empty);
        public static Result Failure(string error) => new(false, error);
    }

    // Для операций, которые должны вернуть данные из сервиса (например, GetByIdAsync)
    public class Result<T> : Result
    {
        private readonly T? _value;

        // Защита от дурака
        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Невозможно получить значение неудавшегося результата.");

        private Result(T? value, bool isSuccess, string error) : base(isSuccess, error)
        {
            _value = value;
        }

        public static Result<T> Success(T value) => new(value, true, string.Empty);
        public static new Result<T> Failure(string error) => new(default, false, error);
    }
}
