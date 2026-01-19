using System.Threading;
using System.Threading.Tasks;

namespace WindowsCleanerUtility.Services
{
    public interface ISchedulerService
    {
        void ScheduleCleanup(CleaningOptions options, int hoursInterval);
        void CancelSchedule();
        bool IsScheduled { get; }
    }
}