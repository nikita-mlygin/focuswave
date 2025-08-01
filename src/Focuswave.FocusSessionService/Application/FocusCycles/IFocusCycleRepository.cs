using Focuswave.FocusSessionService.Domain.FocusCycles;

namespace Focuswave.FocusSessionService.Application.FocusCycles;

public interface IFocusCycleRepository
{
    public Task SaveAsync(FocusCycleAggregate focusCycle);

    public Task<Option<FocusCycleAggregate>> GetByIdAsync(Guid id);

    public Task<Option<FocusCycleAggregate>> GetByUserIdAsync(Guid userId);

    public Task Remove(FocusCycleAggregate focusCycle);
}
