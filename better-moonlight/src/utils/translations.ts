import { useLocalization } from "cs2/l10n";


export const useSettingOptionTranslate = ({
    prefix = "BetterMoonLight.BetterMoonLight.Mod"
} = {}) => {
    const { translate } = useLocalization();

    return {
        transOptionName: (key: string) => {
            return translate(`Options.OPTION[${prefix}.Setting.${key}]`) ?? key;
        },
        transOptionDesc: (key: string) => {
            return translate(`Options.OPTION_DESCRIPTION[${prefix}.Setting.${key}]`) ?? key;
        },
        transOptionGroup: (key: string) => {
            return translate(`Options.GROUP[${prefix}.${key}]`) ?? key;
        },
        optionSection: translate(`Options.SECTION[${prefix}]`) ?? prefix
    };
};
