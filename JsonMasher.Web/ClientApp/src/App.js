import React, { useState } from 'react';

import './custom.css';

const localStorageKey = 'program+input';

const initialContent = JSON.parse(localStorage.getItem(localStorageKey)) || {};

const examples = [
  `identity
------
.
------
1 2 3 4 5
`,
  `construct array
------
[.[] |. + 2]
------
[1, 2, 3, 4, 5]
`,
`construct object
------
.[] | { a: ., b: . + 2 }
------
[1, 2, 3, 4, 5]
`,
`range
------
range(10) | . * .
------
null
`,
`limit
------
limit(3; range(10))
------
null
`,
`if-then-else
------
.[] | if . >= 2 and . <= 4 then . else empty end
------
[1, 2, 3, 4, 5]
`,
`function definition
------
def map(x): .[] | x;
def select(x): if x then . else empty end;

[map(select(. > 2))]
------
[1, 2, 3, 4, 5]
`
];

const parsedExamples = 
  examples.map(value => {
    const parts = value.split("------").map(str => str.trim());
    return {
      title: parts[0],
      program: parts[1],
      input: parts[2]
    }
  });

const App = (_props) => {
  const [program, setProgram] = useState(initialContent.program || ".");
  const [input, setInput] = useState(initialContent.input || "1 2 3 4 5");
  const [output, setOutput] = useState("");
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
      const data = await response.text();
      setOutput(data);
    }
    runImpl();
  };
  const selectExample = (e) => {
    var selection = parsedExamples.filter(x => x.title === e.target.value);
    if (selection.length === 0) {
      return;
    }
    setProgram(selection[0].program);
    setInput(selection[0].input);
  };
  const projectUrl = "https://github.com/mbrezu/JsonMasher";
  return (
    <div className="containerh">
      <div className="separatorh" />
      <div className="containerv">
        <div className="separatorv" />
        <div className="title">
          <a href={projectUrl}><h1>Json Masher</h1></a>
          <a href={projectUrl}>{projectUrl}</a> - a .NET 5 implementation of <a href="https://stedolan.github.io/jq/">jq</a>
        </div>
        <div className="containerh">
          <div className="containerv">
            <div className="containerv">
              <div className="subtitle">
                <h3>Program:</h3>
              </div>
              <div className="flexRow">
                <div className="fixed">
                  <label>Examples:&nbsp;</label>
                  <select onChange={selectExample}>
                    {parsedExamples.map(ex => <option value={ex.title}>{ex.title}</option>)}
                  </select>
                </div>
                <div className="filler" />
                <div className="fixed">
                  <input type="checkbox" checked={slurp} onChange={() => setSlurp(!slurp)} /> <label>Wrap input in array</label>
                  &nbsp;|&nbsp;
                  <button onClick={run}>Run</button>
                </div>
              </div>
              <textarea className="containerv" onChange={e => setProgram(e.target.value)} value={program} />
            </div>
            <div className="containerv">
              <div className="subtitle">
                <h3>Input:</h3>
              </div>
              <textarea className="containerv" onChange={e => setInput(e.target.value)} value={input} />
            </div>
            <div className="separatorv" />
          </div>
          <div className="separatorh" />
          <div className="containerv">
            <div className="containerv">
              <div className="subtitle">
                <h3>Output:</h3>
              </div>
              <textarea className="containerv" readOnly value={output} />
            </div>
            <div className="separatorv" />
          </div>
        </div>
      </div>
      <div className="separatorh" />
    </div>
  );
};

export default App;