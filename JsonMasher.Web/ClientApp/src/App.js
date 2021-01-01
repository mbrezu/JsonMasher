import React, { useState } from 'react';

import './custom.css'

const App = (_props) => {
  const [program, setProgram] = useState("");
  const [input, setInput] = useState("");
  const [output, setOutput] = useState("");
  return (
    <div className="containerh">
      <div className="separatorh" />
      <div className="containerv">
        <div className="separatorv" />
        <div className="title">
          <h1>Json Masher</h1>
        </div>
        <div className="containerh">
          <div className="containerv">
            <div className="containerv">
              <div className="subtitle">
                <h3>Program:</h3>
              </div>
              <div>
                <input type="checkbox" /> <label>Input in array</label>
                &nbsp;|&nbsp;
                <button>Run</button>
              </div>
              <textarea className="containerv" onChange={e => setProgram(e.target.value)}>{program}</textarea>
            </div>
            <div className="containerv">
              <div className="subtitle">
                <h3>Input:</h3>
              </div>
              <textarea className="containerv" onChange={e => setInput(e.target.value)}>{input}</textarea>
            </div>
            <div className="separatorv" />
          </div>
          <div className="separatorh" />
          <div className="containerv">
            <div className="containerv">
              <div className="subtitle">
                <h3>Output:</h3>
              </div>
              <pre class="fillall">
                {output}
              </pre>
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