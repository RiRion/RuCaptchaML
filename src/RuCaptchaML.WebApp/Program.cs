using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using RuCaptchaML.Shared.DependencyInjection;
using RuCaptchaML.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.WebHost.UseContentRoot(Directory.GetCurrentDirectory());

builder.Host.ConfigureContainer<ContainerBuilder>(b => ConfigureContainer(b, builder.Configuration));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddPredictionEnginePool<InMemoryImageData, ImagePrediction>()
    .FromFile(GetMLModelPath(builder.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void ConfigureContainer(ContainerBuilder builder, IConfiguration configuration)
{
    builder.RegisterClientConfigurations(configuration, Collector.GetAssembly("RuCaptchaML.Shared"));

    builder
        .RegisterAssemblyTypes(Collector.GetAssembly("RuCaptcha.Predict"))
        .Where(p => p.Name.EndsWith("Service") || p.IsClass)
        .AsImplementedInterfaces();
}

string GetMLModelPath(IConfiguration configuration)
{
    return Path.Combine(configuration["NeuralNetworkConfig:MlNetModelDirectory"], configuration["NeuralNetworkConfig:MLModelFileName"]);
}
