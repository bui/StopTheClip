﻿﻿﻿﻿﻿﻿﻿﻿﻿using ImGuiNET;
using System.Diagnostics;
using System.Numerics;

namespace StopTheClip
{
    public static class PluginUI
    {
        public static bool isVisible = false;

        public static void Draw(ref bool isEnabled, Configuration configuration)
        {
            if (!isVisible)
                return;

            ImGui.SetNextWindowSize(new Vector2(190, 120), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(190, 120), new Vector2(9999));

            
            if (ImGui.Begin("StopTheClipConfiguration", ref isVisible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ShowKofi();

                ImGui.Checkbox("Enabled", ref isEnabled);

                float nearClip = configuration.NearClipValue;
                if (ImGui.SliderFloat("Near Clip", ref nearClip, 0.001f, 0.1f, "%.3f"))
                {
                    configuration.NearClipValue = nearClip;
                    configuration.Save();
                }
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
