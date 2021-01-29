<script lang="ts">
    import Subtitle from "./Subtitle.svelte";

    export let title: string;
    export let value: string;
</script>

<div class="container">
    <Subtitle {title}/>
    <textarea bind:value={value} />
</div>

<style>
    .container {
        height: 100%;
        width: 100%;
        display: grid;
        grid-template-rows: auto 1fr;
    }
    textarea {
        resize: none;
    }
</style>
