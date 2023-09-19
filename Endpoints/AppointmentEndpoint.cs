using System.Net;
using AlestheticApi.Models;
using AlestheticApi.Models.DTOs;
using AlestheticApi.Repository.IRepository;
using AlestheticApi.Validators;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlestheticApi.Endpoints;

public static class AppointmentEndpoint
{

    public static void ConfigureAppointmentEndpoint(this WebApplication app)
    {
        app.MapGet("/api/appointment", GetAllAppointments)
        .RequireAuthorization()
        .WithName("GetAllAppointments")
        .Produces<APIResponse>(200)
        .Produces(401)
        .Produces(500);

        app.MapGet("/api/appointment/{id:int}", GetAppointmentById)
        .RequireAuthorization()
        .WithName("GetAppointmentById")
        .Produces<APIResponse>(200)
        .Produces(401)
        .Produces(500);

        app.MapPost("/api/appointment", CreateAppointment)
        .WithName("CreateAppointment")
        .Accepts<AppointmentDTO>("application/json")
        .Produces<APIResponse>(201)
        .Produces(400);


        app.MapDelete("/api/appointment/{id:int}", RemoveAppointment)
        .WithName("DeleteAppointment")
        .Produces<APIResponse>(200)
        .Produces(401)
        .Produces(500);

        app.MapPut("/api/appointment", UpdateAppointment)
        .RequireAuthorization()
        .WithName("UpdateAppointment")
        .Accepts<Appointment>("application/json")
        .Produces<APIResponse>(200)
        .Produces(401)
        .Produces(500);
    }

    // GetAllApointments method
    private static async Task<IResult> CreateAppointment(ICRUDRepository<Appointment> appointmentRepository, ILogger<Program> logger, IMapper map, [FromBody] AppointmentDTO appointmentDto, AlestheticDataContext context)
    {
        var validator = new AppointmentValidator();
        var validationResult = await validator.ValidateAsync(appointmentDto);

        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage);
            return Results.BadRequest(new APIResponse
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.BadRequest,
                ErrorMessages = errorMessages.ToList()
            });
        }

        try
        {
            var existingAppointment = await context.Appointments.FirstOrDefaultAsync(a => a.AppointmentDateTime == appointmentDto.AppointmentDateTime);
            if (existingAppointment != null)
            {
                return Results.BadRequest(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "An appointment already exists for that time and date"
                });
            }

            var appointment = map.Map<Appointment>(appointmentDto);
            appointment.Customer = await context.Customers.FindAsync(appointmentDto.CustomerId);
            appointment.Employee = await context.Employees.FindAsync(appointmentDto.EmployeeId);
            appointment.Service = await context.Services.FindAsync(appointmentDto.ServiceId);
            await appointmentRepository.CreateAsync(appointment);
            await appointmentRepository.SaveAsync();

            var createdAppointmentDto = map.Map<AppointmentDTO>(appointment);

            return Results.Ok(new APIResponse
            {
                IsSuccess = true,
                Message = "Appointment created successfully",
                StatusCode = HttpStatusCode.Created,
                Result = createdAppointmentDto
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the appointment");
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    private static async Task<IResult> GetAppointmentById(AlestheticDataContext context, ILogger<Program> logger, int id)
    {
        APIResponse response = new();

        try
        {
            logger.Log(LogLevel.Information, "GetAppointmentById called");
            var appointment = await context.Appointments
            .Include(x => x.Customer)
            .Include(x => x.Employee)
            .Include(x => x.Service)
            .Select(x => x.AppointmentId == id)
            .FirstOrDefaultAsync();

            if (appointment == false)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "Appointment not found" };
                return Results.NotFound(response);
            }

            response.Result = appointment;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }
        catch (System.Exception)
        {
            logger.LogError("An error occurred while getting the appointment");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while getting the appointment" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
    private static async Task<IResult> GetAllAppointments(ILogger<Program> logger, AlestheticDataContext context, IMapper map, int pageNumber, int pageSize)
    {
        APIResponse response = new();
        try
        {
            logger.Log(LogLevel.Information, "GetAllAppointments called");
            int totalCount = await context.Appointments.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if (pageNumber < 1 || pageNumber > totalPages)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "Invalid page number" };
                return Results.NotFound(response);
            }

            var appointments = await context.Appointments
            .Include(x => x.Customer)
            .Include(x => x.Employee)
            .Include(x => x.Service)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            if (appointments.Count == 0)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "No Appointments found" };
                return Results.NotFound(response);
            }


            response.Result = appointments;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting appointments");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while getting the appointments" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    private static async Task<IResult> RemoveAppointment(ICRUDRepository<Appointment> appointmentRepository, ILogger<Program> logger, IMapper map, int id)
    {
        APIResponse response = new();
        try
        {
            var appointment = await appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "Appointment not found" };
                return Results.NotFound(response);
            }

            await appointmentRepository.RemoveAsync(appointment);
            await appointmentRepository.SaveAsync();

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Message = "Appointment was deleted Successfully";
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while removing appointment");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while removing appointment" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    private static async Task<IResult> UpdateAppointment(ICRUDRepository<Appointment> appointmentRepository, ILogger<Program> logger, IMapper map, [FromBody] AppointmentDTO appointmentDto, int id)
    {
        APIResponse response = new();
        try
        {
            Appointment? existingAppointment = await appointmentRepository.GetByIdAsync(id);
            if (existingAppointment == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "Appointment not found" };
                return Results.NotFound(response);
            }

            existingAppointment.ServiceId = appointmentDto.ServiceId;
            existingAppointment.CustomerId = appointmentDto.CustomerId;
            existingAppointment.EmployeeId = appointmentDto.EmployeeId;
            existingAppointment.AppointmentDateTime = appointmentDto.AppointmentDateTime;
            existingAppointment.Notes = appointmentDto.Notes;
            existingAppointment.Status = appointmentDto.Status;

            await appointmentRepository.UpdateAsync(existingAppointment);
            await appointmentRepository.SaveAsync();

            AppointmentDTO AppointmentDto = map.Map<AppointmentDTO>(existingAppointment);

            response.Result = AppointmentDto;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the appointment");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while updating the appointment" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
}