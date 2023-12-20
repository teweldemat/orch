using orch.core.ef.System.DTOs;
using FluentValidation;

namespace orch.core.ef.Transaction.EntityValidators.DTOs
{
    public class CreateAcessTokenRequestValidator : OAbstractValidator<CreateAccessTokenRequest>
    {
        public CreateAcessTokenRequestValidator()
        {
            RuleFor(Request => Request.UserName).NotEmpty();
            RuleFor(Request => Request.Password).NotEmpty();
        }
    }
}