using System.Windows;
using System.Windows.Controls;

namespace RealsonnetApp
{
    public class ChatTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? UserTemplate { get; set; }
        public DataTemplate? BotTemplate { get; set; }
        public DataTemplate? TypingTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ChatMessage message)
            {
                if (message.IsTyping && TypingTemplate != null)
                    return TypingTemplate;
                if (message.IsUser && UserTemplate != null)
                    return UserTemplate;
                if (!message.IsUser && BotTemplate != null)
                    return BotTemplate;
            }

            return base.SelectTemplate(item, container) ?? new DataTemplate();
        }
    }
}