<script lang="ts">
    import Subtitle from "./Subtitle.svelte";

    export let title: string;
    export let value: string;
</script>

<Subtitle {title}/>
<textarea readOnly {value} />

<style>
    textarea {
        width: 100%;
        height: 100%;
    }
</style>
