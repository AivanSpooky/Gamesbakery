<template>
  <div :class="['table-seller-lbl', className]">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';

  export default defineComponent({
    name: 'TableSellerLbl',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: 'Seller 1' },
    },
  });
</script>

<style scoped>
  .table-seller-lbl {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 85px;
    width: 330px;
    overflow: hidden;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    text-align: center;
  }
</style>
