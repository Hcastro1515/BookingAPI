using System.Data;
using AlestheticApi.Models.DTOs;
using FluentValidation;

namespace AlestheticApi.Validators;

public class AppointmentValidator : AbstractValidator<AppointmentDTO>
{
    public AppointmentValidator()
    {
        RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage("Customer Id is required");
        RuleFor(x => x.EmployeeId).NotNull().NotEmpty().WithMessage("Employee Id is required");
        RuleFor(x => x.ServiceId).NotNull().NotEmpty().WithMessage("Service Id is required");
        RuleFor(x => x.AppointmentDateTime).NotNull().NotEmpty().WithMessage("Appointment Date Time is required");
        RuleFor(x => x.Status).NotNull().NotEmpty().WithMessage("Status is required");
    }
}