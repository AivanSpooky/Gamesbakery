<template>
  <div :class="['order-item-seller', className]">
    <div class="text-wrapper">{{ seller }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'OrderItemSeller',
    props: {
      className: { type: String, default: '' },
      seller: { type: String, default: 'Seller-1' },
    },
  });
</script>
<style scoped>
  .order-item-seller {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    height: 90px;
    width: 944px;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    font-weight: 400;
    padding-left: 33px;
  }
</style>
