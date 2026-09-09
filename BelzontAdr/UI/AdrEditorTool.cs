using Game.UI.Editor;
using Unity.Entities;

namespace BelzontAdr.UI
{
    public class AdrEditorTool : EditorTool
    {
        public const string TOOL_ID = "k45__adr_MainWindow";

        public AdrEditorTool(World world) : base(world)
        {
            id = TOOL_ID;
            icon = "coui://adr.k45/UI/images/ADR.svg";
        }
    }
}
