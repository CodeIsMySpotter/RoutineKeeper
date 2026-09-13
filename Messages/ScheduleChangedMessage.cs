using CommunityToolkit.Mvvm.Messaging.Messages;

namespace RoutineKeeper.Messages;

public class ScheduleChangedMessage : ValueChangedMessage<bool>
{
    public ScheduleChangedMessage(bool value = true) : base(value) {}
}
