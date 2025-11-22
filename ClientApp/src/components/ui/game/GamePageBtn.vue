<template>
  <div :class="['game-page-btn', className]">
    <div class="text-wrapper">{{ gamePageBtnText }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'GamePageBtn',
    props: {
      className: { type: String, default: '' },
      gamePageBtnText: { type: String, default: 'Game 1' },
    },
  });
</script>
<style scoped>
  .game-page-btn {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-button);
    display: flex;
    align-items: center;
    justify-content: flex-start;
    height: 90px;
    flex-grow: 1;
    overflow: hidden;
    padding-left: 29px;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-main);
    font-size: var(--font-size-main);
    font-weight: 400;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }
</style>
