import { bindValue, trigger, useValue } from "cs2/api";
import { AdrEditorSettingsPopup } from "settings/AdrEditorSettingsPopup";

export const ADR_EDITOR_TOOL_ID = "k45__adr_MainWindow";

export const AdrEditorToolbar = (input: any) => {
    const activeTool$ = bindValue<string | null>("editorTool", "activeTool", null);

    return (args: any) => {
        const activeTool = useValue(activeTool$);

        return (
            <>
                {input(args)}
                {activeTool === ADR_EDITOR_TOOL_ID && (
                    <AdrEditorSettingsPopup
                        isOpen={true}
                        onClose={() => trigger("editorTool", "selectTool", null)}
                    />
                )}
            </>
        );
    };
};
