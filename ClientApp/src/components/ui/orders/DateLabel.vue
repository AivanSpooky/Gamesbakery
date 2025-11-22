<template>
  <div :class="['date-label', className]">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'DateLabel',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: 'Дата выхода:' },
    },
  });
</script>
<style scoped>
  .date-label {
    background-color: var(--color-gift-accent);
    border: var(--border-notification) solid var(--color-pink-border);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 85px;
    flex: 1;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    text-align: center;
  }
</style>
