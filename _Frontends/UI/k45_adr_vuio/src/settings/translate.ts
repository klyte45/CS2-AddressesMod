export const translate = (key: string) => {
    const fullKey = `K45::ADR.main[${key}]`;
    const tr = engine.translate(fullKey);
    if (tr === fullKey) {
        (window as any).K45_MISSING_I18N ??= new Set<string>();
        ((window as any).K45_MISSING_I18N as Set<string>).add(fullKey);
    }
    return tr;
};
