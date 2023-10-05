using ImGuiNET;
using System.Diagnostics;
using System.Numerics;

namespace StopTheClip
{
    public static class PluginUI
    {
        public static bool isVisible = false;

        public static void Draw(ref bool isEnabled)
        {
            if (!isVisible)
                return;

            ImGui.SetNextWindowSize(new Vector2(190, 120), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(190, 120), new Vector2(9999));

            
            if (ImGui.Begin("StopTheClipConfiguration", ref isVisible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.BeginChild("Outer", new Vector2(180, 110), true);

                ShowKofi();

                ImGui.BeginChild("StopTheClip", new Vector2(160, 40), true);

                ImGui.Checkbox("Enabled", ref isEnabled);

                ImGui.EndChild();

                ImGui.EndChild();
            }

        }

        public static void ShowKofi()
        {
            ImGui.BeginChild("Support", new Vector2(160, 50), true);

            ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
            if (ImGui.Button("Support via Ko-fi"))
            {
                Process.Start(new ProcessStartInfo { FileName = "https://ko-fi.com/projectmimer", UseShellExecute = true });
            }
            ImGui.PopStyleColor(3);
            ImGui.EndChild();
        }
    }
}
