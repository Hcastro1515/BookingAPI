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
    private async static Task<IResult> CreateAppointment(ICRUDRepository<Appointment> appointmentRepository, ILogger<Program> _logger, IMapper _map, [FromBody] AppointmentDTO appointmentDTO, AlestheticDataContext _context)
    {
        var validator = new AppointmentValidator();
        var validationResult = validator.Validate(appointmentDTO);

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
            var existingAppointment = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentDateTime == appointmentDTO.AppointmentDateTime);
            if (existingAppointment != null)
            {
                return Results.BadRequest(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "An appointment already exists for that time and date"
                });
            }

            var appointment = _map.Map<Appointment>(appointmentDTO);
            appointment.Customer = await _context.Customers.FindAsync(appointmentDTO.CustomerId);
            appointment.Employee = await _context.Employees.FindAsync(appointmentDTO.EmployeeId);
            appointment.Service = await _context.Services.FindAsync(appointmentDTO.ServiceId);
            await appointmentRepository.CreateAsync(appointment);
            await appointmentRepository.SaveAsync();

            var createdAppointmentDTO = _map.Map<AppointmentDTO>(appointment);

            return Results.Ok(new APIResponse
            {
                IsSuccess = true,
                Message = "Appointment created successfully",
                StatusCode = HttpStatusCode.Created,
                Result = createdAppointmentDTO
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the appointment");
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    private async static Task<IResult> GetAppointmentById(AlestheticDataContext _context, ILogger<Program> _logger, int id)
    {
        APIResponse response = new();

        try
        {
            _logger.Log(LogLevel.Information, "GetAppointmentById called");
            var appointment = await _context.Appointments
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
            _logger.LogError("An error occurred while getting the appointment");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while getting the appointment" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
    private async static Task<IResult> GetAllAppointments(ILogger<Program> _logger, AlestheticDataContext _context, IMapper _map, int pageNumber, int pageSize)
    {
        APIResponse response = new();
        try
        {
            _logger.Log(LogLevel.Information, "GetAllAppointments called");
            int totalCount = await _context.Appointments.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if (pageNumber < 1 || pageNumber > totalPages)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "Invalid page number" };
                return Results.NotFound(response);
            }

            var appointments = await _context.Appointments
            .Include(x => x.Customer)
            .Include(x => x.Employee)
            .Include(x => x.Service)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            if (appointments == null || appointments.Count == 0)
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
            _logger.LogError(ex, "An error occurred while getting appointments");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while getting the appointments" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    // private async static Task<IResult> CreateAppointment(ICRUDRepository<Appointment> appointmentRepository, ILogger<Program> _logger, IMapper _map, [FromBody] AppointmentDTO appointmentDTO, AlestheticDataContext _context)
    // {
    //     APIResponse response = new();

    //     try
    //     {
    //         var validator = new AppointmentValidator();
    //         var validationResult = await validator.ValidateAsync(appointmentDTO);

    //         if (!validationResult.IsValid)
    //         {
    //             response.IsSuccess = false;
    //             response.StatusCode = HttpStatusCode.BadRequest;
    //             response.ErrorMessages = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
    //             return Results.BadRequest(response);
    //         }

    //         var existingAppointment = await _context.Appointments.Where(a => a.AppointmentDateTime == appointmentDTO.AppointmentDateTime).FirstOrDefaultAsync();
    //         if (existingAppointment != null)
    //         {
    //             response.Message = "appointment already exists for that time and date";
    //             return Results.BadRequest(response);
    //         }

    //         Appointment appointment = _map.Map<Appointment>(appointmentDTO);
    //         appointment.Customer = await _context.Customers.Where(c => c.CustomerId == appointmentDTO.CustomerId).FirstOrDefaultAsync();
    //         appointment.Employee = await _context.Employees.Where(e => e.EmployeeId == appointmentDTO.EmployeeId).FirstOrDefaultAsync();
    //         appointment.Service = await _context.Services.Where(s => s.ServiceId == appointmentDTO.ServiceId).FirstOrDefaultAsync();
    //         await appointmentRepository.CreateAsync(appointment);
    //         await appointmentRepository.SaveAsync();

    //         AppointmentDTO appointmentDto = _map.Map<AppointmentDTO>(appointment);


    //         response.Result = appointmentDto;
    //         response.IsSuccess = true;
    //         response.StatusCode = HttpStatusCode.Created;
    //         return Results.Ok(response);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "An error occurred while creating the appointment");
    //         response.IsSuccess = false;
    //         response.StatusCode = HttpStatusCode.InternalServerError;
    //         response.ErrorMessages = new() { "An error occurred while creating appointment" };
    //         return Results.StatusCode((int)HttpStatusCode.InternalServerError);
    //     }
    // }

    private async static Task<IResult> RemoveAppointment(ICRUDRepository<Appointment> appointmentRepository, ILogger<Program> _logger, IMapper _map, int id)
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
            _logger.LogError(ex, "An error occurred while removing appointment");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while removing appointment" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    private async static Task<IResult> UpdateAppointment(ICRUDRepository<Appointment> appointmentRepository, ILogger<Program> _logger, IMapper _map, [FromBody] AppointmentDTO appointmentDTO, int id)
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

            existingAppointment.ServiceId = appointmentDTO.ServiceId;
            existingAppointment.CustomerId = appointmentDTO.CustomerId;
            existingAppointment.EmployeeId = appointmentDTO.EmployeeId;
            existingAppointment.AppointmentDateTime = appointmentDTO.AppointmentDateTime;
            existingAppointment.Notes = appointmentDTO.Notes;
            existingAppointment.Status = appointmentDTO.Status;

            await appointmentRepository.UpdateAsync(existingAppointment);
            await appointmentRepository.SaveAsync();

            AppointmentDTO AppointmentDto = _map.Map<AppointmentDTO>(existingAppointment);

            response.Result = AppointmentDto;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the appointment");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while updating the appointment" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
}