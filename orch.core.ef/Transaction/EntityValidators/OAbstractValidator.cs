using FluentValidation;

namespace orch.core.ef.Transaction.EntityValidators
{
    /// <summary>
    /// An abstract class that extends the FluentValidation AbstractValidator class.
    /// Provides a method for validating a specific property of a model asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of model to validate.</typeparam>
    public abstract class OAbstractValidator<T> : AbstractValidator<T>
    {
        /// <summary>
        /// Gets a function that validates a specific property of a model asynchronously.
        /// </summary>
        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<T>.CreateWithOptions((T)model, strategy => strategy.IncludeProperties(propertyName)));
            if (result.IsValid)
                return Array.Empty<string>();
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}