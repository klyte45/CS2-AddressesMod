import {
  VanillaSideWindow,
  VanillaWindowActionButton,
  VanillaWindowSectionDescription,
  VanillaWindowSectionIconNotification,
  VanillaWindowSectionMainKeyValue,
  VanillaWindowSectionProgressBar,
  VanillaWindowSectionSubKeyValue,
  VanillaWindowToggleButton
} from "@klyte45/euis-components";

export default function Root(props) {
  return <>
    <VanillaSideWindow
      title={{
        value: "Test Addresesses CS2",
        isInput: false,
        onClose: () => props.selfUnselect(),
        iconSrc: `coui://adr.k45/UI/images/ADR.svg`
      }}
      sections={[
        (x) => <VanillaWindowSectionIconNotification key={x} icon="Media/Gamepad/PS/PS_triangle.svg" description="Triangle!" />,
        (x) => <VanillaWindowSectionDescription key={x} paragraphs={[
          "Paragraph test",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Mauris in nulla lorem. Donec neque nun...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",
          "Lorem ipsum dolor sit amet, consectetu...",

        ]}
          effects={[
            { icon: "Media/Gamepad/PS/PS_X.svg", description: "Cross!!!!" },
            { icon: "Media/Gamepad/PS/PS_O.svg", description: "Circle!!!!" },
          ]} />,
        (x) => <VanillaWindowSectionMainKeyValue key={x} keyName="Teste de Key" value="Value!!!!" />,
        (x) => <VanillaWindowSectionMainKeyValue key={x} keyName="Teste de Key c/ subkeys" value="Values!!!!" subRows={[
          (k) => <VanillaWindowSectionSubKeyValue key={k} keyName="Keyname 1" value="Value 1" />,
          (k) => <VanillaWindowSectionSubKeyValue key={k} keyName="Keyname 2" value="Value 2" />,
          (k) => <VanillaWindowSectionSubKeyValue key={k} keyName="Keyname 3" value="BTN!" onClick={() => { }} />,
        ]} />,
        (x) => <VanillaWindowSectionProgressBar key={x} title="Progress bar" maximum={4500} value={3250} subRows={[
          (k) => <VanillaWindowSectionSubKeyValue key={k} keyName="Keyname 4" value="Value 7" />,
          (k) => <VanillaWindowSectionSubKeyValue key={k} keyName="Keyname 5" value="Value 5" />,
          (k) => <VanillaWindowSectionSubKeyValue key={k} keyName="Keyname 6" value={<b>BTN!</b>} />,
        ]} />,
      ]}
      leftFooterButtons={[
        (k) => <VanillaWindowActionButton key={k} iconURL="Media/Gamepad/PS/PS_L2.svg" onClick={() => { }} tooltip="L2 Test" />,
        (k) => <VanillaWindowToggleButton key={k} iconURL="Media/Gamepad/PS/PS_L1.svg" onToggle={() => { }} tooltip="L1 Test" stateValue={false} />,
      ]}

      rightFooterButtons={[
        (k) => <VanillaWindowActionButton key={k} iconURL="Media/Gamepad/PS/PS_R2.svg" onClick={() => { }} tooltip="R2 Test" />,
        (k) => <VanillaWindowToggleButton key={k} iconURL="Media/Gamepad/PS/PS_R1.svg" onToggle={() => { }} tooltip="R1 Test" stateValue={true} />,
      ]}
    />
  </>;
}
