// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddControllers();
}

var app = builder.Build();
{
    app.UseHttpsRedirection();
    app.MapControllers();
    app.Run();
}
Console.WriteLine("Hello, World!");