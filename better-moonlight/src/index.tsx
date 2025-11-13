import { ModRegistrar } from "cs2/modding";
import { HelloWorldComponent } from "mods/hello-world";
import { FloatingButton } from "cs2/ui";
import { getModule } from "cs2/modding";
import MoonSvgSrc from './res/CarvedMoon.svg'
import { Main } from "main";
import { useBinding } from "utils/bindings";

const register: ModRegistrar = (moduleRegistry) => {

    moduleRegistry.append('GameTopLeft', BetterMoonLightButton);
    moduleRegistry.append('Game', Main)
}

const BetterMoonLightButton = () => {

    const DescriptionTooltip = getModule("game-ui/common/tooltip/description-tooltip/description-tooltip.tsx", "DescriptionTooltip");

    const [showSetting, setShowSetting] = useBinding("ShowSetting")

    return (
        <DescriptionTooltip
            title="Better MoonLight"
            description="Better MoonLight"
        >
            <FloatingButton
                src={MoonSvgSrc}
                onClick={() => setShowSetting(!showSetting)}
            />
        </DescriptionTooltip>
    )
}


export default register;