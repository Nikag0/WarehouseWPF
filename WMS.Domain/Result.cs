namespace WMS.Domain
{
    // Для операций, которые просто выполняют действие (добавление, удаление, обновление)
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }

        // Защита от дурака
        private Result(bool isSuccess, string error)
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
}
