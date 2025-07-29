using FastEndpoints;

namespace Focuswave.FocusSessionService.Application.FocusCycles;

public class FocusCycleGroup : Group
{
    public FocusCycleGroup()
    {
        Configure(
            "focus-cycle",
            ep =>
            {
                ep.Description(d => d.WithTags("focus-cycle"));
            }
        );
    }
}
