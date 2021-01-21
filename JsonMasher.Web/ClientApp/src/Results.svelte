<script lang="ts">
    import ContainerV from "./ContainerV.svelte";
    import SeparatorV from "./SeparatorV.svelte";
    import OutputArea from "./OutputArea.svelte";
    export let stdout: string;
    export let stderr: string;
</script>

<ContainerV>
    <OutputArea title={"Output:"} value={stdout} />
    <OutputArea title={"Debugging:"} value={stderr} />
    <SeparatorV />
</ContainerV>
