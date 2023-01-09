using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyModel;

namespace RuCaptchaML.Shared.DependencyInjection
{
    public static class Collector
    {
        public static Assembly GetAssembly(string name)
        {
            return Assembly.Load(DependencyContext.Default.CompileLibraries
                .First(c => c.Name.Equals(name)).Name);
        }

        public static void RegisterClientConfigurations(this ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterClientConfigurations(configuration, Assembly.GetCallingAssembly());
        }

        public static void RegisterClientConfigurations(this ContainerBuilder builder, IConfiguration configuration, Assembly assemblyToScan)
        {
            var configTypes = assemblyToScan.GetTypes()
                .Where(t => t.Name.EndsWith("Config") && t.IsClass);

            foreach (var configType in configTypes)
            {
                builder.RegisterInstance(configuration.GetSection(configType.Name)
                    .Get(configType)).AsSelf().SingleInstance();
            }
        }
    }
}
