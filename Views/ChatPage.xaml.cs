using Microsoft.Maui.Controls;
using RoutineKeeper.Models;
using RoutineKeeper.ViewModels;

namespace RoutineKeeper.Views;

public class ChatMessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate UserTemplate { get; set; }
    public DataTemplate AssistantTemplate { get; set; }
    public DataTemplate SystemTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is ChatMessage message)
        {
            if (message.Role == ChatRole.User) return UserTemplate;
            if (message.Role == ChatRole.System) return SystemTemplate;
            return AssistantTemplate;
        }
        return AssistantTemplate;
    }
}

public partial class ChatPage : ContentPage
{
    public ChatPage(ChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
