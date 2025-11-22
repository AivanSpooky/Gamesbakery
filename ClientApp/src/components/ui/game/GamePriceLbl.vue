<template>
  <div :class="['game-price-lbl', className]">
    <div class="text-wrapper">{{ gamePriceLblText }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'GamePriceLbl',
    props: {
      className: { type: String, default: '' },
      gamePriceLblText: { type: String, default: '249' },
    },
  });
</script>
<style scoped>
  .game-price-lbl {
    background-color: var(--color-money-accent);
    border: 12px solid var(--color-yellow-border);
    border-radius: var(--radius-main);
    display: flex;
    height: 90px;
    overflow: hidden;
    position: relative;
    align-items: center;
    justify-content: center;
    width: 200px;
  }

  .text-wrapper {
    color: var(--color-text-secondary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    font-weight: 400;
    height: 40px;
    letter-spacing: 0;
    line-height: normal;
    /*    margin-left: 50px;
    margin-top: 26px;*/
    text-align: center;
    white-space: nowrap;
    width: 104px;
  }
</style>
