<template>
  <div :class="['game-lbl', className]">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'GameLbl',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: 'Game 1' },
    },
  });
</script>
<style scoped>
  .game-lbl {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 120px;
    left: 0;
    top: -1px;
    width: 100%;
    max-width: 507px;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-main);
    font-size: var(--font-size-main);
    font-weight: 400;
    text-align: center;
  }
</style>
