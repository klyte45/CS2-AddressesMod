import { FormLine, LocElementType, VanillaWidgets } from "@klyte45/vuio-commons";
import { DropdownItem } from "cs2/bindings";

type NamedOption = { Name: string; IdString: string | null };

type Props<T extends NamedOption> = {
    title: string;
    options: T[];
    value: T | undefined;
    onChange: (option: T) => void;
};

export const NamesetSelectLine = <T extends NamedOption>({ title, options, value, onChange }: Props<T>) => {
    const StringDD = VanillaWidgets.instance.DropdownField<string>();
    const items: DropdownItem<string>[] = options.map((o) => ({
        value: o.IdString ?? "",
        displayName: { __Type: LocElementType.String, value: o.Name },
    }));

    return (
        <FormLine title={title}>
            <StringDD
                items={items}
                value={value?.IdString ?? ""}
                onChange={(id) => {
                    const next = options.find((o) => (o.IdString ?? "") === id);
                    if (next) onChange(next);
                }}
            />
        </FormLine>
    );
};
