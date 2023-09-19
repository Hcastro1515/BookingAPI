using System.Net;
using AlestheticApi.Models;
using AlestheticApi.Models.DTOs;
using AlestheticApi.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AlestheticApi.Endpoints;

public static class CustomerEndpoint
{
    public static void ConfigureCustomerEndpoint(this WebApplication app)
    {
        app.MapGet("/api/customer", GetAllCustomers)
        .WithName("GetAllCustomers")
        .Produces<APIResponse>(200);

        app.MapGet("/api/customer/{id:int}", GetCustomerById)
        .WithName("GetCustomerById")
        .Produces<APIResponse>(200)
        .Produces(500);

        app.MapPost("/api/customer", CreateCustomer)
        .WithName("CreateCustomer")
        .Accepts<CustomerDTO>("application/json")
        .Produces<APIResponse>(201)
        .Produces(400);

        app.MapDelete("/api/customer/{id:int}", DeleteCustomer)
        .WithName("DeleteCustomer")
        .Produces<APIResponse>(200)
        .Produces(500);

        app.MapPut("/api/customer/{id:int}", UpdateCustomer)
        .WithName("UpdateCustomer")
        .Accepts<Customer>("application/json")
        .Produces<APIResponse>(200)
        .Produces(500);
    }

    private static async Task<IResult> GetCustomerById(ICRUDRepository<Customer> customerRepository, ILogger<Program> logger, int id)
    {
        APIResponse response = new();
        try
        {
            logger.Log(LogLevel.Information, "GetCustomerById called");
            response.Result = await customerRepository.GetByIdAsync(id);

            if (response.Result == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "Customer not found" };
                return Results.NotFound(response);
            }

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting customer by id");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while getting customer by id" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
    private static async Task<IResult> GetAllCustomers(ICRUDRepository<Customer> customerRepository, ILogger<Program> logger)
    {
        APIResponse response = new();
        try
        {
            logger.Log(LogLevel.Information, "GetAllCustomers called");
            response.Result = await customerRepository.GetAllAsync();

            if (response.Result == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "No customers found" };
                return Results.NotFound(response);
            }
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while getting customers");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while getting customers" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    private static async Task<IResult> CreateCustomer(ICRUDRepository<Customer> customerRepository,
                                                            ILogger<Program> logger, IMapper map,
                                                                [FromBody] CustomerDTO customerDTO)
    {
        APIResponse response = new();

        try
        {
            var existingCustomer = await customerRepository.GetByEmail(customerDTO.Email);
            if (existingCustomer != null)
            {
                response.Message = "Customer already exists";
                return Results.BadRequest(response);
            }

            Customer customer = map.Map<Customer>(customerDTO);

            await customerRepository.CreateAsync(customer);
            await customerRepository.SaveAsync();

            CustomerDTO customerDto = map.Map<CustomerDTO>(customer);


            response.Result = customerDto;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.Created;
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the customer");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while creating customer" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    private static async Task<IResult> DeleteCustomer(ICRUDRepository<Customer> customerRepository, ILogger<Program> logger, IMapper map, int id)
    {
        APIResponse response = new();
        try
        {
            var customer = await customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "Customer not found" };
                return Results.NotFound(response);
            }

            await customerRepository.RemoveAsync(customer);
            await customerRepository.SaveAsync();

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Message = "Customer was deleted Successfully";
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting customer");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while deleting customer" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    private static async Task<IResult> UpdateCustomer(ICRUDRepository<Customer> customerRepository, ILogger<Program> logger, IMapper map, [FromBody] CustomerDTO newCustomer, int id)
    {
        APIResponse response = new();
        try
        {
            Customer? existingCustomer = await customerRepository.GetByIdAsync(id);
            if (existingCustomer == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.ErrorMessages = new() { "Customer not found" };
                return Results.NotFound(response);
            }

            existingCustomer.FirstName = newCustomer.FirstName;
            existingCustomer.LastName = newCustomer.LastName;
            existingCustomer.Email = newCustomer.Email;
            existingCustomer.PhoneNumber = newCustomer.PhoneNumber;
            existingCustomer.Address = newCustomer.Address;
            existingCustomer.DateOfBirth = newCustomer.DateOfBirth;
            await customerRepository.UpdateAsync(existingCustomer);
            await customerRepository.SaveAsync();

            CustomerDTO customerDto = map.Map<CustomerDTO>(existingCustomer);

            response.Result = customerDto;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating customer");
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.InternalServerError;
            response.ErrorMessages = new() { "An error occurred while Updating customer" };
            return Results.StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }
}