<script lang="ts">
    import ContainerH from "./ContainerH.svelte";
    import ContainerV from "./ContainerV.svelte";
    import SeparatorH from "./SeparatorH.svelte";
    import Header from "./Header.svelte";
    import SeparatorV from "./SeparatorV.svelte";
    import Examples from "./Examples.svelte";
    import InputArea from "./InputArea.svelte";
    import Results from "./Results.svelte";
    import { examples } from "./examples";
    import { onMount } from "svelte";

    const localStorageKey = "program+input";

    let program = "";
    let input = "";
    let stdout = "";
    let stderr = "";
    let slurp = false;

    onMount(() => {
        const initialContent = JSON.parse(localStorage.getItem(localStorageKey)) || {};
        program = initialContent.program || examples[0].program;
        input = initialContent.input || examples[0].input;
        slurp = initialContent.slurp || false;
    });

    const selectExample = (example: any) => {
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

<ContainerH>
    <SeparatorH />
    <ContainerV>
        <SeparatorV/>
        <Header />
        <ContainerH>
            <ContainerV>
                <div class="subtitle">
                    <h3>Program:</h3>
                </div>
                <div class="flexRow">
                    <Examples {examples} onSelectExample={selectExample} />
                    <div class="filler" />
                    <div class="fixed">
                        <input name="slurp" type="checkbox" bind:checked={slurp} />
                        <label for="slurp">Slurp (wrap input in array)</label>
                        &nbsp;|&nbsp;
                        <button on:click={run}>Run</button>
                    </div>
                </div>
                <textarea class="fillall" bind:value={program} />
                <InputArea title="Input:" bind:value={input} />
                <SeparatorV />
            </ContainerV>
            <SeparatorH />
            <Results {stdout} {stderr} />
        </ContainerH>
    </ContainerV>
    <SeparatorH />
</ContainerH>

<style>
    .subtitle {
        padding: 5px;
        background: lightyellow;
    }
    .flexRow {
        display: flex;
        flex-direction: row;
        align-items: center;
        margin-top: 5px;
        margin-bottom: 5px;
    }
    .fillall {
        width: 100%;
        height: 100%;
    }

    .filler {
        flex-grow: 1;
    }

    .fixed {
        flex-grow: 0;
    }
</style>
