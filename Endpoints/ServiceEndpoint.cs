using System.Net;
using AlestheticApi.Models;
using AlestheticApi.Models.DTOs;
using AlestheticApi.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AlestheticApi.Endpoints;

public static class ServiceEndpoint
{
    public static void ConfigureServiceEndpoint(this WebApplication app)
    {
        app.MapGet("/api/service", GetAllServices)
        .WithName("GetAllServices")
        .Produces<APIResponse>(200);

        app.MapGet("/api/service/{id:int}", GetServiceById)
        .WithName("GetServiceById")
        .Produces<APIResponse>(200)
        .Produces(500);

        app.MapPost("/api/service", CreateService)
        .WithName("CreateService")
        .Accepts<ServiceDTO>("application/json")
        .Produces<APIResponse>(201)
        .Produces(400);

        app.MapDelete("/api/service/{id:int}", DeleteService)
        .WithName("DeleteService")
        .Produces<APIResponse>(200)
        .Produces(500);

        app.MapPut("/api/service", UpdateService)
        .WithName("UpdateService")
        .Accepts<Service>("application/json")
        .Produces<APIResponse>(200)
        .Produces(500);
    }

    private async static Task<IResult> GetServiceById(ICRUDRepository<Service> serviceRepository, ILogger<Program> _logger, int id)
    {
        APIResponse response = new();
        try
        {
            _logger.Log(LogLevel.Information, "GetServiceById called");
            response.Result = await serviceRepository.GetByIdAsync(id);

            if (response.Result == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "Service not found" };
                return Results.NotFound(response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting service by id");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while getting service by id" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
    private async static Task<IResult> GetAllServices(ICRUDRepository<Service> serviceRepository, ILogger<Program> _logger)
    {
        APIResponse response = new();
        try
        {
            _logger.Log(LogLevel.Information, "GetAllServices called");
            response.Result = await serviceRepository.GetAllAsync();

            if (response.Result == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "No services found" };
                return Results.NotFound(response);
            }
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting services");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while getting the services" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    private async static Task<IResult> CreateService(ICRUDRepository<Service> serviceRepository,ILogger<Program> _logger, IMapper _map, [FromBody] ServiceDTO serviceDTO)
    {
        APIResponse response = new();

        try
        {
            var existingService= await serviceRepository.GetByName(serviceDTO.ServiceName);
            if (existingService != null)
            {
                response.Message = "Service already exists";
                return Results.BadRequest(response);
            }

            Service service = _map.Map<Service>(serviceDTO);

            await serviceRepository.CreateAsync(service);
            await serviceRepository.SaveAsync();

            ServiceDTO serviceDto = _map.Map<ServiceDTO>(service);


            response.Result = serviceDto;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.Created;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the service");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while creating service" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    private async static Task<IResult> DeleteService(ICRUDRepository<Service> serviceRepository, ILogger<Program> _logger, IMapper _map, int id)
    {
        APIResponse response = new();
        try
        {
            var service = await serviceRepository.GetByIdAsync(id);
            if (service == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "Service not found" };
                return Results.NotFound(response);
            }

            await serviceRepository.RemoveAsync(service);
            await serviceRepository.SaveAsync();

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Message = "Service was deleted Successfully";
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting service");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() {"An error occurred while deleting service"};
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    private async static Task<IResult> UpdateService(ICRUDRepository<Service> serviceRepository, ILogger<Program> _logger, IMapper _map, [FromBody] ServiceDTO serviceDTO, int id)
    {
        APIResponse response = new();
        try
        {
            Service? existingService = await serviceRepository.GetByIdAsync(id);
            if (existingService == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() {"Service not found"};
                return Results.NotFound(response);
            }

            existingService.ServiceName = serviceDTO.ServiceName;
            existingService.Description= serviceDTO.Description;
            existingService.Price = serviceDTO.Price;
            existingService.Duration = serviceDTO.Duration;
            
            await serviceRepository.UpdateAsync(existingService);
            await serviceRepository.SaveAsync();

            ServiceDTO serviceDto = _map.Map<ServiceDTO>(existingService);

            response.Result = serviceDto;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the service");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while updating service" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
}