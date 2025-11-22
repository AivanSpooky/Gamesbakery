<template>
  <div :class="['publisher', className]">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'Publisher',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: 'tinyBuild' },
    },
  });
</script>
<style scoped>
  .publisher {
    background-color: var(--color-teal-accent);
    border: 6px solid var(--color-teal-border);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 90px;
    overflow: hidden;
    position: relative;
    width: 229px;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-medium);
    font-weight: 400;
    height: 28px;
    letter-spacing: 0;
    line-height: normal;
    text-align: center;
    white-space: nowrap;
    width: 92px;
  }
</style>
