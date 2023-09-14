# Alesthetic API

This is the backend API for the Alesthetic web application.

## Getting Started

To get started with the API, follow these steps:

1. Clone the repository to your local machine.
2. Open the solution file in Visual Studio.
3. Build the solution to restore NuGet packages.
4. Run the project.

## Dependencies

The API uses the following dependencies:

- ASP.NET Core 5.0
- Entity Framework Core 5.0
- SQLite

## Authentication

The API uses JWT authentication. To authenticate, send a POST request to the `/api/authenticate` endpoint with a JSON body containing your username and password. The API will respond with a JWT token that you can use to authenticate subsequent requests.

## API Documentation

The API documentation is available at the `/swagger` endpoint.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.# AppointmentBooking-API
