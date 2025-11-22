<template>
  <div :class="['table-price-lbl', className]">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'TablePriceLbl',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: '249' },
    },
  });
</script>
<style scoped>
  .table-price-lbl {
    background-color: var(--color-money-accent);
    border: 12px solid var(--color-yellow-border);
    border-radius: 40px;
    align-items: center;
    justify-content: center;
    display: flex;
    height: 85px;
    overflow: hidden;
    position: relative;
    flex: 2;
  }

  .text-wrapper {
    color: var(--color-text-secondary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    text-align: center;
    font-weight: 400;
    height: 38px;
    letter-spacing: 0;
    line-height: normal;
    text-align: center;
    white-space: nowrap;
    position: center;
    /*    top: 24px;*/
    /*    left: 65px;
    width: 90px;*/
  }
</style>
