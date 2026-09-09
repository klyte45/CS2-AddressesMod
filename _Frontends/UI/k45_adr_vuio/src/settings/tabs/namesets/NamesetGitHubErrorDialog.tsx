import { GitHubAddressesFilesService, replaceArgs } from "adr";
import { VanillaComponentResolver } from "@klyte45/vuio-commons";
import { Portal } from "cs2/ui";
import { FocusDisabled } from "cs2/input";
import { translate } from "../../translate";
import { GitHubPickerError } from "./namesetGitHubDataProvider";

type Props = {
    error: GitHubPickerError | null;
    onClose: () => void;
};

export const NamesetGitHubErrorDialog = ({ error, onClose }: Props) => {
    if (!error) return null;
    const Dialog = VanillaComponentResolver.instance.Dialog;
    const message = error.resetTime
        ? replaceArgs(translate("githubListing.errorLoadingRateLimit"), { formattedTime: error.resetTime.toString() })
        : translate("githubListing.errorLoadingMsg");

    return (
        <Portal>
            <Dialog
                onClose={onClose}
                title={translate("githubLibrary.title")}
                buttons={
                    <FocusDisabled>
                        <div className="k45_btnHost k45_dialogBtns">
                            <button type="button" className="positiveBtn" onClick={() => GitHubAddressesFilesService.goToRepository()}>{translate("githubLibrary.visitRepository")}</button>
                            <button type="button" className="negativeBtn" onClick={onClose}>{engine.translate("Common.CANCEL")}</button>
                        </div>
                    </FocusDisabled>
                }
            >
                <p>{message}</p>
            </Dialog>
        </Portal>
    );
};
