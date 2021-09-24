using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace OptionsPatterns
{
	public class Program
	{
		public static Task Main(string[] args)
		{
			return CreateHostBuilder(args).Build().RunAsync();
		}

		private static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureServices((builderContext, services) =>
				{
					var configuration = builderContext.Configuration;

					services.Configure<WorkerOptions>(configuration.GetSection(nameof(WorkerOptions)));

					services.AddHostedService<Worker>();
					services.AddHostedService<DynamicWorker>();
				});
		}
	}

	public class DynamicWorker : BackgroundService
	{
		private readonly IOptionsMonitor<WorkerOptions> _options;

		public DynamicWorker(IOptionsMonitor<WorkerOptions> options)
		{
			_options = options;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				Console.WriteLine($"Dynamic worker running at: {DateTimeOffset.Now}");
				await Task.Delay(_options.CurrentValue.Delay, stoppingToken);
			}
		}
	}

	public class Worker : BackgroundService
	{
		private readonly int _delay;

		public Worker(IOptions<WorkerOptions> options)
		{
			_delay = options.Value.Delay;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				Console.WriteLine($"Worker running at: {DateTimeOffset.Now}");
				await Task.Delay(_delay, stoppingToken);
			}
		}
	}

	public class WorkerOptions
	{
		public int Delay { get; set; }
	}
}
