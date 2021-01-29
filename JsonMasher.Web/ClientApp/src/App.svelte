<script lang="ts">
    import Header from "./Header.svelte";
    import Examples from "./Examples.svelte";
    import InputArea from "./InputArea.svelte";
    import { examples } from "./examples";
    import { onMount } from "svelte";
    import Subtitle from "./Subtitle.svelte";
    import OutputArea from "./OutputArea.svelte";

    const localStorageKey = "program+input";

    let program = "";
    let input = "";
    let stdout = "";
    let stderr = "";
    let slurp = false;

    onMount(() => {
        const initialContent =
            JSON.parse(localStorage.getItem(localStorageKey)) || {};
        program = initialContent.program || examples[0].program;
        input = initialContent.input || examples[0].input;
        slurp = initialContent.slurp || false;
    });

    const onSelectExample = (example: any) => {
        input = example.input;
        program = example.program;
        slurp = false;
    };

    const run = async () => {
        const body = JSON.stringify({ program, input, slurp });
        localStorage.setItem(localStorageKey, body);
        const response = await fetch("jsonmasher", {
            headers: {
                Accept: "text/plain",
                "Content-Type": "application/json",
            },
            method: "POST",
            body,
        });
        const data = await response.json();
        stdout = data.stdOut;
        stderr = data.stdErr;
    };
</script>

<div class="container">
    <div class="header">
        <Header />
    </div>
    <div class="program">
        <Subtitle title="Program:" />
        <div class="programSettings">
            <Examples {examples} {onSelectExample} />
            <div/>
            <div>
                <input
                    name="slurp"
                    type="checkbox"
                    bind:checked={slurp}
                />
                <label for="slurp">Slurp (wrap input in array)</label>
                &nbsp;|&nbsp;
                <button on:click={run}>Run</button>
            </div>
        </div>
        <textarea bind:value={program} />
    </div>
    <div class="input">
        <InputArea title="Input:" bind:value={input} />
    </div>
    <div class="stdout">
        <OutputArea title={"Output:"} value={stdout} />
    </div>
    <div class="stderr">
        <OutputArea title={"Debugging:"} value={stderr} />
    </div>
</div>

<style lang="scss">
    .container {
        display: grid;
        padding: 10px;
        grid-gap: 10px;
        height: calc(100% - 20px);
        width: calc(100% - 20px);
        grid-template-columns: 1fr 1fr;
        grid-template-rows: auto 1fr 1fr;
        grid-template-areas: 
            "header header"
            "program stdout"
            "input stderr";
    }
    .header {
        grid-area: header;
    }
    .program {
        grid-area: program;
        display: grid;
        grid-template-rows: auto auto 1fr;
    }
    .programSettings {
        display: grid;
        grid-template-columns: auto 1fr auto;
    }
    .input {
        grid-area: input;
    }
    .stdout {
        grid-area: stdout;
    }
    .stderr {
        grid-area: stderr;
    }
    textarea {
        resize: none;
    }
</style>
