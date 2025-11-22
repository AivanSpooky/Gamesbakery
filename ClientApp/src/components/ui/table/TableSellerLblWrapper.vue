<template>
  <div :class="['table-seller-lbl-wrapper', className]">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'TableSellerLblWrapper',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: '' },
    },
  });
</script>
<style scoped>
  .table-seller-lbl-wrapper {
    background-color: var(--color-teal-accent);
    border: var(--border-interactive) solid var(--color-teal-border);
    border-radius: var(--radius-main);
    display: flex;
    align-items: center;
    justify-content: center;
    height: 85px;
    overflow: hidden;
    position: relative;
    flex: 1;
  }

  .text-wrapper {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    text-align: center;
  }
</style>
