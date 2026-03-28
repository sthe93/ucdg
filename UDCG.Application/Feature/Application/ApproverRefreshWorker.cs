using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UDCG.Application.Feature.ApplicationsAdmin.Interface;

namespace UDCG.Application.Feature.Application
{
    public class ApproverRefreshWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ApproverRefreshWorker> _logger;

        private string? _lastFundAdminStaffNo;
        private string? _lastSiaDirectorStaffNo;
        private DateTime _lastFullRefreshUtc = DateTime.MinValue;

        public ApproverRefreshWorker(IServiceScopeFactory scopeFactory, ILogger<ApproverRefreshWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Approver refresh worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IApproverRefreshService>();

                    var roles = await service.GetCurrentRoleOwnersAsync();

                    var fundAdminChanged = !string.Equals(_lastFundAdminStaffNo?.Trim(), roles.FundAdminStaffNo?.Trim(), StringComparison.OrdinalIgnoreCase);

                    var siaDirectorChanged = !string.Equals(_lastSiaDirectorStaffNo?.Trim(), roles.SiaDirectorStaffNo?.Trim(), StringComparison.OrdinalIgnoreCase);

                    var fiveMinutesElapsed = DateTime.UtcNow >= _lastFullRefreshUtc.AddMinutes(5);

                    if (fundAdminChanged || siaDirectorChanged || fiveMinutesElapsed)
                    {
                        var reason = fundAdminChanged || siaDirectorChanged ? "role owner changed" : "scheduled 5-minute refresh";

                        _logger.LogInformation("Running approver refresh because {Reason}", reason);

                        var result = await service.RefreshActiveApproverAssignmentsAsync(0);

                        _lastFundAdminStaffNo = roles.FundAdminStaffNo;
                        _lastSiaDirectorStaffNo = roles.SiaDirectorStaffNo;
                        _lastFullRefreshUtc = DateTime.UtcNow;

                        _logger.LogInformation("Approver refresh completed. Considered: {Considered}, Updated: {Updated}, StillStuck: {StillStuck}",
                            result.Considered,
                            result.Updated,
                            result.StillStuck);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error running approver refresh worker.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
