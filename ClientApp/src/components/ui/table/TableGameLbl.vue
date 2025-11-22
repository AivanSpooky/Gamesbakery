<template>
  <div :class="['table-game-lbl', className]">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'TableGameLbl',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: 'Game 1' },
    },
  });
</script>
<style scoped>
  .table-game-lbl {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 85px;
    overflow: hidden;
    flex: 3;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    text-align: center;
  }
</style>
