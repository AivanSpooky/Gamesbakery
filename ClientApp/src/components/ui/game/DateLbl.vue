<template>
  <div :class="['date-lbl', className]">
    <div class="text-wrapper">Дата выхода:</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'DateLbl',
    props: {
      className: { type: String, default: '' },
    },
  });
</script>
<style scoped>
  .date-lbl {
    background-color: var(--color-purple-accent);
    border: 6px solid var(--color-pink-border);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
    overflow: hidden;
    position: relative;
    width: 253px;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-main);
    font-size: var(--font-size-main);
    font-weight: 400;
    height: 32px;
    letter-spacing: 0;
    line-height: normal;
    text-align: center;
    white-space: nowrap;
    width: 212px;
  }
</style>
