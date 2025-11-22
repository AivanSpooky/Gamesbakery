<template>
  <div class="div-wrapper neon-button" @click="$emit('filter')">
    <div class="text-wrapper-7">Фильтровать</div>
  </div>
</template>
<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'FilterButton',
    emits: ['filter'],
  });
</script>
<style scoped>
  .div-wrapper {
    background-color: var(--color-background-accent);
    border: var(--border-interactive) solid var(--color-border-1);
    border-radius: var(--radius-main);
    display: flex;
    height: 81px;
    overflow: hidden;
    width: 511px;
    cursor: pointer;
    align-items: center;
    justify-content: center;
  }

  .text-wrapper-7 {
    color: var(--color-text-primary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    font-weight: 400;
    letter-spacing: 0;
    line-height: normal;
    text-align: center;
    white-space: nowrap;
    width: 198px;
  }
</style>
