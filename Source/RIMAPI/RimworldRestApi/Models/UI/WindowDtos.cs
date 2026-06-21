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

    // Close one or more open windows by type name.
    // WindowTypes is matched case-insensitively against each open window's
    // runtime type name (e.g. "Dialog_NamePlayerSettlement", "EditWindow_Log");
    // a substring is enough so "Log" closes EditWindow_Log. When omitted/empty,
    // ForcePauseOnly (default true) closes every window that force-pauses the
    // game — the unattended-benchmark nuisance case.
    public class WindowCloseRequestDto
    {
        public List<string> WindowTypes { get; set; }
        public bool ForcePauseOnly { get; set; } = true;
    }

    public class WindowCloseResultDto
    {
        public int ClosedCount { get; set; }
        public List<string> ClosedWindows { get; set; } = new List<string>();
    }

    public class OpenWindowDto
    {
        public string WindowType { get; set; }
        public bool ForcePause { get; set; }
    }
}