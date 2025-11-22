<template>
  <div :class="['price-value', className]">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'PriceValue',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: '498' },
    },
  });
</script>
<style scoped>
  .price-value {
    background-color: var(--color-money-accent);
    border: var(--border-notification) solid var(--color-yellow-border);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 85px;
    flex: 1;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-secondary);
    font-family: var(--font-additional);
    font-size: var(--font-size-medium);
    text-align: center;
  }
</style>
