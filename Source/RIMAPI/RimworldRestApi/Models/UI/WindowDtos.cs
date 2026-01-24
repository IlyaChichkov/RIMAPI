using System.Collections.Generic;

namespace RIMAPI.Models
{
    // Simple "OK" Message
    public class WindowMessageRequestDto
    {
        public string Title { get; set; } = "Alert";
        public string Text { get; set; }
        public string ButtonText { get; set; } = "OK";
    }

    // Dialog with Choices
    public class WindowDialogRequestDto
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public List<DialogOptionDto> Options { get; set; }
    }

    public class DialogOptionDto
    {
        public string Label { get; set; }

        // Optional: Perform an action when clicked?
        // For a basic implementation, we might just log the click or close the window.
        // Advanced: You could map this to a callback ID if needed.
        public string ActionId { get; set; }
        public bool ResolveTree { get; set; } = true; // Close window after click
    }
}