<template>
  <div class="gift-key-add-example">
    <div class="layout flex items-center justify-center">
      <button class="add-button" @click="$emit('add')">Добавить ключ</button>
    </div>
  </div>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    name: 'GiftKeyAddExample',
    emits: ['add'],
  });
</script>

<style scoped>
  .gift-key-add-example {
    display: flex;
    height: 99px;
    position: relative;
    width: 100%;
  }

  .layout {
    align-items: center;
    display: flex;
    gap: 10px;
    height: 85px;
    justify-content: center;
    padding: 0 30px;
    position: relative;
    width: 100%;
  }

  .add-button {
    background-color: var(--color-background-border);
    color: var(--color-text-secondary);
    font-family: var(--font-additional);
    font-size: var(--font-size-main);
    padding: 20px 40px;
    border-radius: var(--radius-main);
    border: none;
    cursor: pointer;
    white-space: nowrap;
  }
</style>
