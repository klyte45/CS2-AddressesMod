///<reference path="euis.d.ts" />
import "#styles/main.scss"

export default function Root(props) {
  return <>
    <h1>ON!</h1>
    <button onClick={() => location.reload()}>REFRESH PAGE</button>
  </>;
}
