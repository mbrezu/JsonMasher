<script>
    export let examples;
    export let onSelectExample;
    let selected;
</script>

<div class="fixed">
    <label for="examples">Examples:&nbsp;</label>
    <!-- svelte-ignore a11y-no-onchange -->
    <select bind:value={selected} name="examples" on:change={() => onSelectExample(selected)}>
        {#each examples as example (example.title)}
            <option value={example}>
                {example.title}
            </option>
        {/each}
    </select>
</div>

<style>
    .fixed {
        flex-grow: 0;
    }
</style>