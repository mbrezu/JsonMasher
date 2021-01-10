import React, { useState } from 'react';
import { examples } from './examples';

import './custom.css';

const localStorageKey = 'program+input';

const initialContent = JSON.parse(localStorage.getItem(localStorageKey)) || {};

const Header = (_props) => {
  const projectUrl = "https://github.com/mbrezu/JsonMasher";
  return (<div className="title">
    <a href={projectUrl}><h1>Json Masher</h1></a>
    <a href={projectUrl}>{projectUrl}</a> - a .NET 5 implementation of <a href="https://stedolan.github.io/jq/">jq</a>
  </div>);
};

const Examples = ({ examples, onSelectExample }) => (<div className="fixed">
  <label>Examples:&nbsp;</label>
  <select onChange={onSelectExample}>
    {examples.map((ex, idx) => <option key={idx} value={ex.title}>{ex.title}</option>)}
  </select>
</div>);

const InputArea = ({ title, onChange, value }) => (<>
  <div className="subtitle">
    <h3>{title}</h3>
  </div>
  <textarea className="fillall" onChange={onChange} value={value} />
</>);

const Results = ({ stdout, stderr }) => (<div className="containerv">
  <OutputArea title={"Output:"} value={stdout}/>
  <OutputArea title={"Debugging:"} value={stderr}/>
  <div className="separatorv" />
</div>);

const OutputArea = ({ title, value}) => (<>
  <div className="subtitle">
    <h3>{title}</h3>
  </div>
  <textarea className="fillall" readOnly value={value} />
</>);

const App = (_props) => {
  const [program, setProgram] = useState(initialContent.program || ".");
  const [input, setInput] = useState(initialContent.input || "1 2 3 4 5");
  const [stdout, setStdOut] = useState("");
  const [stderr, setStdErr] = useState("");
  const [slurp, setSlurp] = useState(initialContent.slurp || false);
  const run = () => {
    async function runImpl() {
      const body = JSON.stringify({ program, input, slurp });
      localStorage.setItem(localStorageKey, body);
      const response = await fetch(
        'jsonmasher',
        {
          headers: {
            'Accept': 'text/plain',
            'Content-Type': 'application/json'
          },
          method: "POST",
          body
        });
      const data = await response.json();
      setStdOut(data.stdOut);
      setStdErr(data.stdErr);
    }
    runImpl();
  };
  const selectExample = (e) => {
    var selection = examples.filter(x => x.title === e.target.value);
    if (selection.length === 0) {
      return;
    }
    setSlurp(false);
    setProgram(selection[0].program);
    setInput(selection[0].input);
  };
  return (
    <div className="containerh">
      <div className="separatorh" />
      <div className="containerv">
        <div className="separatorv" />
        <Header/>
        <div className="containerh">
          <div className="containerv">
            <div className="subtitle">
              <h3>Program:</h3>
            </div>
            <div className="flexRow">
              <Examples examples={examples} onSelectExample={selectExample}/>
              <div className="filler" />
              <div className="fixed">
                <input type="checkbox" checked={slurp} onChange={() => setSlurp(!slurp)} /> <label>Slurp (wrap input in array)</label>
                &nbsp;|&nbsp;
                <button onClick={run}>Run</button>
              </div>
            </div>
            <textarea className="fillall" onChange={e => setProgram(e.target.value)} value={program} />
            <InputArea title={"Input:"} onChange={e => setInput(e.target.value)} value={input}/>
            <div className="separatorv" />
          </div>
          <div className="separatorh" />
          <Results stdout={stdout} stderr={stderr} />
        </div>
      </div>
      <div className="separatorh" />
    </div>
  );
};

export default App;