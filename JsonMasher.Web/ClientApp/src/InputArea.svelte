<script lang="ts">
    export let title: string;
    export let value: string;
</script>

<div class="subtitle">
    <h3>{title}</h3>
</div>
<textarea class="fillall" bind:value={value} />

<style>
    .subtitle {
        padding: 5px;
        background: lightyellow;
    }
    .fillall {
        width: 100%;
        height: 100%;
    }
</style>
