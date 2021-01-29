<script lang="ts">
    import Subtitle from "./Subtitle.svelte";

    export let title: string;
    export let value: string;
</script>

<div class="container">
    <Subtitle {title}/>
    <textarea readOnly {value} />
</div>

<style>
    .container {
        display: grid;
        width: 100%;
        height: 100%;
        grid-template-rows: auto 1fr;
    }
    textarea {
        resize: none;
    }
</style>
