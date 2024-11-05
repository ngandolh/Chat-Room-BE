
using Chat_Room_Demo.DataService;
using Chat_Room_Demo.Hubs;
using Chat_Room_Demo.Services;
using Domain.Chat.Models;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Chat_Room_Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSignalR(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(10);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
            });

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
            builder.Services.AddDbContext<ChatRoomContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
                Console.WriteLine("DbContext configured successfully.");
            });

            builder.Services.AddSingleton<SharedDb>();
            builder.Services.AddScoped<IChatService,  ChatService>();
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
            app.MapHub<ChatOne>("/chat-one");

            app.Run();
        }
    }
}
