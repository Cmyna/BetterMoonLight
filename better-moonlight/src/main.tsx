import { useLocalization } from "cs2/l10n";
import { getModule } from "cs2/modding";
import { 
    Button, Panel, Dropdown, 
    DropdownToggle, DropdownItem, 
    Scrollable, 
} from "cs2/ui"
import styles from 'css/panel.module.scss'
import { CSSProperties, ReactNode, useEffect, useState } from "react";
import { useBinding, useTrigger } from "utils/bindings"
import { useSettingOptionTranslate } from "utils/translations";
import { useMouseDrag as useDrag } from "utils/useDragging";


const COMPONENTS_PATH = "game-ui/game/components"
const INFOVIEW_PANEL_SECTION_PATH = `${COMPONENTS_PATH}/infoviews/active-infoview-panel/components/sections/infoview-panel-section.tsx`

const FOCUS_DISABLED = getModule("game-ui/common/focus/focus-key.ts", "FOCUS_DISABLED");


export const Main = () => {

    const {
        transOptionName, transOptionGroup, optionSection
    } = useSettingOptionTranslate();
    const { translate } = useLocalization();
    const translateTexSelectionName = (key: string) => {
        return translate(`BetterMoonLight.Texture[${key}]`) ?? key;
    }
    const transAuroraOverrideLv = (lv: number) => {
        return translate(`OPTIONS.BetterMoonLight.OverwriteAuroraLevel[${lv}]`) ?? lv
    }

    // control panel position
    const {
        offset, reset: resetOffset,
        eventListeners: { mouse: dragListeners }
    } = useDrag();
    
    const [showSetting, setShowSetting] = useBinding<boolean>("ShowSetting");
    const [overrideTexture, setOverrideTexture] = useBinding<boolean>("OverrideTexture");
    const [selectedTexture, setSelectedTexture] = useBinding<string>("SelectedTexture");
    const [availableTextures] = useBinding<{selections: string[]}>("AvailableTextures");
    const [overrideNightLighting, setOverrideNightNightling] = useBinding<boolean>("OverrideNightLighting");
    const [auroraOverrideLv, setAuroraOverrideLv] = useBinding<number>("AuroraOverwriteLevel");
    const resetSettings = useTrigger("Reset");

    // if hide and show setting, reset dragging offset
    useEffect(() => {
        resetOffset();
    }, [showSetting]);

    const Checkbox = getModule("game-ui/common/input/toggle/checkbox/checkbox.tsx", "Checkbox");

    const defaultStyle = getModule("game-ui/common/panel/themes/default.module.scss", "classes");
    const { closeButton } = getModule("game-ui/common/panel/panel.module.scss", "classes");
    const { button: btnRoundHighLight } = getModule("game-ui/common/input/button/themes/round-highlight-button.module.scss", "classes");
    const dropdownStyle = getModule("game-ui/menu/themes/dropdown.module.scss", "classes");
    const { button: buttonAnim } = getModule("game-ui/menu/widgets/button/button.module.scss", "classes")
    const { button: secondaryBtnStyle } = getModule("game-ui/menu/themes/secondary-button.module.scss", "classes")


    const Header = ({title}: {title: string}) => {
        return (<span style={{color: "white", marginBottom: "5rem"}}>{title}</span>)
    }


    const selections = (<>
        <Scrollable style={{maxHeight: "200rem"}}>
            {availableTextures.selections.map(selection => {
                return (
                    <DropdownItem 
                        focusKey={FOCUS_DISABLED}
                        onChange={(v) => setSelectedTexture(v)}
                        key={selection} 
                        value={selection}
                    >
                        {translateTexSelectionName(selection)}
                    </DropdownItem>
                )
            })}
        </Scrollable>
    </>)

    return (<>
        {<Panel 
            style={{ 
                left: `${offset.x + 10}rem`, 
                top: `${offset.y + 60}rem`,
                // just make it invisble but not destroy whole components tree
                // so some context (eg. scroll position) will be kept
                visibility: showSetting ? "visible" : "hidden" 
            }}
            className={styles.main}
        >
            <div className={`${defaultStyle.header} ${styles.header}`} {...dragListeners}>
                <div style={{ minWidth: "10rem", minHeight: "5rem" }}></div>
                <div>{optionSection}</div>
                <Button 
                    style={{color: "white"}}
                    className={`${closeButton} ${btnRoundHighLight}`} 
                    src="Media/Glyphs/Close.svg"
                    onClick={() => setShowSetting(false)}
                    tinted={true}
                />
            </div>
            <Scrollable style={{maxHeight: "500rem"}}>
                {/* Basic Section */}
                <InfoViewSectionMod>
                    <Header title={transOptionGroup("Basic")}/>
                    <div className={styles.row}>
                        <span>{transOptionName("OverwriteNightLighting")}</span>
                        <Checkbox 
                            focusKey={FOCUS_DISABLED}
                            checked={overrideNightLighting} 
                            onChange={(v: boolean) => setOverrideNightNightling(v)} 
                        />
                    </div>
                    <div className={styles.row}>
                        <div/>
                        <button 
                            className={`${buttonAnim} ${secondaryBtnStyle}`} 
                            onClick={() => resetSettings(true)}
                        >
                            {transOptionName("ResetModSettings")}
                        </button>
                    </div>
                </InfoViewSectionMod>

                {/* Texture Section */}
                <InfoViewSectionMod>
                    <Header title={transOptionGroup("Texture")}/>
                    <div className={styles.row}>
                        <span>{transOptionName("OverrideTexture")}</span>
                        <Checkbox 
                            focusKey={FOCUS_DISABLED}
                            checked={overrideTexture} 
                            onChange={(v: boolean) => setOverrideTexture(v)} 
                        />
                    </div>
                    <div style={{ display: "flex", flexDirection: "column", marginBottom: "10rem" }}>
                        <span style={{marginBottom: "10rem"}}>{transOptionName("SelectedTexture")}</span>
                        <Dropdown 
                            focusKey={FOCUS_DISABLED}
                            theme={dropdownStyle}
                            content={selections}
                        >
                            <DropdownToggle disabled={false}>
                                <div>{translateTexSelectionName(selectedTexture)}</div>
                            </DropdownToggle>
                        </Dropdown>
                    </div>
                </InfoViewSectionMod>

                {/* Night Lighting Section */}
                <InfoViewSectionMod>
                    <Header title={transOptionGroup("Night")}/>
                    <SliderRow bindingName="AmbientLight" min={0} max={15} />
                    <SliderRow bindingName="NightSkyLight" min={0} max={15} />
                    <SliderRow bindingName="MoonDirectionalLight" min={0} max={15} />
                    <SliderRow bindingName="MoonDiskSize" min={0} max={20} />
                    <SliderRow bindingName="MoonDiskIntensity" min={0} max={10} />
                    <SliderRow bindingName="NightLightTemperature" min={3500} max={10000} step={1} />
                    <SliderRow bindingName="MoonTemperature" min={3500} max={10000} step={1} />
                    <SliderRow bindingName="StarfieldEmmisionStrength" min={0} max={1} />
                </InfoViewSectionMod>

                {/* Aurora Section */}
                <InfoViewSectionMod>
                    <Header title={transOptionGroup("Aurora")}/>
                    <SliderRow bindingName="AuroraIntensity" min={0} max={10} step={0.05} />
                    <div style={{ display: "flex", flexDirection: "column", marginBottom: "10rem" }}>
                        <span style={{marginBottom: "10rem"}}>{transOptionName("AuroraOverwriteLevel")}</span>
                        <Dropdown 
                            focusKey={FOCUS_DISABLED}
                            theme={dropdownStyle}
                            content={[0, 1, 2].map(lv => {
                                return (
                                    <DropdownItem
                                        focusKey={FOCUS_DISABLED}
                                        key={lv}
                                        value={lv}
                                        onChange={(v) => setAuroraOverrideLv(v)}
                                    >
                                        {transAuroraOverrideLv(lv)}
                                    </DropdownItem>
                                )
                            })}
                        >
                            <DropdownToggle disabled={false}>
                                <div>{transAuroraOverrideLv(auroraOverrideLv)}</div>
                            </DropdownToggle>
                        </Dropdown>
                    </div>
                </InfoViewSectionMod>
            </Scrollable>
        </Panel>}
    </>)
}



const InfoViewSectionMod = ({children}: {children: ReactNode[]}) => {

    const InfoviewPanelSection = getModule(INFOVIEW_PANEL_SECTION_PATH, "InfoviewPanelSection");

    return (
        <InfoviewPanelSection  disableFocus={true}>
            <div style={{color: "#a7a7a7ff"}}>{children}</div>
        </InfoviewPanelSection>
    )
}



type SliderRowProps = {
    bindingName: string;
    min: number;
    max: number;
    step?: number;
    title?: string;
    valueDisplay?: (v: number) => string;
}


const SliderRow = ({
    title,
    bindingName,
    min, max,
    step = 1,
    valueDisplay = (v) => v.toFixed(2)
}: SliderRowProps) => {

    const {transOptionName} = useSettingOptionTranslate({});
    const [value, setValue] = useBinding<number>(bindingName);

    const Slider = getModule("game-ui/common/input/slider/slider.tsx", "Slider");

    const colFlexStyle: CSSProperties = {
        display: "flex", 
        flexDirection: "column"
    }
    const row2ItemFlex: CSSProperties = {
        display: "flex", 
        flexDirection: "row", 
        alignItems: "center", 
        justifyContent: "space-between"
    }

    if (!title) {
        title = transOptionName(bindingName) ?? bindingName;
    }

    return (
        <div style={{...colFlexStyle}}>
            <div style={{...row2ItemFlex, marginBottom: "5rem"}}>
                <span style={{ fontSize: "15rem" }}>{title}</span>
                <p style={{ fontSize: "13rem" }} >{valueDisplay(value)}</p>
            </div>
            <Slider 
                style={{ width: "280rem", marginBottom: "15rem" }}
                focusKey={FOCUS_DISABLED}
                value={value} 
                start={min} 
                end={max} 
                gamepadStep={step}
                onChange={(v: number) => setValue(v)}
            />
        </div>
    )
}
