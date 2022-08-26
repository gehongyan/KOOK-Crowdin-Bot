using Microsoft.Extensions.Hosting;
using Serilog;

namespace Kook.Bot.Crowdin.ScheduledServices;

public abstract class ScheduledServiceBase : IHostedService, IDisposable
{
    private readonly ILogger _logger;
    private TimeSpan _dueTime;
    private TimeSpan _period;
    private readonly string _serviceName;
    private readonly bool _allowConcurrentCalls;
    private readonly Timer _timer;
    private readonly SemaphoreSlim _semaphoreSlim;

    protected ScheduledServiceBase(TimeSpan dueTime, TimeSpan period, ILogger logger, string serviceName, bool allowConcurrentCalls)
    {
        _logger = logger;
        _serviceName = serviceName;
        _allowConcurrentCalls = allowConcurrentCalls;
        _dueTime = dueTime;
        _period = period;
        _timer = new Timer(Execute!, null, Timeout.Infinite, 0);
        _semaphoreSlim = allowConcurrentCalls
            ? null
            : new SemaphoreSlim(1);
    }

    public virtual void Dispose()
    {
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("定时服务 {ServiceName} 已启动", _serviceName);
        _timer.Change(_dueTime, _period);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Fatal("定时服务 {ServiceName} 已终止", _serviceName);
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Execute(object state = null)
    {
        if (!_allowConcurrentCalls && _semaphoreSlim.CurrentCount == 0)
        {
            _logger.Verbose("定时服务 {ServiceName} 正在运行中", _serviceName);
            return;
        }

        try
        {
            if (!_allowConcurrentCalls)
                _semaphoreSlim.Wait();
            _logger.Verbose("定时服务 {ServiceName} 已触发", _serviceName);
            ExecuteAsync().GetAwaiter().GetResult();
            _logger.Verbose("定时服务 {ServiceName} 执行已完成", _serviceName);
        }
        catch (Exception e)
        {
            _logger.Error(e, "定时服务 {ServiceName} 执行过程中发生异常", _serviceName);
        }
        finally
        {
            if (!_allowConcurrentCalls)
                _semaphoreSlim.Release();
        }
    }

    protected abstract Task ExecuteAsync();

    public void ExecuteImmediately() => ChangeTimer(TimeSpan.Zero);

    protected void ChangeTimer(TimeSpan? dueTime = null, TimeSpan? period = null)
    {
        _dueTime = dueTime ?? _dueTime;
        _period = period ?? _period;

        _timer.Change(_dueTime, _period);
    }
}