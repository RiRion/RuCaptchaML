using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.ML;
using RuCaptchaML.Shared.DependencyInjection;
using RuCaptchaML.Train;

using var container = GetContainer(GetConfiguration());

var context = container.Resolve<ApplicationContext>();

try
{
    await context.RunAsync();
}
catch (Exception e)
{
    Console.WriteLine(e);
}

IContainer GetContainer(IConfigurationRoot configuration)
{
    var builder = new ContainerBuilder();

    builder.RegisterClientConfigurations(configuration);
    builder.RegisterClientConfigurations(configuration, Collector.GetAssembly("RuCaptchaML.Shared"));

    builder.RegisterType<ApplicationContext>();

    builder.RegisterInstance(new MLContext(seed: 1)).AsSelf();

    builder
        .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
        .Where(p => p.Name.EndsWith("Service") || p.IsClass)
        .AsImplementedInterfaces();

    return builder.Build();
}

IConfigurationRoot GetConfiguration()
{
    return new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();
}
