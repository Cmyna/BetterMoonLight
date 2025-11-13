import { getModule } from "cs2/modding";
import { 
    Button, Panel, Dropdown, 
    DropdownToggle, DropdownItem, 
    Scrollable, 
} from "cs2/ui"
import styles from 'css/panel.module.scss'
import { CSSProperties, ReactNode, useState } from "react";
import { useBinding, useObjectBinding, useTrigger } from "utils/bindings"


const COMPONENTS_PATH = "game-ui/game/components"
const INFOVIEW_PANEL_SECTION_PATH = `${COMPONENTS_PATH}/infoviews/active-infoview-panel/components/sections/infoview-panel-section.tsx`

const FOCUS_DISABLED = getModule("game-ui/common/focus/focus-key.ts", "FOCUS_DISABLED");


export const Main = () => {

    // control panel position
    const [pos, setPos] = useState({x: 10, y: 60})
    
    const [showSetting, setShowSetting] = useBinding<boolean>("ShowSetting");
    const [overrideTexture, setOverrideTexture] = useBinding<boolean>("OverrideTexture");
    const [selectedTexture, setSelectedTexture] = useBinding<string>("SelectedTexture");
    const [availableTextures] = useBinding<{selections: string[]}>("AvailableTextures");
    const [overrideNightLighting, setOverrideNightNightling] = useBinding<boolean>("OverrideNightLighting");
    const reset = useTrigger("Reset");

    const defaultStyle = getModule("game-ui/common/panel/themes/default.module.scss", "classes");
    const { closeButton } = getModule("game-ui/common/panel/panel.module.scss", "classes");
    const { button: btnRoundHighLight } = getModule("game-ui/common/input/button/themes/round-highlight-button.module.scss", "classes");
    const DropdownStyle = getModule("game-ui/menu/themes/dropdown.module.scss", "classes");
    const Checkbox = getModule("game-ui/common/input/toggle/checkbox/checkbox.tsx", "Checkbox");
    const { button: buttonAnim } = getModule("game-ui/menu/widgets/button/button.module.scss", "classes")
    const { button: secondaryBtnStyle } = getModule("game-ui/menu/themes/secondary-button.module.scss", "classes")


    const Header = ({title}: {title: string}) => {
        return (<span style={{color: "white", marginBottom: "5rem"}}>{title}</span>)
    }


    const selections = availableTextures.selections.map(selection => {
        return (
            <DropdownItem 
                onChange={(v) => setSelectedTexture(v)}
                key={selection} 
                value={selection}
            >
                {selection}
            </DropdownItem>
        )
    });

    return (<>
        {showSetting && <Panel 
            style={{ left: `${pos.x}rem`, top: `${pos.y}rem` }}
            className={styles.main}
        >
            <div className={`${defaultStyle.header} ${styles.header}`}>
                <div style={{ minWidth: "10rem", minHeight: "5rem" }}></div>
                <div>Better MoonLight</div>
                <Button 
                    style={{color: "white"}}
                    className={`${closeButton} ${btnRoundHighLight}`} 
                    src="Media/Glyphs/Close.svg"
                    onClick={() => setShowSetting(false)}
                    tinted={true}
                />
            </div>
            <Scrollable>
                {/* Basic Section */}
                <InfoViewSectionMod>
                    <Header title="Basic"/>
                    <div className={styles.row}>
                        <span>Override Night Lighting</span>
                        <Checkbox 
                            focusKey={FOCUS_DISABLED}
                            checked={overrideNightLighting} 
                            onChange={(v: boolean) => setOverrideNightNightling(v)} 
                        />
                    </div>
                    <div className={styles.row}>
                        <div></div>
                        <button 
                            className={`${buttonAnim} ${secondaryBtnStyle}`} 
                            onClick={() => reset(true)}
                        >
                            Reset Mod Settings
                        </button>
                    </div>
                </InfoViewSectionMod>

                {/* Texture Section */}
                <InfoViewSectionMod>
                    <Header title="Texture"/>
                    <div className={styles.row}>
                        <span>Override Texture</span>
                        <Checkbox 
                            focusKey={FOCUS_DISABLED}
                            checked={overrideTexture} 
                            onChange={(v: boolean) => setOverrideTexture(v)} 
                        />
                    </div>
                    <div style={{ display: "flex", flexDirection: "column", marginBottom: "10rem" }}>
                        <span style={{marginBottom: "10rem"}}>Select Texture</span>
                        <Dropdown 
                            focusKey={FOCUS_DISABLED}
                            theme={DropdownStyle}
                            content={selections}
                        >
                            <DropdownToggle disabled={false}>
                                <div>{selectedTexture}</div>
                            </DropdownToggle>
                        </Dropdown>
                    </div>
                </InfoViewSectionMod>

                {/* Night Lighting Section */}
                <InfoViewSectionMod>
                    <Header title="Night Lighting"/>
                    <SliderRow bindingName="AmbientLight" min={0} max={15} />
                    <SliderRow bindingName="NightSkyLight" min={0} max={15} />
                    <SliderRow bindingName="MoonDirectionalLight" min={0} max={15} />
                    <SliderRow bindingName="MoonDiskSize" min={0} max={20} />
                    <SliderRow bindingName="MoonDiskIntensity" min={0} max={10} />
                    <SliderRow bindingName="NightLightTemperature" min={3500} max={10000} step={1} />
                    <SliderRow bindingName="MoonTemperature" min={3500} max={10000} step={1} />
                    <SliderRow bindingName="StarfieldEmmisionStrength" min={0} max={1} />
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

    return (
        <div style={{...colFlexStyle}}>
            <div style={{...row2ItemFlex, marginBottom: "5rem"}}>
                <span style={{ fontSize: "15rem" }}>{title ? title : bindingName}</span>
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
