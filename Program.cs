
using Chat_Room_Demo.DataService;
using Chat_Room_Demo.Hubs;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Chat_Room_Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSignalR();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "MyDefaultPolicy",
                   policy =>
                   {
                       policy.WithOrigins("http://localhost:3000")
                             .WithOrigins("http://localhost:5173")
                             .WithOrigins("http://localhost:8081")
                             .WithOrigins("https://rhcqs.vercel.app")
                             .AllowAnyHeader()
                             .AllowAnyMethod()
                             .AllowCredentials();
                   });
            });

            builder.Services.AddSingleton<SharedDb>();
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors("MyDefaultPolicy");


            app.MapControllers();

            app.MapHub<ChatHub>("/Chat");

            app.Run();
        }
    }
}
