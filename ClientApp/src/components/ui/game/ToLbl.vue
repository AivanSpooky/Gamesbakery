<template>
  <div :class="['to-lbl', className]">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'ToLbl',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: 'до' },
    },
  });
</script>
<style scoped>
  .to-lbl {
    background-color: var(--color-gift-accent);
    border: 6px solid var(--color-pink-border);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
    width: 120px;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-main);
    font-size: var(--font-size-main);
    font-weight: 400;
    text-align: center;
    width: 104px;
  }
</style>
