<template>
  <div :class="['status-value', className]">
    <div class="text-wrapper">{{ text }}</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'StatusValue',
    props: {
      className: { type: String, default: '' },
      text: { type: String, default: 'оплачен' },
    },
  });
</script>
<style scoped>
  .status-value {
    background-color: var(--color-teal-accent);
    border: var(--border-notification) solid var(--color-teal-border);
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
