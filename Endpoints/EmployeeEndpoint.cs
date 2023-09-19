using System.Net;
using AlestheticApi.Models;
using AlestheticApi.Models.DTOs;
using AlestheticApi.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AlestheticApi.Endpoints;

public static class EmployeeEndpoint
{
    public static void ConfigureEmployeeEndpoint(this WebApplication app)
    {
        app.MapGet("/api/employee", GetAllEmployees)
        .WithName("GetAllEmployee")
        .Produces<APIResponse>(200);

        app.MapGet("/api/employee/{id:int}", GetEmployeeById)
        .WithName("GetEmployeeById")
        .Produces<APIResponse>(200)
        .Produces(500);

        app.MapPost("/api/employee", CreateEmployee)
        .WithName("CreateEmployee")
        .Accepts<ServiceDTO>("application/json")
        .Produces<APIResponse>(201)
        .Produces(400);

        app.MapDelete("/api/employee/{id:int}/{_map}", DeleteEmployee)
        .WithName("DeleteEmployee")
        .Produces<APIResponse>(200)
        .Produces(500);

        app.MapPut("/api/employee", UpdateEmployee)
        .WithName("UpdateEmployee")
        .Accepts<Service>("application/json")
        .Produces<APIResponse>(200)
        .Produces(500);
    }

    private static async Task<IResult> GetEmployeeById(ICRUDRepository<Employee> employeeRepository, ILogger<Program> logger, int id)
    {
        APIResponse response = new();
        try
        {
            logger.Log(LogLevel.Information, "GetEmployeeById called");
            response.Result = await employeeRepository.GetByIdAsync(id);

            if (response.Result == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "employee not found" };
                return Results.NotFound(response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting the employee by id");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while getting employee by id" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
    private static async Task<IResult> GetAllEmployees(ICRUDRepository<Employee> employeeRepository, ILogger<Program> logger)
    {
        APIResponse response = new();
        try
        {
            logger.Log(LogLevel.Information, "GetAllServices called");
            response.Result = await employeeRepository.GetAllAsync();

            if (response.Result == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "No employee found" };
                return Results.NotFound(response);
            }
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting the employee");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while getting the employees" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    private static async Task<IResult> CreateEmployee(ICRUDRepository<Employee> employeeRepository, ILogger<Program> logger, IMapper map, [FromBody] EmployeeDTO employeeDTO)
    {
        APIResponse response = new();

        try
        {
            var existingEmployee = await employeeRepository.GetByName(employeeDTO.FirstName);
            if (existingEmployee != null)
            {
                response.Message = "Employee already exists";
                return Results.BadRequest(response);
            }

            Employee employee = map.Map<Employee>(employeeDTO);

            await employeeRepository.CreateAsync(employee);
            await employeeRepository.SaveAsync();

            EmployeeDTO serviceDto = map.Map<EmployeeDTO>(employee);


            response.Result = serviceDto;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.Created;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the employees");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while creating employees" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    private static async Task<IResult> DeleteEmployee(ICRUDRepository<Employee> employeeRepository, ILogger<Program> logger, IMapper _map, int id)
    {
        APIResponse response = new();
        try
        {
            var employee = await employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "employee not found" };
                return Results.NotFound(response);
            }

            await employeeRepository.RemoveAsync(employee);
            await employeeRepository.SaveAsync();

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Message = "employee was deleted Successfully";
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting the employee");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while deleting the employee" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    private static async Task<IResult> UpdateEmployee(ICRUDRepository<Employee> employeeRepository, ILogger<Program> logger, IMapper map, [FromBody] Employee employeeDTO, int id)
    {
        APIResponse response = new();
        try
        {
            Employee? existingEmployee = await employeeRepository.GetByIdAsync(id);
            if (existingEmployee == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "Employee not found" };
                return Results.NotFound(response);
            }

            existingEmployee.FirstName = employeeDTO.FirstName;
            existingEmployee.LastName = employeeDTO.LastName;
            existingEmployee.Email = employeeDTO.Email;
            existingEmployee.PhoneNumber = employeeDTO.PhoneNumber;

            await employeeRepository.UpdateAsync(existingEmployee);
            await employeeRepository.SaveAsync();

            ServiceDTO serviceDto = map.Map<ServiceDTO>(existingEmployee);

            response.Result = serviceDto;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the employee");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while Updating the employee" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
}