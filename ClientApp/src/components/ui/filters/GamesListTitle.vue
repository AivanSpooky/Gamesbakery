<template>
  <div class="div-2">
    <div class="rectangle-8" />
    <div class="text-wrapper-8">Список игр</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'GamesListTitle',
  });
</script>
<style scoped>
  .div-2 {
    height: 120px;
    position: relative;
    width: 511px;
  }

  .rectangle-8 {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    height: 120px;
    width: 507px;
  }

  .text-wrapper-8 {
    color: var(--color-text-primary);
    font-family: var(--font-main);
    font-size: var(--font-size-large);
    font-weight: 400;
    letter-spacing: 0;
    line-height: normal;
    position: absolute;
    text-align: center;
    top: 28px;
    left: 76px;
    white-space: nowrap;
  }
</style>
